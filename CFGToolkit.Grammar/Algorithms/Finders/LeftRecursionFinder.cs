using CFGToolkit.Grammar.Structure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CFGToolkit.Grammar.Algorithms.Finders
{
    public class LeftRecursionFinder
    {
        public IEnumerable<string> FindLeftRecursion(Grammar grammar, bool direct = true, Func<string, bool> filter = null)
        {
            foreach (var production in grammar.Productions.Keys.Where(k => filter == null || filter(k)))
            {
                var visitedIdentifiers = new HashSet<string>();

                if (IsLeftRecursion(grammar, production, production, visitedIdentifiers, direct))
                {
                    yield return production;
                }
            }

            yield break;
        }

        protected static bool IsLeftRecursion(Grammar grammar, string productionName, string identifier, HashSet<string> visited, bool direct)
        {
            var production = grammar.Productions[productionName];
            visited.Add(productionName);

            foreach (var alternative in production.Alternatives)
            {
                var firstSymbol = alternative.Symbols.First();
                if (firstSymbol is ProductionIdentifier i)
                {
                    if (i.Value == productionName)
                    {
                        return true;
                    }

                    if (i.Value == identifier && !direct)
                    {
                        return true;
                    }

                    if (!direct && !visited.Contains(i.Value) && IsLeftRecursion(grammar, i.Value, identifier, visited, direct))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
