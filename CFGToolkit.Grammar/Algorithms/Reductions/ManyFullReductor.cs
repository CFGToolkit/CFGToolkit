using CFGToolkit.Grammar.Structure;
using System.Collections.Generic;
using System.Linq;

namespace CFGToolkit.Grammar.Algorithms.Reductions
{
    public class ManyFullReductor : ExpressionReductor
    {
        public override IEnumerable<Expression> TransformAlternative(Production production, Queue<Production> queue, Expression alternative)
        {
            var newAlternative = new Expression();
            for (var i = 0; i < alternative.Symbols.Count; i++)
            {
                var symbol = alternative.Symbols[i];
                if (symbol is ManyExpression many)
                {
                    var newProductionIdentifier = new ProductionIdentifier(NameProvider.GetUniqueName(production.Name.Value + "_many"));

                    if (many.Inside.Count == 1 && many.Inside[0].Symbols.Count == 1 && many.Inside[0].Symbols[0] is ProductionIdentifier p)
                    {
                        newProductionIdentifier = new ProductionIdentifier(NameProvider.GetUniqueName(production.Name.Value + "_" + p.Value + "_many"));
                    }

                    newAlternative.Symbols.Add(newProductionIdentifier);
                    var newProduction = new Production() { Name = newProductionIdentifier, Tags = new Dictionary<string, string>(production.Tags) };
                    newProduction.Alternatives.AddRange(many.Inside.Select(exp => new Expression(exp.Symbols, newProductionIdentifier)));
                    newProduction.Alternatives.Add(new Expression(new Empty()));
                    queue.Enqueue(newProduction);
                }
                else
                {
                    newAlternative.Symbols.Add(symbol);
                }
            }

            yield return newAlternative;
        }
    }
}
