using CFGToolkit.Grammar.Structure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CFGToolkit.Grammar.Algorithms.Finders
{
    public class RelatedFinder
    {
        public Grammar FindRelated(Grammar grammar, Func<string, bool> filter)
        {
            var toProcess = new Queue<string>();
            var result = new HashSet<string>();
            foreach (var production in grammar.Productions.Keys.Where(k => filter == null || filter(k)))
            {
                toProcess.Enqueue(production);
                result.Add(production);
            }

            while (toProcess.Count > 0)
            {
                var productionName = toProcess.Dequeue();
                var production = grammar.Productions[productionName];

                production.Alternatives.ToList().ForEach(alternative =>
                {
                    alternative.Symbols.ForEach(symbol =>
                    {
                        if (symbol is ProductionIdentifier i && !result.Contains(i.Value))
                        {
                            result.Add(i.Value);
                            toProcess.Enqueue(i.Value);
                        }
                        if (symbol is OptionalExpression o && o.Inside.Count == 1 && o.Inside[0].Symbols[0] is ProductionIdentifier pOpt && !result.Contains(pOpt.Value))
                        {
                            result.Add(pOpt.Value);
                            toProcess.Enqueue(pOpt.Value);
                        }

                        if (symbol is ManyExpression many && many.Inside.Count == 1 && many.Inside[0].Symbols[0] is ProductionIdentifier vv && !result.Contains(vv.Value))
                        {
                            result.Add(vv.Value);
                            toProcess.Enqueue(vv.Value);
                        }
                    });

                });
            }

            return grammar.Clone(result);
        }
    }
}
