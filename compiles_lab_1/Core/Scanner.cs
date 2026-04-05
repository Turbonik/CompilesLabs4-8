using System;

namespace compiles_lab_1.Core
{
    public static class Scanner
    {
        static bool IsLatinLetter(char c) =>
            (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');

        public static ScanResult Analyze(string source)
        {
            var result = new ScanResult();
            int line = 1, col = 1, i = 0;

            while (i < source.Length)
            {
                char ch = source[i];

                if (ch == '\n')
                {
                    line++; col = 1; i++;
                    continue;
                }

                if (ch == ' ' || ch == '\t')
                {
                    i++; col++;
                    continue;
                }

                if (i + 6 <= source.Length &&
                    source.AsSpan(i, 5).SequenceEqual("const".AsSpan()) &&
                    source[i + 5] == ' ')
                {
                    result.Lexemes.Add(new Lexeme
                    {
                        Code = LexemeCode.KeywordConst,
                        Type = Strings.Keyword,
                        Text = "const",
                        Line = line,
                        StartColumn = col,
                        EndColumn = col + 4
                    });
                    i += 6; col += 6;
                    continue;
                }

                if (i + 4 <= source.Length &&
                    source.AsSpan(i, 3).SequenceEqual("val".AsSpan()) &&
                    source[i + 3] == ' ')
                {
                    result.Lexemes.Add(new Lexeme
                    {
                        Code = LexemeCode.KeywordVal,
                        Type = Strings.Keyword,
                        Text = "val",
                        Line = line,
                        StartColumn = col,
                        EndColumn = col + 2
                    });
                    i += 4; col += 4;
                    continue;
                }

                if (i + 3 <= source.Length &&
                    source.AsSpan(i, 3).SequenceEqual("Int".AsSpan()))
                {
                    result.Lexemes.Add(new Lexeme
                    {
                        Code = LexemeCode.KeywordInt,
                        Type = Strings.Keyword,
                        Text = "Int",
                        Line = line,
                        StartColumn = col,
                        EndColumn = col + 2
                    });
                    i += 3; col += 3;
                    continue;
                }

                if (char.IsDigit(ch))
                {
                    int startCol = col, startIndex = i;
                    while (i < source.Length && char.IsDigit(source[i]))
                    {
                        i++; col++;
                    }
                    result.Lexemes.Add(new Lexeme
                    {
                        Code = LexemeCode.Integer,
                        Type = Strings.Integer,
                        Text = source.Substring(startIndex, i - startIndex),
                        Line = line,
                        StartColumn = startCol,
                        EndColumn = col - 1
                    });
                    continue;
                }

                if (IsLatinLetter(ch) || ch == '_')
                {
                    int startCol = col, startIndex = i;
                    while (i < source.Length &&
                          (IsLatinLetter(source[i]) || char.IsDigit(source[i]) || source[i] == '_'))
                    {
                        i++; col++;
                    }
                    result.Lexemes.Add(new Lexeme
                    {
                        Code = LexemeCode.Identifier,
                        Type = Strings.Identifier,
                        Text = source.Substring(startIndex, i - startIndex),
                        Line = line,
                        StartColumn = startCol,
                        EndColumn = col - 1
                    });
                    continue;
                }

                if (ch == ':' || ch == '=' || ch == ';' || ch == '-')
                {
                    LexemeCode code =
                        ch == ':' ? LexemeCode.Colon :
                        ch == '=' ? LexemeCode.Assign :
                        ch == ';' ? LexemeCode.Semicolon :
                        LexemeCode.Minus;

                    string type =
                        ch == ':' ? Strings.DeclareOperator :
                        ch == '=' ? Strings.EqualsOperator :
                        ch == ';' ? Strings.EndOperator :
                        Strings.SubstractionOperator;

                    result.Lexemes.Add(new Lexeme
                    {
                        Code = code,
                        Type = type,
                        Text = ch.ToString(),
                        Line = line,
                        StartColumn = col,
                        EndColumn = col
                    });

                    i++; col++;
                    continue;
                }

                int errStartCol = col, errStartIndex = i;
                while (i < source.Length)
                {
                    char c = source[i];
                    bool ok =
                        char.IsDigit(c) ||
                        IsLatinLetter(c) ||
                        c == '_' || c == ':' || c == '=' ||
                        c == ';' || c == '-' ||
                        c == ' ' || c == '\t' || c == '\n';
                    if (ok) break;
                    i++; col++;
                }

                string bad = source.Substring(errStartIndex, i - errStartIndex);

                result.Lexemes.Add(new Lexeme
                {
                    Code = LexemeCode.Error,
                    Type = Strings.Error,
                    Text = bad,
                    Line = line,
                    StartColumn = errStartCol,
                    EndColumn = col - 1
                });
            }

            return result;
        }
    }
}
