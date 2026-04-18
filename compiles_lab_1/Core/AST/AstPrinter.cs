using System.IO;
using System.Text;

namespace compiles_lab_1.Core.Ast
{
    public static class AstPrinter
    {
        public static string Print(AstNode node)
        {
            var sw = new StringWriter();
            PrintNode(node, "", true, sw);
            return sw.ToString();
        }

        private static void PrintNode(AstNode node, string indent, bool last, TextWriter output)
        {
            string marker = last ? "└── " : "├── ";

            switch (node)
            {
                case ConstDeclNode c:
                    output.WriteLine($"{indent}{marker}ConstDeclNode");
                    string childIndent = indent + (last ? "    " : "│   ");

                    output.WriteLine($"{childIndent}├── name: \"{c.Name}\"");
                    output.WriteLine($"{childIndent}├── modifiers: [\"const\", \"val\"]");
                     
                    output.WriteLine($"{childIndent}├── type: IntNode");
                    output.WriteLine($"{childIndent}│   └── name: \"Int\"");
 
                    output.WriteLine($"{childIndent}└── value: IntLiteralNode");
                    output.WriteLine($"{childIndent}    └── value: {((IntLiteralNode)c.Children[3]).Value}");
                    break;
            }
        }
    }
}
