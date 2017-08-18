﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Mutate4l.Cli
{
    public class Lexer
    {
        private readonly string Buffer;

        private Dictionary<char, TokenType> SingleOperators = new Dictionary<char, TokenType>
        {
            { ':', TokenType.Colon }
        };

        private Dictionary<string, TokenType> DoubleOperators = new Dictionary<string, TokenType>
        {
            { "=>", TokenType.Destination },
            { "+>", TokenType.AddToDestination }
        };

        private Dictionary<string, TokenType> Commands = new Dictionary<string, TokenType>
        {
            { "interleave", TokenType.Interleave },
            { "constrain", TokenType.Constrain },
            { "explode", TokenType.Explode }
        };

        private Dictionary<string, TokenType> Options = new Dictionary<string, TokenType>
        {
            { "start", TokenType.Start },
            { "pitch", TokenType.Pitch },
            { "ranges", TokenType.Ranges },
            { "counts", TokenType.Counts },
            { "mode", TokenType.Mode },
            { "mask", TokenType.Mask },
            { "strength", TokenType.Strength }
        };

        private Dictionary<string, TokenType> EnumValues = new Dictionary<string, TokenType>
        {
            { "time", TokenType.Time },
            { "event", TokenType.Event }
        };

        public Lexer(string buffer)
        {
            Buffer = buffer;
        }

        private bool IsSingleOperator(int pos)
        {
            return SingleOperators.Any(o => o.Key == Buffer[pos]);
        }

        private bool IsDoubleOperator(int pos)
        {
            if (Buffer.Length > pos + 1)
            {
                string nextTwoChars = $"{Buffer[pos]}{Buffer[pos + 1]}";
                return DoubleOperators.Any(o => o.Key == nextTwoChars);
            }
            return false;
        }

        private bool IsClipReference(int pos)
        {
            return (Buffer.Length > pos + 1) && IsAlpha(pos) && IsNumeric(pos + 1);
        }

        private bool IsMusicalDivision(int pos)
        {
            return IsNumeric(pos) && Buffer[pos + 1] == '/' && IsNumeric(pos + 2);
        }

        private bool IsAlpha(int pos)
        {
            return IsAlpha(Buffer[pos]);
        }

        public static bool IsAlpha(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        }

        private bool IsNumeric(int pos)
        {
            return IsNumeric(Buffer[pos]);
        }

        public static bool IsNumeric(char c)
        {
            return c >= '0' && c <= '9';
        }

        private Token GetIdentifier(int pos, params Dictionary<string, TokenType>[] validValues)
        {
            string identifier = "";
            int initialPos = pos;

            while (IsAlpha(pos))
            {
                identifier += Buffer[pos++].ToString();
            }
            if (identifier.Length > 0 && validValues.Any(va => va.Any(v => v.Key == identifier)))
            {
                try
                {
                    return new Token(validValues.Where(va => va.Any(v => v.Key.Equals(identifier, StringComparison.InvariantCultureIgnoreCase))).First()[identifier.ToLower()], identifier, initialPos);
                }
                catch (Exception)
                {
                    throw new Exception($"Unknown token encountered at position {initialPos}");
                }
            }
            throw new Exception($"Unknown token encountered at position {initialPos}");
        }

        public IEnumerable<Token> GetTokens()
        {
            int position = 0;
            while (position < Buffer.Length)
            {
                Token token = null;
                if (IsSingleOperator(position))
                {
                    char value = Buffer[position];
                    token = new Token(SingleOperators[value], value.ToString(), position);
                }
                else if (IsDoubleOperator(position))
                {
                    string value = $"{Buffer[position]}{Buffer[position + 1]}";
                    token = new Token(DoubleOperators[value], value, position);
                }
                else if (IsClipReference(position))
                {
                    token = new Token(TokenType.ClipReference, GetRemainingNumericToken(position, 2), position);
                }
                else if (IsMusicalDivision(position))
                {
                    token = new Token(TokenType.MusicalDivision, GetRemainingNumericToken(position, 3), position);
                }
                else if (IsNumeric(position))
                {
                    token = new Token(TokenType.Number, GetRemainingNumericToken(position, 1), position);
                }
                else if (IsAlpha(position))
                {
                    Token identifierToken = GetIdentifier(position, Commands, Options, EnumValues);
                    token = identifierToken;
                }
                if (token != null)
                {
                    position += token.Value.Length;
                    yield return token;
                }
                position = SkipNonTokens(position);
            }
            yield break;
        }

        // Fetches the remainder of a token consisting of numeric digits, with an optional offset which can be used in the case of values like 1/32 where you know the first 3 digits to be valid
        private string GetRemainingNumericToken(int position, int offset)
        {
            while (position + offset < Buffer.Length && IsNumeric(position + offset))
            {
                offset++;
            }
            return Buffer.Substring(position, offset);
        }

        private int SkipNonTokens(int position)
        {
            while (position < Buffer.Length && (Buffer[position] == ' ' || Buffer[position] == '\t' || Buffer[position] == '\r' || Buffer[position] == '\n'))
            {
                position++;
            }
            return position;
        }
    }
}
