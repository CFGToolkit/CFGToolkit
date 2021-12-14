using CFGToolkit.Grammar.Structure;
using System.Collections.Generic;

namespace CFGToolkit.Grammar.Algorithms.Reductions
{
    public class OptionalSingleReductor : ExpressionReductor
    {
        public override IEnumerable<Expression> TransformAlternative(Production production, Queue<Production> queue, Expression alternative)
        {
            var newAlternative = new Expression();
            for (var i = 0; i < alternative.Symbols.Count; i++)
            {
                var symbol = alternative.Symbols[i];
                if (symbol is OptionalExpression opt && !IsSingleAlready(opt.Inside))
                {
                    var newProductionIdentifier = new ProductionIdentifier(NameProvider.GetUniqueName(production.Name.Value + "_optional"));
                    newAlternative.Symbols.Add(new OptionalExpression() { Inside = new Expressions(new Expression(newProductionIdentifier)) });

                    var newProduction = new Production() { Name = newProductionIdentifier };

                    newProduction.Alternatives.AddRange(opt.Inside);

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
