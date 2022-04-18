using CFGToolkit.Grammar.Readers;
using CFGToolkit.Grammar.Structure;
using System;
using System.Collections.Generic;

namespace CFGToolkit.Grammar.Algorithms.Reductions
{
    public abstract class ExpressionReductor : IReductor
    {
        public ExpressionReductor()
        {
            NameProvider = new UniqueIdentifiers();
        }

        protected UniqueIdentifiers NameProvider { get; }

        public Grammar Reduct(Grammar input, Func<string, bool> filter = null)
        {
            var result = new Grammar() { Start = input.Start };

            Precompute(input);

            foreach (var production in input.Productions.Keys)
            {
                if (filter == null || filter(production))
                {
                    Reduct(result, input.Productions[production]);
                }
            }

            return result;
        }

        public virtual void Precompute(Grammar input)
        {
        }

        protected void Reduct(Grammar result, Production production)
        {
            var queue = new Queue<Production>();
            queue.Enqueue(production);

            while (queue.Count > 0)
            {
                var item = queue.Dequeue();

                var cleanedProduction = new Production() { Name = item.Name, Tags = new Dictionary<string, string>(production.Tags) };

                foreach (var alternative in item.Alternatives)
                {
                    var newAlternatives = TransformAlternative(production, queue, alternative);
                    cleanedProduction.Alternatives.AddRange(newAlternatives);
                }

                result.Productions[item.Name.Value] = cleanedProduction;
            }
        }

        public abstract IEnumerable<Expression> TransformAlternative(Production production, Queue<Production> queue, Expression alternative);

        protected bool IsSingleAlready(Expressions inside)
        {
            return inside.Count == 1 && inside[0].Symbols.Count == 1 && inside[0].Symbols[0] is ProductionIdentifier;
        }
    }
}
