using TMPro;
using UnityEngine;

namespace Blackletter
{
    public class IDEWindow : MonoBehaviour
    {
        [Header("References")]
        public BlackletterScript script;

        [Tooltip("Assign a TMP_InputField (uGUI) here.")]
        public TMP_InputField inputField;

        private void Awake()
        {
            // Optional: auto-load script text into the field at runtime.
            LoadFromScript();
        }


        public void SaveToScript()
        {
            if (script == null)
            {
                Debug.LogError($"{nameof(IDEWindow)}: No script assigned.", this);
                return;
            }

            if (inputField == null)
            {
                Debug.LogError($"{nameof(IDEWindow)}: No TMP_InputField assigned.", this);
                return;
            }

            script.SourceText = inputField.text;

#if UNITY_EDITOR
            // Persist changes to the asset when running in editor (and in edit mode).
            UnityEditor.EditorUtility.SetDirty(script);
            UnityEditor.AssetDatabase.SaveAssets();
#endif
        }

        public void LoadFromScript()
        {
            if (script == null || inputField == null) return;
            inputField.text = script.SourceText ?? "";
        }

        public void SaveAndCompile()
        {
            Debug.Log("Saving...");
            SaveToScript();
            Compile();
        }


        public void Compile()
        {
            if (script == null)
            {
                Debug.LogError($"{nameof(IDEWindow)}: No script assigned.", this);
                return;
            }

            script.Compile();

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(script);
            UnityEditor.AssetDatabase.SaveAssets();
#endif
        }
    }
}
