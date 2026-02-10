using TMPro;
using UnityEngine;

namespace Blackletter
{
    public sealed class IDEWindow : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BlackletterScript script;
        [SerializeField] private TMP_InputField inputField;

        [Header("Execution")]
        [Tooltip("Target GameObject the script operates on (move, etc).")]
        [SerializeField] private GameObject executionTarget;

        private void Awake()
        {
            LoadFromScript();
        }

        // -------------------------
        // UI → Script
        // -------------------------

        public void SaveToScript()
        {
            if (!ValidateScriptAndField()) return;

            script.Source = inputField.text;
        }

        public void LoadFromScript()
        {
            if (script == null || inputField == null) return;
            inputField.text = script.Source ?? string.Empty;
        }

        // -------------------------
        // Compile
        // -------------------------

        public void Compile()
        {
            if (script == null)
            {
                Debug.LogError("IDEWindow: No script assigned.", this);
                return;
            }

            script.tokens = Lexer.Tokenize(script.Source);
            script.isDirty = false;

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(script);
#endif
        }

        // -------------------------
        // Run
        // -------------------------

        public void Run()
        {
            if (script == null)
            {
                Debug.LogError("IDEWindow: No script assigned.", this);
                return;
            }

            if (script.tokens == null || script.isDirty)
            {
                Compile();
            }

            Interpreter.Execute(script.tokens, executionTarget);
        }

        // -------------------------
        // Convenience (Buttons)
        // -------------------------

        public void SaveAndCompile()
        {
            SaveToScript();
            Compile();
        }

        public void SaveCompileAndRun()
        {
            SaveToScript();
            Compile();
            Run();
        }

        // -------------------------
        // Validation
        // -------------------------

        private bool ValidateScriptAndField()
        {
            if (script == null)
            {
                Debug.LogError("IDEWindow: No BlackletterScript assigned.", this);
                return false;
            }

            if (inputField == null)
            {
                Debug.LogError("IDEWindow: No TMP_InputField assigned.", this);
                return false;
            }

            return true;
        }
    }
}
