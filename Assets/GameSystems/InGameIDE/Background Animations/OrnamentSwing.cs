using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PendulumSim2D : MonoBehaviour
{
    public enum Plane2D { XY, XZ }

    [Serializable]
    public struct Pendulum
    {
        [Header("Scene references (optional)")]
        public Transform pivot;      // If null, uses this component's transform
        public Transform bob;        // If null, nothing is moved (still simulated)

        [Header("Parameters")]
        [Min(0.01f)] public float length;   // meters
        public float gravity;               // m/s^2 (positive number, applied downward in chosen plane)
        [Min(0f)] public float damping;     // 0 = no damping, higher = more damping

        [Header("Initial state (radians)")]
        public float initialAngle;          // angle from "down" direction, radians
        public float initialAngularVelocity; // rad/s

        [Header("Runtime state (read-only-ish)")]
        [SerializeField] public float angle;          // radians
        [SerializeField] public float angularVelocity; // rad/s
    }

    [Header("2D plane + visualization")]
    [SerializeField] private Plane2D plane = Plane2D.XY;
    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private float bobRadiusGizmo = 0.05f;

    [Header("Internal time system")]
    [Tooltip("Simulation ticks per second (independent of Unity physics).")]
    [SerializeField] private int tickRate = 120;

    [Tooltip("Scales how fast the simulation runs (1 = real time).")]
    [SerializeField] private float timeScale = 1f;

    [Tooltip("If true, stops simulation time (you can still step manually).")]
    [SerializeField] private bool paused = false;

    [Tooltip("Prevents spiral-of-death when frame time spikes.")]
    [SerializeField] private int maxTicksPerFrame = 8;

    [Header("Pendulums")]
    [SerializeField] private Pendulum[] pendulums;

    // Internal clock
    private double _accumulator;
    private double _simTime;
    private double _dt;

    // Optional: expose sim time
    public double SimTimeSeconds => _simTime;

    private void Awake()
    {
        tickRate = Mathf.Max(1, tickRate);
        _dt = 1.0 / tickRate;

        // Initialize runtime state from initial values
        for (int i = 0; i < pendulums.Length; i++)
        {
            var p = pendulums[i];
            p.length = Mathf.Max(0.01f, p.length <= 0 ? 1f : p.length);
            if (p.gravity <= 0f) p.gravity = 9.81f;

            p.angle = p.initialAngle;
            p.angularVelocity = p.initialAngularVelocity;
            pendulums[i] = p;

            ApplyTransform(i); // place bobs immediately
        }
    }

    private void OnValidate()
    {
        tickRate = Mathf.Max(1, tickRate);
        maxTicksPerFrame = Mathf.Clamp(maxTicksPerFrame, 1, 10_000);
        timeScale = Mathf.Max(0f, timeScale);
    }

    private void Update()
    {
        if (paused) return;

        // Unity's frame delta -> accumulate into our fixed-step sim time
        double frameDt = (double)Time.unscaledDeltaTime * (double)timeScale;
        _accumulator += frameDt;

        int ticks = 0;
        while (_accumulator >= _dt && ticks < maxTicksPerFrame)
        {
            Step((float)_dt);
            _accumulator -= _dt;
            _simTime += _dt;
            ticks++;
        }

        // If we hit max ticks, drop remaining time to avoid runaway
        if (ticks == maxTicksPerFrame)
            _accumulator = 0.0;
    }

    /// <summary>Manually advance simulation by exactly one tick (useful when paused).</summary>
    public void StepOneTick()
    {
        Step((float)_dt);
        _simTime += _dt;
    }

    /// <summary>Reset all pendulums to their initial state.</summary>
    public void ResetAll()
    {
        _accumulator = 0.0;
        _simTime = 0.0;

        for (int i = 0; i < pendulums.Length; i++)
        {
            var p = pendulums[i];
            p.angle = p.initialAngle;
            p.angularVelocity = p.initialAngularVelocity;
            pendulums[i] = p;
            ApplyTransform(i);
        }
    }

    // --- Simulation core ---

    private void Step(float dt)
    {
        for (int i = 0; i < pendulums.Length; i++)
        {
            var p = pendulums[i];

            // Handle missing/invalid parameters safely
            float L = Mathf.Max(0.01f, p.length);
            float g = Mathf.Max(0f, p.gravity);
            float d = Mathf.Max(0f, p.damping);

            // State: theta (angle), omega (angular velocity)
            float theta = p.angle;
            float omega = p.angularVelocity;

            // RK4 integration for:
            // dtheta/dt = omega
            // domega/dt = -(g/L) * sin(theta) - d * omega
            RK4(theta, omega, dt, g, L, d, out theta, out omega);

            p.angle = theta;
            p.angularVelocity = omega;
            pendulums[i] = p;

            ApplyTransform(i);
        }
    }

    private static void RK4(
        float theta, float omega, float dt,
        float g, float L, float damping,
        out float thetaOut, out float omegaOut)
    {
        // Derivatives
        static float dTheta(float om) => om;
        static float dOmega(float th, float om, float g_, float L_, float d_) =>
            -(g_ / L_) * Mathf.Sin(th) - d_ * om;

        float k1_th = dTheta(omega);
        float k1_om = dOmega(theta, omega, g, L, damping);

        float th2 = theta + 0.5f * dt * k1_th;
        float om2 = omega + 0.5f * dt * k1_om;
        float k2_th = dTheta(om2);
        float k2_om = dOmega(th2, om2, g, L, damping);

        float th3 = theta + 0.5f * dt * k2_th;
        float om3 = omega + 0.5f * dt * k2_om;
        float k3_th = dTheta(om3);
        float k3_om = dOmega(th3, om3, g, L, damping);

        float th4 = theta + dt * k3_th;
        float om4 = omega + dt * k3_om;
        float k4_th = dTheta(om4);
        float k4_om = dOmega(th4, om4, g, L, damping);

        thetaOut = theta + (dt / 6f) * (k1_th + 2f * k2_th + 2f * k3_th + k4_th);
        omegaOut = omega + (dt / 6f) * (k1_om + 2f * k2_om + 2f * k3_om + k4_om);
    }

    // --- Mapping angle -> 2D position in a plane ---

    private void ApplyTransform(int index)
    {
        var p = pendulums[index];
        if (p.bob == null) return;

        Transform pivotT = p.pivot != null ? p.pivot : transform;

        // Define "down" direction in the chosen plane.
        // Angle is measured from down; theta=0 means straight down.
        Vector3 down, right;
        switch (plane)
        {
            case Plane2D.XZ:
                down = Vector3.down;     // -Y
                right = Vector3.right;   // +X
                break;
            default: // XY
                down = Vector3.down;     // -Y
                right = Vector3.right;   // +X
                break;
        }

        // Rotate 'down' toward 'right' by theta within the plane.
        // In XY: movement in X/Y (Z fixed)
        // In XZ: movement in X/Z (Y fixed) -> we remap down/right below.
        Vector3 offset;
        float s = Mathf.Sin(p.angle);
        float c = Mathf.Cos(p.angle);

        if (plane == Plane2D.XY)
        {
            // In XY, "down" = (0,-1,0), "right"=(1,0,0)
            offset = (right * s + down * c) * p.length;
        }
        else // XZ
        {
            // For XZ, we want "down in plane" to mean negative Z (commonly),
            // but we still treat gravity as downward visually.
            // We'll interpret pendulum swinging in XZ with Z as vertical-down in-plane.
            Vector3 downXZ = Vector3.back;   // -Z is "down" in the XZ plane
            Vector3 rightXZ = Vector3.right; // +X
            offset = (rightXZ * s + downXZ * c) * p.length;
        }

        p.bob.position = pivotT.position + offset;
    }

    // --- Gizmos ---

    private void OnDrawGizmos()
    {
        if (!drawGizmos || pendulums == null) return;

        for (int i = 0; i < pendulums.Length; i++)
        {
            var p = pendulums[i];
            Transform pivotT = p.pivot != null ? p.pivot : transform;

            Vector3 pivotPos = pivotT.position;

            // Compute current bob position the same way ApplyTransform does (without moving it).
            float theta = Application.isPlaying ? p.angle : p.initialAngle;
            float L = Mathf.Max(0.01f, p.length <= 0 ? 1f : p.length);

            Vector3 bobPos;
            if (plane == Plane2D.XY)
            {
                Vector3 down = Vector3.down;
                Vector3 right = Vector3.right;
                bobPos = pivotPos + (right * Mathf.Sin(theta) + down * Mathf.Cos(theta)) * L;
            }
            else
            {
                Vector3 downXZ = Vector3.back;
                Vector3 rightXZ = Vector3.right;
                bobPos = pivotPos + (rightXZ * Mathf.Sin(theta) + downXZ * Mathf.Cos(theta)) * L;
            }

            Gizmos.DrawLine(pivotPos, bobPos);
            Gizmos.DrawSphere(bobPos, bobRadiusGizmo);
        }
    }
}
