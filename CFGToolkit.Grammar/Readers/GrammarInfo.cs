using System.Collections.Generic;

namespace CFGToolkit.Grammar.Readers
{
    public class GrammarInfo
    {
        public HashSet<string> ProductionNames { get; set; } = new HashSet<string>();

        public bool IsIdentifier(string name)
        {
            return ProductionNames.Contains(name);
        }
    }
}
