using System;
using System.Collections.Generic;
using System.Text;

namespace compiles_lab_1.Core
{
    public enum LexemeCode
    {
        Plus = 1,
        Minus = 2,
        Star = 3,
        Slash = 4,
        Percent = 5,
        LParen = 6,
        RParen = 7,
        Identifier = 8,
        Number = 9,
        Error = 10
    }


    public class Lexeme
    {
        public LexemeCode Code { get; set; }
        public string Type { get; set; }
        public string Text { get; set; }
        public int Line { get; set; }
        public int StartColumn { get; set; }
        public int EndColumn { get; set; }
    }

    public class ScanResult
    {
        public List<Lexeme> Lexemes { get; } = new();
    }

    public class Scanner
    {
        private bool IsLatinLetter(char c)
        {
            return (c >= 'A' && c <= 'Z') ||
                   (c >= 'a' && c <= 'z');
        }

        private bool IsValidSingle(char c)
        {
            return c == '+' || c == '-' || c == '*' || c == '/' ||
                   c == '%' || c == '(' || c == ')';
        }

        public ScanResult Scan(string text)
        {
            var result = new ScanResult();
            int line = 1, col = 1, i = 0;

            while (i < text.Length)
            {
                char c = text[i];

                if (c == '\r') { i++; continue; }
                if (c == '\n') { line++; col = 1; i++; continue; }
                if (char.IsWhiteSpace(c)) { col++; i++; continue; }

                int start = col;

                if (IsLatinLetter(c))
                {
                    var sb = new StringBuilder();
                    sb.Append(c);
                    i++; col++;

                    while (i < text.Length)
                    {
                        char ch = text[i];
                        if (IsLatinLetter(ch) || char.IsDigit(ch) || ch == '_')
                        {
                            sb.Append(ch);
                            i++; col++;
                        }
                        else break;
                    }

                    result.Lexemes.Add(new Lexeme
                    {
                        Code = LexemeCode.Identifier,
                        Type = "id",
                        Text = sb.ToString(),
                        Line = line,
                        StartColumn = start,
                        EndColumn = col - 1
                    });

                    continue;
                }

                if (char.IsDigit(c))
                {
                    var sb = new StringBuilder();
                    sb.Append(c);
                    i++; col++;

                    while (i < text.Length && char.IsDigit(text[i]))
                    {
                        sb.Append(text[i]);
                        i++; col++;
                    }

                    result.Lexemes.Add(new Lexeme
                    {
                        Code = LexemeCode.Number,
                        Type = "num",
                        Text = sb.ToString(),
                        Line = line,
                        StartColumn = start,
                        EndColumn = col - 1
                    });

                    continue;
                }

                Lexeme Add(LexemeCode code, string type)
                {
                    return new Lexeme
                    {
                        Code = code,
                        Type = type,
                        Text = c.ToString(),
                        Line = line,
                        StartColumn = start,
                        EndColumn = start
                    };
                }

                if (IsValidSingle(c))
                {
                    LexemeCode code =
                        c == '+' ? LexemeCode.Plus :
                        c == '-' ? LexemeCode.Minus :
                        c == '*' ? LexemeCode.Star :
                        c == '/' ? LexemeCode.Slash :
                        c == '%' ? LexemeCode.Percent :
                        c == '(' ? LexemeCode.LParen :
                                   LexemeCode.RParen;

                    string type =
                        c == '+' ? "plus" :
                        c == '-' ? "minus" :
                        c == '*' ? "star" :
                        c == '/' ? "slash" :
                        c == '%' ? "percent" :
                        c == '(' ? "lparen" :
                                   "rparen";

                    result.Lexemes.Add(Add(code, type));
                    i++; col++;
                    continue;
                }

                var err = new StringBuilder();
                err.Append(c);
                i++; col++;

                while (i < text.Length)
                {
                    char ch = text[i];

                    if (!IsLatinLetter(ch) &&
                        !char.IsDigit(ch) &&
                        !char.IsWhiteSpace(ch) &&
                        !IsValidSingle(ch))
                    {
                        err.Append(ch);
                        i++; col++;
                    }
                    else break;
                }

                result.Lexemes.Add(new Lexeme
                {
                    Code = LexemeCode.Error,
                    Type = "error",
                    Text = err.ToString(),
                    Line = line,
                    StartColumn = start,
                    EndColumn = col - 1
                });
            }

            return result;
        }
    }
}
