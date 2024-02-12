using Ephemera.Reusable;
using System;
using System.Collections.Generic;

namespace Ephemera.Lexing
{
    public class Lexer
    {
        public static List<Token> Lex(string input)
        {
            var tokens = new List<Token>();

            for (int i = 0; i < input.Length; i++)
            {
                var character = input[i];

                if (character == ' ')
                {
                    var token = TokenizeWhitespace(input, i);
                    i = token.Lexeme.EndPosition - 1;
                    tokens.Add(token);
                }
                else if (character == '+')
                {
                    tokens.Add(new Token(TokenClass.PlusOperator, character, i, i));
                }
                else if (character == '-')
                {
                    tokens.Add(new Token(TokenClass.MinusOperator, character, i, i));
                }
                else if (character == '*')
                {
                    tokens.Add(new Token(TokenClass.TimesOperator, character, i, i));
                }
                else if (character == '/')
                {
                    tokens.Add(new Token(TokenClass.DivisionOperator, character, i, i));
                }
                else if (character == '%')
                {
                    tokens.Add(new Token(TokenClass.PercentOperator, character, i, i));
                }
                else if (character == ':')
                {
                    tokens.Add(new Token(TokenClass.Colon, character, i, i));
                }
                else if (character == '>')
                {
                    if (input.CharAt(i + 1) == ']')
                    {
                        i++;
                        tokens.Add(new Token(TokenClass.AttributeEnd, ">]", i, i + 1));
                    }
                    else
                    {
                        TokenizeBooleanOperator(input, character, ref i, tokens, TokenClass.GreaterOperator, TokenClass.GreaterOrEqualsOperator);
                    }
                }
                else if (character == '<')
                {
                    TokenizeBooleanOperator(input, character, ref i, tokens, TokenClass.LessOperator, TokenClass.LessOrEqualsOperator);
                }
                else if (character == '=')
                {
                    if (input.CharAt(i + 1) == '>')
                    {
                        i++;
                        tokens.Add(new Token(TokenClass.FatArrow, "=>", i, i + 1));
                    }
                    else
                    {
                        TokenizeBooleanOperator(input, character, ref i, tokens, TokenClass.AssignmentOperator, TokenClass.EqualsOperator);
                    }
                }
                else if (character == '!')
                {
                    TokenizeBooleanOperator(input, character, ref i, tokens, TokenClass.NegationOperator, TokenClass.NotEqualsOperator);
                }
                else if (character == '&')
                {
                    if (input.CharAt(i + 1) == '&')
                    {
                        i++;
                        tokens.Add(new Token(TokenClass.AndOperator, "&&", i, i + 1));
                    }
                    else
                    {
                        //todo error
                    }
                }
                else if (character == '|')
                {
                    if (input.CharAt(i + 1) == '|')
                    {
                        i++;
                        tokens.Add(new Token(TokenClass.OrOperator, "||", i, i + 1));
                    }
                    else
                    {
                        //todo error
                    }
                }
                else if (character == '(')
                {
                    tokens.Add(new Token(TokenClass.OpenParen, character, i, i));
                }
                else if (character == ')')
                {
                    tokens.Add(new Token(TokenClass.CloseParen, character, i, i));
                }
                else if (character == '[')
                {
                    if (input.CharAt(i + 1) == '<')
                    {
                        i++;
                        tokens.Add(new Token(TokenClass.AttributeStart, "[<", i, i + 1));
                    }
                    else
                    {
                        tokens.Add(new Token(TokenClass.OpenSquare, character, i, i));
                    }
                }
                else if (character == ']')
                {
                    tokens.Add(new Token(TokenClass.CloseSquare, character, i, i));
                }
                else if (character == '?')
                {
                    if (input.CharAt(i + 1) == '?')
                    {
                        tokens.Add(new Token(TokenClass.QuestionQuestionOperator, "??", i, ++i));
                    }
                    else if (input.CharAt(i + 1) == '.')
                    {
                        tokens.Add(new Token(TokenClass.QuestionDotOperator, "?.", i, ++i));
                    }
                    else
                    {
                        tokens.Add(new Token(TokenClass.QuestionMark, character, i, i));
                    }
                }
                else if (character == '.')
                {
                    if (input.CharAt(i + 1) == '.')
                    {
                        tokens.Add(new Token(TokenClass.DotDotOperator, "..", i, ++i));
                    }
                    else
                    {
                        tokens.Add(new Token(TokenClass.DotOperator, character, i, i));
                    }
                }
                else if (character == '\n')
                {
                    tokens.Add(new Token(TokenClass.NewLine, character, i, i));
                }
                else if (character == '{')
                {
                    tokens.Add(new Token(TokenClass.OpenCurly, character, i, i));
                }
                else if (character == '}')
                {
                    tokens.Add(new Token(TokenClass.CloseCurly, character, i, i));
                }
                else if (character == ',')
                {
                    tokens.Add(new Token(TokenClass.Comma, character, i, i));
                }
                else if (char.IsDigit(character))
                {
                    var token = TokenizeNumber(input, i);
                    i = token.Lexeme.EndPosition - 1;
                    tokens.Add(token);
                }
                else if (char.IsLetter(character))
                {
                    var lexeme = GetLexeme(input, i, char.IsLetter);
                    i = lexeme.EndPosition - 1;
                    var tokenClass = TokenAssistant.GetKeyword(lexeme.Word) ?? TokenClass.Identifier;
                    tokens.Add(new Token(tokenClass, lexeme));
                }
                else if (character == '"')
                {
                    var token = TokenizeStringLiteral(input, i);
                    tokens.Add(token);
                    i = token.Lexeme.EndPosition - 1;
                }
                else if (character == '#')
                {
                    var endPosition = i;
                    var word = "#" + GetWord(input, i + 1, ref endPosition, char.IsLetter);
                    i = endPosition - 1;
                    var token = new Token(TokenClass.TypeParam, word, i, endPosition);
                    tokens.Add(token);
                }
            }

            return tokens;
        }

