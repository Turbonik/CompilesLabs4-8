using System.Collections.Generic;

namespace compiles_lab_1.Core
{
    public class ParserError
    {
        public string Fragment { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
    }

    public class ParserResult
    {
        public List<ParserError> Errors { get; } = new();
        public bool Success => Errors.Count == 0;
    }

    public class Parser
    {
        private readonly List<Lexeme> _lex;
        private int _pos;
        private ParserResult _res;

        public Parser(List<Lexeme> lexemes)
        {
            _lex = lexemes;
        }

        private Lexeme Cur => _pos < _lex.Count ? _lex[_pos] : null;

        private void Err(Lexeme lex, string msg)
        {
            if (_res.Errors.Count > 0) return;

            _res.Errors.Add(new ParserError
            {
                Fragment = lex?.Text ?? "",
                Location = lex != null ? $"{lex.Line}:{lex.StartColumn}-{lex.EndColumn}" : "",
                Description = msg
            });
        }

        private bool Match(LexemeCode code)
        {
            if (Cur != null && Cur.Code == code)
            {
                _pos++;
                return true;
            }
            return false;
        }

        public ParserResult Parse()
        {
            _res = new ParserResult();

            foreach (var l in _lex)
            {
                if (l.Code == LexemeCode.Error)
                {
                    Err(l, "Лексическая ошибка");
                    return _res;
                }
            }

            if (_lex.Count == 0)
                return _res;

            E();

            if (_res.Errors.Count > 0)
                return _res;

            if (_pos < _lex.Count)
                Err(Cur, "Лишние символы после выражения");

            return _res;
        }

        private void E()
        {
            T();
            if (_res.Errors.Count > 0) return;
            A();
        }

        private void A()
        {
            while (Cur != null)
            {
                if (Cur.Code == LexemeCode.Plus || Cur.Code == LexemeCode.Minus)
                {
                    _pos++;
                    T();
                    if (_res.Errors.Count > 0) return;
                    continue;
                }
                return;
            }
        }

        private void T()
        {
            F();
            if (_res.Errors.Count > 0) return;
            B();
        }

        private void B()
        {
            while (Cur != null)
            {
                if (Cur.Code == LexemeCode.Star ||
                    Cur.Code == LexemeCode.Slash ||
                    Cur.Code == LexemeCode.Percent)
                {
                    _pos++;
                    F();
                    if (_res.Errors.Count > 0) return;
                    continue;
                }
                return;
            }
        }

        private void F()
        {
            if (Cur == null)
            {
                Err(null, "Ожидался операнд");
                return;
            }

            if (Cur.Code == LexemeCode.Number ||
                Cur.Code == LexemeCode.Identifier)
            {
                _pos++;
                return;
            }

            if (Cur.Code == LexemeCode.LParen)
            {
                var open = Cur;
                _pos++;
                E();
                if (_res.Errors.Count > 0) return;

                if (!Match(LexemeCode.RParen))
                    Err(open, "Нет закрывающей скобки");

                return;
            }

            Err(Cur, "Ожидался операнд или '('");
        }
    }
}
