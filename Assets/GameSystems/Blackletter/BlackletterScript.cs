using UnityEngine;
using System.Collections.Generic;
using System;



namespace Blackletter
{
    [CreateAssetMenu(fileName = "BlackletterScript", menuName = "Scriptable Objects/BlackletterScript")]
    public sealed class BlackletterScript : ScriptableObject
    {
        [Header("Source")]
        [TextArea(8, 32)]
        [SerializeField] private string sourceText = "";

        [Header("Compilation State")]
        [SerializeField, HideInInspector] private bool hasErrors;

    #nullable enable
        [System.NonSerialized] private List<Blackletter.Token>? tokens;
        [System.NonSerialized] private List<Blackletter.Diagnostic>? diagnostics;
        [System.NonSerialized] private object? compiledProgram; 
    #nullable disable


        public string SourceText
        {
            get => sourceText;
            set
            {
                if (sourceText == value) return;
                sourceText = value;
                Invalidate();
            }
        }



    #nullable enable
        public bool HasErrors => hasErrors;
        public IReadOnlyList<Blackletter.Token>? Tokens => tokens;
        public IReadOnlyList<Blackletter.Diagnostic>? Diagnostics => diagnostics;
    #nullable disable

        //mark code as dirty
        public void Invalidate()
        {
            tokens = null;
            diagnostics = null;
            compiledProgram = null;
            hasErrors = false;
        }

        //frequent compile on demand
        public void Compile()
        {
            Invalidate();

            //
            // var lexer = new Lexer(sourceText);
            // tokens = lexer.Lex(out diagnostics);
            // if (diagnostics.HasErrors) { hasErrors = true; return; }
            // compiledProgram = compiler.Emit(boundTree);

            diagnostics = new List<Blackletter.Diagnostic>();
            tokens = new List<Blackletter.Token>();

            hasErrors = false;
        }

    #if UNITY_EDITOR
        private void OnValidate()
        {

            Compile();
        }
    #endif
    }
}

