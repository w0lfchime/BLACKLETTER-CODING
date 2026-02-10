using System.Collections.Generic;
using UnityEngine;

namespace Blackletter
{
    public static class Interpreter
    {
        public static void Execute(List<Token> tokens, GameObject context)
        {
            Debug.Log("Executing script...");

            int i = 0;

            while (i < tokens.Count)
            {
                var token = tokens[i];

                if (token.type == TokenType.Keyword)
                {
                    switch (token.lexeme)
                    {
                        case "print":
                            i++;
                            Debug.Log(ReadSingleValue(tokens, ref i));
                            break;

                        case "move":
                            i++;
                            float amount = float.Parse(ReadSingleValue(tokens, ref i));
                            context.transform.position += Vector3.right * amount;
                            break;
                    }
                }

                i++;
            }
        }

        private static string ReadSingleValue(List<Token> tokens, ref int i)
        {
            if (tokens[i].type == TokenType.Number ||
                tokens[i].type == TokenType.Identifier)
            {
                return tokens[i].lexeme;
            }

            return "";
        }
    }
}