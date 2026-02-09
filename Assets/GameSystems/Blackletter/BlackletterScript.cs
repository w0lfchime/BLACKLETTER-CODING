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
        [SerializeField] public string sourceText = "";


        public bool isValid = false;

        public List<Blackletter.Token> tokens;
 


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

        //mark code as dirty
        public void Invalidate()
        {



            isValid = false;
        }

        //frequent compile on demand
        public void Compile()
        {



            isValid = true;
        }

    #if UNITY_EDITOR
        private void OnValidate()
        {

            Compile();
        }
    #endif
    }
}

