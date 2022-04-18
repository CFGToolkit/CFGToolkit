using CFGToolkit.Grammar.Structure;
using System.Collections.Generic;

namespace CFGToolkit.Grammar.Algorithms.Reductions
{
    public class ManySingleReductor : ExpressionReductor
    {
        public override IEnumerable<Expression> TransformAlternative(Production production, Queue<Production> queue, Expression alternative)
        {
            var newAlternative = new Expression();
            for (var i = 0; i < alternative.Symbols.Count; i++)
            {
                var symbol = alternative.Symbols[i];
                if (symbol is ManyExpression many && !IsSingleAlready(many.Inside))
                {
                    var newProductionIdentifier = new ProductionIdentifier(NameProvider.GetUniqueName(production.Name.Value + "_many"));
                    newAlternative.Symbols.Add(new ManyExpression() { Inside = new Expressions(new Expression(newProductionIdentifier)) });

                    var newProduction = new Production() { Name = newProductionIdentifier, Tags = production.Tags};

                    newProduction.Alternatives.AddRange(many.Inside);

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
