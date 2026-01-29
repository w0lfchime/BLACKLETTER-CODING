using UnityEngine;
using System.Collections.Generic;
using System;



[CreateAssetMenu(fileName = "BlackletterScript", menuName = "Scriptable Objects/BlackletterScript")]
public sealed class BlackletterScript : ScriptableObject
{
    [Header("Source")]
    [TextArea(8, 32)]
    [SerializeField] private string sourceText = "";

    [Header("Compilation State")]
    [SerializeField, HideInInspector] private bool hasErrors;

#nullable enable
    // These are NOT serialized on purpose
    [System.NonSerialized] private List<Blackletter.Token>? tokens;
    [System.NonSerialized] private List<Blackletter.Diagnostic>? diagnostics;
    [System.NonSerialized] private object? compiledProgram; // later: instruction list / bytecode
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

    /// <summary>Marks cached compilation results dirty.</summary>
    public void Invalidate()
    {
        tokens = null;
        diagnostics = null;
        compiledProgram = null;
        hasErrors = false;
    }

    /// <summary>Compile on demand. Safe to call repeatedly.</summary>
    public void Compile()
    {
        Invalidate();

        // Later this becomes:
        // var lexer = new Lexer(sourceText);
        // tokens = lexer.Lex(out diagnostics);
        // if (diagnostics.HasErrors) { hasErrors = true; return; }
        // var parser = new Parser(tokens);
        // ...
        // compiledProgram = compiler.Emit(boundTree);

        diagnostics = new List<Blackletter.Diagnostic>();
        tokens = new List<Blackletter.Token>();

        // placeholder until lexer exists
        hasErrors = false;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Auto-recompile in editor when text changes
        Compile();
    }
#endif
}
