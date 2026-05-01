
using System.Collections.Generic;

namespace compiles_lab_1.Core
{
    public class ScannerRow
    {
        public string TokenCode { get; set; }
        public string TokenType { get; set; }
        public string Lexeme { get; set; }
        public string Location { get; set; }
    }

    public static class ScannerModel
    {
        public static List<ScannerRow> FromScanResult(ScanResult scan)
        {
            var list = new List<ScannerRow>();

            foreach (var lex in scan.Lexemes)
            {
                list.Add(new ScannerRow
                {
                    TokenCode = ((int)lex.Code).ToString(),
                    TokenType = lex.Type,
                    Lexeme = lex.Text,
                    Location = $"{lex.Line}:{lex.StartColumn}-{lex.EndColumn}"
                });
            }

            return list;
        }
    }
}
