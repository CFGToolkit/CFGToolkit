using CFGToolkit.Grammar.Structure;
using System.Collections.Generic;
using System.Linq;

namespace CFGToolkit.Grammar
{
    public class Grammar
    {
        public Grammar(string start = "")
        {
            Start = start;
        }

        public Grammar(IEnumerable<Production> productions)
        {
            foreach (var production in productions)
            {
                Productions[production.Name.Value] = production;
            }
        }

        public Dictionary<string, Production> Productions { get; set; } = new Dictionary<string, Production>();

        public string Start { get; set; }

        public void Add(Production production)
        {
            Productions[production.Name.Value] = production;
        }

        public Grammar Clone()
        {
            return new Grammar(Productions.Select(p => p.Value.Clone())) { Start = Start };
        }

        public Grammar Clone(HashSet<string> filter)
        {
            return new Grammar(Productions.Where(p => filter.Contains(p.Key)).Select(p => p.Value.Clone())) { Start = Start };
        }
    }
}
