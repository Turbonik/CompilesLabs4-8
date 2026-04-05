using System;
using System.Collections.Generic;
using System.Linq;

namespace compiles_lab_1.Core
{
    public enum ParsePhase
    {
        ExpectStart,
        ExpectVal,
        ExpectId,
        ExpectColon,
        ExpectIntKeyword,
        ExpectAssign,
        ExpectNumber,
        Done
    }

    public class ParseError
    {
        public string Fragment { get; set; }
        public string Message { get; set; }
        public int Line { get; set; }
        public int StartColumn { get; set; }
        public int EndColumn { get; set; }
    }

    public class ParseResult
    {
        public List<ParseError> Errors { get; } = new();
    }

    public static class Parser
    {
        static string MsgConst => $"{Strings.Expkey} \"const\"";
        static string MsgVal => $"{Strings.Expkey} \"val\"";
        static string MsgInt => $"{Strings.Expkey} \"Int\"";
        static string MsgColon => $"{Strings.Expsymb} ':'";
        static string MsgAssign => $"{Strings.Expsymb} '='";
        static string MsgNumber => Strings.Expnum;
        static string MsgId => $"{Strings.Expid}";
        static string MsgSemi => $"{Strings.Expsymb} ';'";

        private static bool IsValidUnsigned(Lexeme lex)
        {
            return lex.Text == "0" || (lex.Text.Length > 0 && lex.Text[0] != '0' && lex.Text.All(char.IsDigit));
        }

        private static bool IsValidSigned(Lexeme lex)
        {
            return lex.Text.Length > 0 && lex.Text[0] != '0' && lex.Text.All(char.IsDigit);
        }

        private static int CheckMatch(IReadOnlyList<Lexeme> tokens, int index, ParsePhase phase)
        {
            if (index >= tokens.Count) return 0;
            var t = tokens[index];

            switch (phase)
            {
                case ParsePhase.ExpectStart:
                    return t.Code == LexemeCode.KeywordConst ? 1 : 0;
                case ParsePhase.ExpectVal:
                    return t.Code == LexemeCode.KeywordVal ? 1 : 0;
                case ParsePhase.ExpectId:
                    return (t.Code == LexemeCode.Identifier && t.Text != "val" && t.Text != "const") ? 1 : 0;
                case ParsePhase.ExpectColon:
                    return t.Code == LexemeCode.Colon ? 1 : 0;
                case ParsePhase.ExpectIntKeyword:
                    return t.Code == LexemeCode.KeywordInt ? 1 : 0;
                case ParsePhase.ExpectAssign:
                    return t.Code == LexemeCode.Assign ? 1 : 0;
                case ParsePhase.ExpectNumber:
                    if (t.Code == LexemeCode.Integer && IsValidUnsigned(t)) return 1;
                    if (t.Code == LexemeCode.Minus && index + 1 < tokens.Count)
                    {
                        var t2 = tokens[index + 1];
                        if (t2.Code == LexemeCode.Integer && IsValidSigned(t2)) return 2;
                    }
                    return 0;
                case ParsePhase.Done:
                    return t.Code == LexemeCode.Semicolon ? 1 : 0;
                default:
                    return 0;
            }
        }

        private static string GetExpectedMessage(ParsePhase phase)
        {
            return phase switch
            {
                ParsePhase.ExpectStart => MsgConst,
                ParsePhase.ExpectVal => MsgVal,
                ParsePhase.ExpectId => MsgId,
                ParsePhase.ExpectColon => MsgColon,
                ParsePhase.ExpectIntKeyword => MsgInt,
                ParsePhase.ExpectAssign => MsgAssign,
                ParsePhase.ExpectNumber => MsgNumber,
                ParsePhase.Done => MsgSemi,
                _ => ""
            };
        }