        private static Token TokenizeStringLiteral(string input, int startPosition)
        {
            var endPosition = startPosition + 1;
            while (endPosition < input.Length && input.CharAt(endPosition++) != '"') ;
            var word = input.Substring(startPosition, endPosition - startPosition);
            return new Token(TokenClass.StringLiteral, word, startPosition, endPosition);
        }

        private static void TokenizeBooleanOperator(string input, char character, ref int i, List<Token> tokens, TokenClass singleCharOperator, TokenClass doubleCharOperator)
        {
            var nextChar = input.CharAt(i + 1);
            Token token;
            if (nextChar == '=')
            {
                token = new Token(doubleCharOperator, character.ToString() + nextChar.Value.ToString(), i, ++i);
            }
            else
            {
                token = new Token(singleCharOperator, character, i, i);
            }
            tokens.Add(token);
        }

        private static Token TokenizeNumber(string input, int startPosition)
        {
            var end = startPosition;
            var word = GetWord(input, startPosition, ref end, char.IsDigit);            

            var dot = input.CharAt(end);
            if (dot == '.')
            {
                var end2 = end + 1;
                var digit = input.CharAt(end2);
                if (digit != null && char.IsDigit(digit.Value))
                {
                    var word2 = GetWord(input, end2, ref end2, char.IsDigit);
                    return new Token(TokenClass.NumberLiteral, input.Substring(startPosition, end2 - startPosition), startPosition, end2);
                }
            }

            return new Token(TokenClass.NumberLiteral, input.Substring(startPosition, end - startPosition), startPosition, end);
            //return TokenizeByPredicate(input, startPosition, TokenClass.NumberLiteral, char.IsNumber);
        }

        private static Token TokenizeWhitespace(string input, int startPosition)
        {
            return TokenizeByPredicate(input, startPosition, TokenClass.Whitespace, c => c == ' ');
        }

        private static Token TokenizeByPredicate(string input, int startPosition, TokenClass @class, Func<char, bool> predicate)
        {
            var lexeme = GetLexeme(input, startPosition, predicate);
            return new Token(@class, lexeme);
        }

        private static Lexeme GetLexeme(string input, int startPosition, Func<char, bool> predicate)
        {
            var endPosition = startPosition;
            var word = GetWord(input, startPosition, ref endPosition, predicate);
            return new Lexeme(word, startPosition, endPosition);
        }

        private static string GetWord(string input, int startPosition, ref int endPosition, Func<char, bool> predicate)
        {
            while (endPosition < input.Length)
            {
                endPosition++;
                var character = input.CharAt(endPosition);
                if (character == null || !predicate(character.Value))
                {
                    break;
                }
            }
            return input.Substring(startPosition, endPosition - startPosition);
        }
    }
}
