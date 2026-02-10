using System.Collections.Generic;
using System;
using UnityEngine;

namespace Blackletter
{
    [CreateAssetMenu(fileName = "BlackletterScript", menuName = "Blackletter/Script")]
    public sealed class BlackletterScript : ScriptableObject
    {
        [TextArea(10, 40)]
        [SerializeField] private string source;

        [NonSerialized] public List<Token> tokens;
        [NonSerialized] public bool isDirty = true;

        public string Source
        {
            get => source;
            set
            {
                if (source == value) return;
                source = value;
                isDirty = true;
            }
        }
    }
}