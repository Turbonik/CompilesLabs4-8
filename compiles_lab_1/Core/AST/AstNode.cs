using System.Collections.Generic;

namespace compiles_lab_1.Core.Ast
{
    public abstract class AstNode
    {
        public int Line { get; set; }
        public int StartColumn { get; set; }
        public int EndColumn { get; set; }

        public List<AstNode> Children { get; } = new();
    }

    public class ConstDeclNode : AstNode
    {
        public string Name { get; set; }
        public string[] Modifiers { get; set; } = new[] { "const", "val" };
    }

    public class ModifiersNode : AstNode { }

    public class IdentifierNode : AstNode
    {
        public string Name { get; set; }
    }

    public class IntNode : AstNode
    {
        public string Name { get; set; }  
    }

    public class IntLiteralNode : AstNode
    {
        public int Value { get; set; }    
    }

    public class TokenNode : AstNode
    {
        public string Text { get; }
        public TokenNode(string text) => Text = text;
    }

    public class SemicolonNode : AstNode { }

}
