using System.Collections.Generic;
using System.Linq;

namespace CFGToolkit.Grammar.Structure
{
    public class Production : IStatement
    {
        public ProductionIdentifier Name { get; set; }

        public Expressions Alternatives { get; set; } = new Expressions();

        public HashSet<string> Attributes { get; set; } = new HashSet<string>();

        public override string ToString()
        {
            return (Name?.Value ?? "null") + " ::=  " + Alternatives.ToString();
        }

        public Production Clone()
        {
            return new Production() { Name = Name, Attributes = new HashSet<string>(Attributes), Alternatives = new Expressions(Alternatives.Select(exp => exp.Clone())) };
        }
    }
}
