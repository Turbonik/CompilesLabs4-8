namespace compiles_lab_1.Core.Ast
{
    public class SemanticError
    {
        public string Message { get; set; }
        public int Line { get; set; }
        public int StartColumn { get; set; }
        public int EndColumn { get; set; }
    }
}
