Shader "Custom/EnemyInstanced"
{
    Properties
    {
        _Color ("Color", Color) = (1,0,0,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma instancing_options procedural:setup

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Enemy
            {
                float3 position;
                float3 velocity;
            };

            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            StructuredBuffer<Enemy> enemyBuffer;
            float _SphereRadius;
            #endif

            float4 _Color;

            void setup()
            {
                #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                Enemy enemy = enemyBuffer[unity_InstanceID];
                float r = _SphereRadius;
                
                unity_ObjectToWorld = float4x4(
                    r, 0, 0, enemy.position.x,
                    0, r, 0, enemy.position.y + r,
                    0, 0, r, enemy.position.z,
                    0, 0, 0, 1
                );
                unity_WorldToObject = unity_ObjectToWorld;
                #endif
            }

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return _Color;
            }
            ENDCG
        }
    }
}
