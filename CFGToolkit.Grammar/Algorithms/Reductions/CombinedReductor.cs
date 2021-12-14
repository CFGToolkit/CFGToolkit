using CFGToolkit.Grammar.Structure;
using System;
using System.Collections.Generic;

namespace CFGToolkit.Grammar.Algorithms.Reductions
{
    public class CombinedReductor : ExpressionReductor
    {
        public CombinedReductor(ExpressionReductor[] reductors)
        {
            if (reductors is null)
            {
                throw new ArgumentNullException(nameof(reductors));
            }

            Reductors = reductors;
        }

        protected ExpressionReductor[] Reductors { get; }

        public override void Precompute(Grammar input)
        {
            foreach (var reductor in Reductors)
            {
                reductor.Precompute(input);
            }
        }

        public override IEnumerable<Expression> TransformAlternative(Production production, Queue<Production> queue, Expression alternative)
        {
            var list = new List<Expression>();
            list.Add(alternative);

            for (var i = 0; i < Reductors.Length; i++)
            {
                var copy = list.ToArray();
                list.Clear();
                foreach (var item in copy)
                {
                    list.AddRange(Reductors[i].TransformAlternative(production, queue, item));
                }
            }


            return list;
        }
    }
}