        public static ParseResult Analyze(string source)
        {
            var scan = Scanner.Analyze(source);
            var tokens = scan.Lexemes;
            var result = new ParseResult();

            int i = 0;
            ParsePhase phase = ParsePhase.ExpectStart;

            while (i < tokens.Count)
            {
                var t = tokens[i];

                if (phase == ParsePhase.ExpectStart &&
                    t.Code == LexemeCode.Identifier &&
                    (i + 1 >= tokens.Count || tokens[i + 1].Code == LexemeCode.Semicolon))
                {
                    result.Errors.Add(new ParseError
                    {
                        Fragment = t.Text,
                        Message = MsgConst,
                        Line = t.Line,
                        StartColumn = t.StartColumn,
                        EndColumn = t.EndColumn
                    });

                    int line = t.Line;
                    while (i < tokens.Count && tokens[i].Line == line && tokens[i].Code != LexemeCode.Semicolon)
                        i++;

                    if (i < tokens.Count && tokens[i].Code == LexemeCode.Semicolon)
                        i++;

                    phase = ParsePhase.ExpectStart;
                    continue;
                }

                if (phase == ParsePhase.ExpectStart && t.Code == LexemeCode.Semicolon)
                {
                    int start = i;
                    int line = t.Line;
                    while (i < tokens.Count && tokens[i].Code == LexemeCode.Semicolon && tokens[i].Line == line)
                        i++;
                    var first = tokens[start];
                    var last = tokens[i - 1];
                    string fragment = string.Concat(tokens.Skip(start).Take(i - start).Select(x => x.Text));
                    result.Errors.Add(new ParseError
                    {
                        Fragment = fragment,
                        Message = MsgConst,
                        Line = line,
                        StartColumn = first.StartColumn,
                        EndColumn = last.EndColumn
                    });
                    continue;
                }

                if (t.Code == LexemeCode.Error)
                {
                    int start = i;
                    int line = t.Line;
                    var first = t;
                    string fragment = t.Text;

                    i++;
                    while (i < tokens.Count && tokens[i].Code == LexemeCode.Error && tokens[i].Line == line)
                    {
                        fragment += tokens[i].Text;
                        i++;
                    }

                    var last = tokens[i - 1];

                    result.Errors.Add(new ParseError
                    {
                        Fragment = fragment,
                        Message = Strings.InvalidLexeme,
                        Line = line,
                        StartColumn = first.StartColumn,
                        EndColumn = last.EndColumn
                    });

                    continue;
                }

                int consumed = CheckMatch(tokens, i, phase);
                if (consumed > 0)
                {
                    i += consumed;
                    phase = (phase == ParsePhase.Done) ? ParsePhase.ExpectStart : (ParsePhase)((int)phase + 1);
                    continue;
                }
   
         

                int startIndex = i;
                int syncIndex = -1;
                ParsePhase syncPhase = phase;
 
                bool hasFutureConst = tokens.Skip(i + 1).Any(x => x.Code == LexemeCode.KeywordConst);
                bool hasFutureVal = tokens.Skip(i + 1).Any(x => x.Code == LexemeCode.KeywordVal);

                for (int j = i; j < tokens.Count; j++)
                {
                    if (phase == ParsePhase.ExpectAssign)
                    {
                        if (tokens[j].Code == LexemeCode.Integer && IsValidUnsigned(tokens[j]))
                        {
                            syncIndex = j;
                            syncPhase = ParsePhase.ExpectNumber;
                            break;
                        }

                        if (tokens[j].Code == LexemeCode.Minus &&
                            j + 1 < tokens.Count &&
                            tokens[j + 1].Code == LexemeCode.Integer &&
                            IsValidSigned(tokens[j + 1]))
                        {
                            syncIndex = j;
                            syncPhase = ParsePhase.ExpectNumber;
                            break;
                        }
                    }

                    for (int p = (int)phase; p <= (int)ParsePhase.Done; p++)
                    {
                        var ph = (ParsePhase)p;
                        if (ph == ParsePhase.ExpectNumber)
                            continue;

                        if (phase == ParsePhase.ExpectStart &&
                                (hasFutureConst || hasFutureVal) &&
                                ph == ParsePhase.ExpectId)
                            continue;


                        if (phase == ParsePhase.ExpectVal &&
                            hasFutureVal &&
                            ph == ParsePhase.ExpectId)
                            continue;


                        if (CheckMatch(tokens, j, ph) > 0)
                        {
                            syncIndex = j;
                            syncPhase = ph;
                            break;
                        }
                    }

                    if (syncIndex != -1) break;
                }

                if (phase != ParsePhase.ExpectId &&
            i + 1 < tokens.Count &&
            CheckMatch(tokens, i + 1, phase) > 0)
                {
                    int line = tokens[i].Line;
                    var first = tokens[i];
                    string fragment = first.Text;

                    int k = i + 1;
                    while (k < tokens.Count &&
                      CheckMatch(tokens, k, phase) == 0 &&
                      tokens[k].Code != LexemeCode.Semicolon &&
                      !(phase == ParsePhase.ExpectStart && tokens[k].Code == LexemeCode.KeywordConst) &&
                      !(phase == ParsePhase.ExpectVal && tokens[k].Code == LexemeCode.KeywordVal))
                    {
                        fragment += tokens[k].Text;
                        k++;
                    }


                    result.Errors.Add(new ParseError
                    {
                        Fragment = fragment,
                        Message = GetExpectedMessage(phase),
                        Line = line,
                        StartColumn = first.StartColumn,
                        EndColumn = tokens[k - 1].EndColumn
                    });

                    int consumedExpected = CheckMatch(tokens, i + 1, phase);
                    i = i + 1 + consumedExpected;
                    phase = (phase == ParsePhase.Done) ? ParsePhase.ExpectStart : (ParsePhase)((int)phase + 1);
                    continue;

                }

                string errorMsg = GetExpectedMessage(phase);

                if (syncIndex != -1)
                {
                    if (syncIndex == startIndex)
                    {
                        var cur = tokens[startIndex];

                        if (phase == ParsePhase.ExpectStart)
                        {
                            result.Errors.Add(new ParseError
                            {
                                Fragment = "",
                                Message = errorMsg,
                                Line = cur.Line,
                                StartColumn = cur.StartColumn,
                                EndColumn = cur.StartColumn
                            });
                        }
                        else
                        {
                            var prev = startIndex > 0 ? tokens[startIndex - 1] : cur;
                            result.Errors.Add(new ParseError
                            {
                                Fragment = "",
                                Message = errorMsg,
                                Line = prev.Line,
                                StartColumn = prev.EndColumn + 1,
                                EndColumn = prev.EndColumn + 1
                            });
                        }
                    }
                    else
                    {
                        var firstSkipped = tokens[startIndex];
                        var lastSkipped = tokens[syncIndex - 1];
                        string fragment = string.Join("", tokens.GetRange(startIndex, syncIndex - startIndex).Select(x => x.Text));
                        result.Errors.Add(new ParseError
                        {
                            Fragment = fragment,
                            Message = errorMsg,
                            Line = firstSkipped.Line,
                            StartColumn = firstSkipped.StartColumn,
                            EndColumn = lastSkipped.EndColumn
                        });
                    }

                    i = syncIndex;
                    phase = syncPhase;
                }
                else
                {
                    var firstSkipped = tokens[startIndex];
                    var lastSkipped = tokens[tokens.Count - 1];
                    string fragment = string.Join("", tokens.GetRange(startIndex, tokens.Count - startIndex).Select(x => x.Text));
                    result.Errors.Add(new ParseError
                    {
                        Fragment = fragment,
                        Message = errorMsg,
                        Line = firstSkipped.Line,
                        StartColumn = firstSkipped.StartColumn,
                        EndColumn = lastSkipped.EndColumn
                    });
                    i = tokens.Count;
                }
            }

            if (phase != ParsePhase.ExpectStart && tokens.Count > 0)
            {
                var last = tokens[tokens.Count - 1];
                var lastErr = result.Errors.LastOrDefault();
                var msg = GetExpectedMessage(phase);

                if (lastErr == null || lastErr.Message != msg || lastErr.Line != last.Line)
                {
                    result.Errors.Add(new ParseError
                    {
                        Fragment = "",
                        Message = msg,
                        Line = last.Line,
                        StartColumn = last.EndColumn + 1,
                        EndColumn = last.EndColumn + 1
                    });
                }
            }

            return result;
        }
    }
}
