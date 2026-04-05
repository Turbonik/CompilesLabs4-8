using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace compiles_lab_1.Core
{
    public static class SyntaxHighlighter
    {
        private static readonly HashSet<string> keywords = new()
        {
            "Int", "const", "val"
        };

        private static readonly char[] separators =
        {
            ' ', '\t', '\n', '\r', '(', ')', '{', '}', '[', ']', ';', ',', '.', ':',
            '"', '\'', '+', '-', '*', '/', '%', '=', '!', '<', '>'
        };

        public static void Highlight(RichTextBox box)
        {
            int selStart = box.SelectionStart;
            int selLength = box.SelectionLength;

            box.SuspendLayout();
 
            box.Select(0, box.Text.Length);
            box.SelectionColor = Color.Black;

            int index = 0;

            while (index < box.Text.Length)
            {
 
                if (Array.IndexOf(separators, box.Text[index]) >= 0)
                {
                    index++;
                    continue;
                }
 
                int start = index;
                while (index < box.Text.Length &&
                       Array.IndexOf(separators, box.Text[index]) < 0)
                {
                    index++;
                }

                string word = box.Text.Substring(start, index - start);

                if (keywords.Contains(word))
                {
                    box.Select(start, word.Length);
                    box.SelectionColor = Color.Blue;
                }
            }

            box.SelectionStart = selStart;
            box.SelectionLength = selLength;

            box.ResumeLayout();
        }
    }
}
