


namespace Blackletter
{
    public readonly struct Token
    {
        public readonly TokenType type;
        public readonly string lexeme;

        public Token(TokenType type, string lexeme)
        {
            this.type = type;
            this.lexeme = lexeme;
        }

        public override string ToString() => $"{type}: {lexeme}";
    }
    public enum TokenType
    {
        Identifier,
        Number,
        Keyword,
        LeftParen,
        RightParen,
        NewLine,
        EOF
    }
}
