using System.Collections.Generic;

namespace compiles_lab_1.Core.Ast
{
    public class SymbolInfo
    {
        public string Name { get; set; }
        public string TypeName { get; set; } 
        public int Value { get; set; }
        public int Line { get; set; }
    }

    public class SymbolTable
    {
        private readonly Dictionary<string, SymbolInfo> _symbols = new();

        public bool IsDeclared(string name) => _symbols.ContainsKey(name);

        public SymbolInfo Lookup(string name)
            => _symbols.TryGetValue(name, out var s) ? s : null;

        public bool Declare(SymbolInfo info, out SymbolInfo existing)
        {
            if (_symbols.TryGetValue(info.Name, out existing))
                return false;

            _symbols[info.Name] = info;
            return true;
        }
    }
}
