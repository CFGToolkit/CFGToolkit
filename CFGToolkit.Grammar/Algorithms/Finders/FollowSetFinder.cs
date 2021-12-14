using CFGToolkit.Grammar.Structure;
using System.Collections.Generic;

namespace CFGToolkit.Grammar.Algorithms.Finders
{
    public class FollowSetFinder
    {
        public Dictionary<string, HashSet<string>> FindFollow(Grammar grammar)
        {
            var result = new Dictionary<string, HashSet<string>>();

            result[grammar.Start] = new HashSet<string>() { null };
            var firsts = new FirstSetFinder().FindFirst(grammar);

            var queue = new Queue<string>();
            var visited = new HashSet<string>();

            queue.Enqueue(grammar.Start);
            visited.Add(grammar.Start);

            while (queue.Count > 0)
            {
                var productionName = queue.Dequeue();
                visited.Add(productionName);

                if (!result.ContainsKey(productionName))
                {
                    result[productionName] = new HashSet<string>();
                }

                var production = grammar.Productions[productionName];

                for (var i = 0; i < production.Alternatives.Count; i++)
                {
                    for (var j = 0; j < production.Alternatives[i].Symbols.Count; j++)
                    {
                        var symbol = production.Alternatives[i].Symbols[j];

                        if (symbol is OptionalExpression opt && opt.Inside.Count == 1 && opt.Inside[0].Count == 1 && opt.Inside[0].Symbols[0] is ProductionIdentifier optIdentifier)
                        {
                            HandleOptional(result, firsts, productionName, production, i, j, optIdentifier);
                        }

                        if (symbol is ManyExpression many && many.Inside.Count == 1 && many.Inside[0].Count == 1 && many.Inside[0].Symbols[0] is ProductionIdentifier manyIdentifier)
                        {
                            if (!result.ContainsKey(manyIdentifier.Value))
                            {
                                result[manyIdentifier.Value] = new HashSet<string>();
                            }

                            // add all firsts
                            foreach (var first in firsts[manyIdentifier.Value])
                            {
                                if (first != null)
                                {
                                    if (!result[manyIdentifier.Value].Contains(first))
                                    {
                                        result[manyIdentifier.Value].Add(first);
                                    }
                                }
                            }
                            HandleOptional(result, firsts, productionName, production, i, j, manyIdentifier);

                        }

                        if (symbol is ProductionIdentifier identifier)
                        {
                            HandleProductionIdentifier(result, firsts, productionName, production, i, j, identifier);
                        }
                    }
                }

                for (var i = 0; i < production.Alternatives.Count; i++)
                {
                    for (var j = 0; j < production.Alternatives[i].Symbols.Count; j++)
                    {
                        var symbol = production.Alternatives[i].Symbols[j];

                        if (symbol is ProductionIdentifier p)
                        {
                            if (!visited.Contains(p.Value) && !queue.Contains(p.Value))
                            {
                                queue.Enqueue(p.Value);
                            }
                        }
                        if (symbol is ManyExpression many && many.Inside.Count == 1 && many.Inside[0].Count == 1 && many.Inside[0].Symbols[0] is ProductionIdentifier p2)
                        {
                            if (!visited.Contains(p2.Value) && !queue.Contains(p2.Value))
                            {
                                queue.Enqueue(p2.Value);
                            }
                        }
                        if (symbol is OptionalExpression opt && opt.Inside.Count == 1 && opt.Inside[0].Count == 1 && opt.Inside[0].Symbols[0] is ProductionIdentifier p3)
                        {
                            if (!visited.Contains(p3.Value) && !queue.Contains(p3.Value))
                            {
                                queue.Enqueue(p3.Value);
                            }
                        }
                    }
                }
            }

            return result;
        }

        private static void HandleProductionIdentifier(Dictionary<string, HashSet<string>> result, Dictionary<string, HashSet<string>> firsts, string productionName, Production production, int i, int j, ProductionIdentifier identifier)
        {
            if (!result.ContainsKey(identifier.Value))
            {
                result[identifier.Value] = new HashSet<string>();
            }

            var nextSymbol = j < production.Alternatives[i].Symbols.Count - 1 ? production.Alternatives[i].Symbols[j + 1] : null;

            if (nextSymbol is Literal l)
            {
                if (!result[identifier.Value].Contains(l.Value))
                {
                    result[identifier.Value].Add(l.Value);
                }
            }

            if (nextSymbol is ProductionIdentifier nextIdentifier)
            {
                if (firsts[nextIdentifier.Value].Contains(null))
                {
                    foreach (var follow in result[productionName])
                    {
                        if (!result[identifier.Value].Contains(follow))
                        {
                            result[identifier.Value].Add(follow);
                        }
                    }
                }

                foreach (var first in firsts[nextIdentifier.Value])
                {
                    if (first != null)
                    {
                        if (!result[identifier.Value].Contains(first))
                        {
                            result[identifier.Value].Add(first);
                        }
                    }
                }
            }

            if (nextSymbol is null)
            {
                foreach (var follow in result[productionName])
                {
                    if (follow != null)
                    {
                        if (!result[identifier.Value].Contains(follow))
                        {
                            result[identifier.Value].Add(follow);
                        }
                    }
                }
            }
        }

        private static void HandleOptional(Dictionary<string, HashSet<string>> result, Dictionary<string, HashSet<string>> firsts, string productionName, Production production, int i, int j, ProductionIdentifier optIdentifier)
        {
            if (!result.ContainsKey(optIdentifier.Value))
            {
                result[optIdentifier.Value] = new HashSet<string>();
            }
            var prevSymbol = j > 0 ? production.Alternatives[i].Symbols[j - 1] : null;
            var nextSymbol = j < production.Alternatives[i].Symbols.Count - 1 ? production.Alternatives[i].Symbols[j + 1] : null;

            if (nextSymbol is Literal l)
            {
                if (!result[optIdentifier.Value].Contains(l.Value))
                {
                    result[optIdentifier.Value].Add(l.Value);
                }
            }

            if (nextSymbol is ProductionIdentifier nextIdentifier)
            {
                if (firsts[nextIdentifier.Value].Contains(null))
                {
                    foreach (var follow in result[productionName])
                    {
                        if (!result[optIdentifier.Value].Contains(follow))
                        {
                            result[optIdentifier.Value].Add(follow);
                        }
                    }
                }

                foreach (var first in firsts[nextIdentifier.Value])
                {
                    if (first != null)
                    {
                        if (!result[optIdentifier.Value].Contains(first))
                        {
                            result[optIdentifier.Value].Add(first);
                        }
                    }
                }

                if (prevSymbol is ProductionIdentifier prevIdentifier)
                {
                    if (!result.ContainsKey(prevIdentifier.Value))
                    {
                        result[prevIdentifier.Value] = new HashSet<string>();
                    }

                    foreach (var first in firsts[nextIdentifier.Value])
                    {
                        if (first != null)
                        {
                            if (!result[prevIdentifier.Value].Contains(first))
                            {
                                result[prevIdentifier.Value].Add(first);
                            }
                        }
                    }
                }
            }
        }
    }
}
