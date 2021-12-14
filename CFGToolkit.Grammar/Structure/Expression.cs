using System.Collections;
using System.Collections.Generic;

namespace CFGToolkit.Grammar.Structure
{
    public class Expression : IEnumerable<ISymbol>
    {
        public Expression(params object[] symbols)
        {
            foreach (var symbol in symbols)
            {
                if (symbol is ISymbol s)
                {
                    Symbols.Add(s);
                }

                if (symbol is IEnumerable<ISymbol> list)
                {
                    Symbols.AddRange(list);
                }
            }
        }

        public int Count => Symbols.Count;

        public List<ISymbol> Symbols { get; set; } = new List<ISymbol>();

        public IEnumerator<ISymbol> GetEnumerator()
        {
            return Symbols.GetEnumerator();
        }

        public Expression Clone()
        {
            return new Expression(Symbols);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Symbols.GetEnumerator();
        }

        public override string ToString()
        {
            return string.Join(" ", Symbols);
        }
    }
}
