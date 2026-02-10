


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

        public override string ToString()
        {
            string returnValue = $"{type}: {lexeme}";
            switch (type)
            {
                case (TokenType.LeftParen):
                    returnValue = "(";
                    break;
                case (TokenType.RightParen):
                    returnValue = ")";
                    break;
                default:
                    return $"{type}: {lexeme}";
            }


            return returnValue;



        }
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
