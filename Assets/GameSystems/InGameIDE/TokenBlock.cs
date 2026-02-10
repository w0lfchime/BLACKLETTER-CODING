using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Blackletter
{
    public sealed class TokenBlock : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;
        [SerializeField] private Image background;

        public RectTransform Rect => (RectTransform)transform;

        public void Set(Token token)
        {
            label.text = $"{token.type}\n{token.lexeme}";
            background.color = ColorFor(token.type);
        }

        private Color ColorFor(TokenType type)
        {
            return type switch
            {
                TokenType.Keyword => new Color(0.4f, 0.6f, 1f),
                TokenType.Identifier => new Color(0.6f, 0.6f, 0.6f),
                TokenType.Number => new Color(0.4f, 1f, 0.4f),
                TokenType.NewLine => new Color(1f, 0.8f, 0.4f),
                _ => Color.white
            };
        }
    }
}
