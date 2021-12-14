using CFGToolkit.Grammar.Structure;
using System.Collections.Generic;
using System.Linq;

namespace CFGToolkit.Grammar.Algorithms.Finders
{
    public class EmptyProductionFinder
    {
        public IEnumerable<string> Find(Grammar input)
        {
            var copy = input.Clone();

            //1. Find direct 
            var empty = new HashSet<string>();

            foreach (var productionName in copy.Productions.Keys)
            {
                var production = input.Productions[productionName];

                if (HasEmpty(production.Alternatives))
                {
                    empty.Add(productionName);
                }
            }

            var toProcess = new HashSet<string>(empty);
            while (true)
            {
                var emptyProductionName = toProcess.FirstOrDefault();

                if (emptyProductionName is null)
                {
                    break;
                }

                toProcess.Remove(emptyProductionName);

                foreach (var productionName in copy.Productions.Keys)
                {
                    if (!empty.Contains(productionName))
                    {
                        var production = copy.Productions[productionName];

                        foreach (var alternative in production.Alternatives)
                        {
                            alternative.Symbols = new List<ISymbol>(alternative.Symbols.Where(s => s is not OptionalExpression));
                            alternative.Symbols = new List<ISymbol>(alternative.Symbols.Where(s => s is not ManyExpression));
                            alternative.Symbols = new List<ISymbol>(alternative.Symbols.Where(s => s is not ProductionIdentifier || s is ProductionIdentifier p && !empty.Contains(p.Value)));
                        }

                        if (HasEmpty(production.Alternatives))
                        {
                            empty.Add(productionName);
                            toProcess.Add(productionName);
                        }
                    }
                }
            }

            return empty;
        }

        private bool HasEmpty(Expressions altenatives)
        {
            return altenatives.Count == 0 || altenatives.Any(a => a.Symbols.Count == 0 || a.Symbols.Count == 1 && a.Symbols.Any(s => s is Empty));
        }
    }
}
