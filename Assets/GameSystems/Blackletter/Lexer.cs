using System.Collections.Generic;

namespace Blackletter
{
    public static class Lexer
    {
        private static readonly HashSet<string> Keywords = new()
    {
        "print",
        "move"
    };

        public static List<Token> Tokenize(string source)
        {
            var tokens = new List<Token>();
            int i = 0;

            while (i < source.Length)
            {
                char c = source[i];

                // whitespace
                if (c == ' ' || c == '\t')
                {
                    i++;
                    continue;
                }

                if (c == '\n')
                {
                    tokens.Add(new Token(TokenType.NewLine, "\\n"));
                    i++;
                    continue;
                }

                // number
                if (char.IsDigit(c))
                {
                    int start = i;
                    while (i < source.Length && char.IsDigit(source[i])) i++;
                    tokens.Add(new Token(TokenType.Number, source[start..i]));
                    continue;
                }

                // identifier / keyword
                if (char.IsLetter(c))
                {
                    int start = i;
                    while (i < source.Length && char.IsLetter(source[i])) i++;
                    string word = source[start..i];

                    tokens.Add(
                        Keywords.Contains(word)
                            ? new Token(TokenType.Keyword, word)
                            : new Token(TokenType.Identifier, word)
                    );
                    continue;
                }

                // punctuation
                if (c == '(')
                {
                    tokens.Add(new Token(TokenType.LeftParen, "("));
                    i++;
                    continue;
                }

                if (c == ')')
                {
                    tokens.Add(new Token(TokenType.RightParen, ")"));
                    i++;
                    continue;
                }

                // unknown → skip for now
                i++;
            }

            tokens.Add(new Token(TokenType.EOF, ""));
            return tokens;
        }
    }
}