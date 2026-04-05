using Antlr4.Runtime;
using System;
using System.IO;

namespace compiles_lab_1.Core
{
    public static class AntlrWrapper
    {
        public static ParseResult Analyze(string source)
        {
            var result = new ParseResult();

            var errorWriter = new StringWriter();
            Console.SetError(errorWriter);

            try
            {
                var input = new AntlrInputStream(source);
                var lexer = new ConstValLexer(input);
                lexer.RemoveErrorListeners();
                lexer.AddErrorListener(new ConsoleErrorListener<int>());

                var tokens = new CommonTokenStream(lexer);
                var parser = new ConstValParser(tokens);
                parser.RemoveErrorListeners();
                parser.AddErrorListener(new ConsoleErrorListener<IToken>());

                parser.start();
            }
            catch (Exception ex)
            {
                result.Errors.Add(new ParseError
                {
                    Message = ex.Message,
                    Fragment = "",
                    Line = 0,
                    StartColumn = 0,
                    EndColumn = 0
                });
            }

            var raw = errorWriter.ToString();
            if (!string.IsNullOrWhiteSpace(raw))
            {
                result.Errors.Add(new ParseError
                {
                    Message = raw.Trim(),
                    Fragment = "",
                    Line = 0,
                    StartColumn = 0,
                    EndColumn = 0
                });
            }

            return result;
        }
    }
}
