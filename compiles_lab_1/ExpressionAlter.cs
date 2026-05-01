using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using compiles_lab_1.Core;

namespace compiles_lab_1
{
    public class Tetrad
    {
        public string Result { get; set; }
        public string Operation { get; set; }
        public string Arg1 { get; set; }
        public string Arg2 { get; set; }
    }

    internal static class ExpressionAlter
    {
        public static List<string> BuildPoliz(List<Lexeme> lexemes)
        {
            var output = new List<string>();
            var stack = new Stack<string>();

            int Priority(string op) =>
                op == "*" || op == "/" || op == "%" ? 2 :
                op == "+" || op == "-" ? 1 : 0;

            foreach (var lex in lexemes)
            {
                if (lex.Code == LexemeCode.Number || lex.Code == LexemeCode.Identifier)
                {
                    output.Add(lex.Text);
                }
                else if (lex.Code == LexemeCode.Plus ||
                         lex.Code == LexemeCode.Minus ||
                         lex.Code == LexemeCode.Star ||
                         lex.Code == LexemeCode.Slash ||
                         lex.Code == LexemeCode.Percent)
                {
                    string op = lex.Text;

                    while (stack.Count > 0 &&
                           Priority(stack.Peek()) >= Priority(op))
                    {
                        output.Add(stack.Pop());
                    }

                    stack.Push(op);
                }
                else if (lex.Code == LexemeCode.LParen)
                {
                    stack.Push("(");
                }
                else if (lex.Code == LexemeCode.RParen)
                {
                    while (stack.Peek() != "(")
                        output.Add(stack.Pop());
                    stack.Pop();  
                }
            }

            while (stack.Count > 0)
                output.Add(stack.Pop());

            return output;
        }

        public static List<Tetrad> BuildTetrads(List<string> poliz)
        {
            var tetrads = new List<Tetrad>();
            var stack = new Stack<string>();
            int tempIndex = 0;

            foreach (var token in poliz)
            {
                if (IsOperator(token))
                {
                    string b = stack.Pop();
                    string a = stack.Pop();

                    string res = $"t{tempIndex++}";

                    tetrads.Add(new Tetrad
                    {
                        Operation = token,
                        Arg1 = a,
                        Arg2 = b,
                        Result = res
                    });

                    stack.Push(res);
                }
                else
                {
                    stack.Push(token);
                }
            }

            return tetrads;
        }

       static bool IsOperator(string s) =>
            s == "+" || s == "-" || s == "*" || s == "/" || s == "%";

        public static int EvalPoliz(List<string> poliz)
        {
            var stack = new Stack<int>();

         
                foreach (var token in poliz)
                {
                    if (int.TryParse(token, out int num))
                    {
                        stack.Push(num);
                    }
                    else
                    {
                        int b = stack.Pop();
                        int a = stack.Pop();

                    try
                    {

                        if ((token == "/" || token == "%") && b == 0)
                            throw new Exception("Деление на 0 запрещено");

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Ошибка при вычислении выражения", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return 0;
                    }
                        int r = token switch
                        {
                            "+" => a + b,
                            "-" => a - b,
                            "*" => a * b,
                            "/" => a / b,
                            "%" => a % b,
                            _ => throw new Exception("Unknown operator")
                        };

                        stack.Push(r);
                    }
                }
            
         
            return stack.Pop();
        }


    }
}
