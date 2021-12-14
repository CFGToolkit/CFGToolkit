using CFGToolkit.Grammar.Algorithms.Finders;
using CFGToolkit.Grammar.Structure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CFGToolkit.Grammar.Algorithms.Reductions
{
    public class EmptyReductor : ExpressionReductor
    {
        private string _start;
        private HashSet<string> _emptySymbols;

        public override void Precompute(Grammar input)
        {
            var finder = new EmptyProductionFinder();
            _start = input.Start;
            _emptySymbols = new HashSet<string>(finder.Find(input));
        }

        public override IEnumerable<Expression> TransformAlternative(Production production, Queue<Production> queue, Expression alternative)
        {
            if (alternative.Symbols.Count == 1 && alternative.Symbols[0] is Empty)
            {
                if (production.Name.Value == _start)
                {
                    yield return alternative;
                }
                yield break;
            }

            var positions = new List<int>();
            for (var i = 0; i < alternative.Symbols.Count; i++)
            {
                /*if (alternative.Symbols[i] is ManyExpression)
                {
                    positions.Add(i);
                }

                if (alternative.Symbols[i] is OptionalExpression)
                {
                    positions.Add(i);
                }*/

                if (alternative.Symbols[i] is ProductionIdentifier p && _emptySymbols.Contains(p.Value))
                {
                    positions.Add(i);
                }
            }

            int count = positions.Count;

            if (count == 0)
            {
                yield return alternative;
            }
            else
            {
                for (var i = 0; i < Math.Pow(2, count); i++)
                {
                    var newexp = new Expression();

                    for (var j = 0; j < alternative.Symbols.Count; j++)
                    {
                        int index = positions.IndexOf(j);

                        if (index != -1)
                        {
                            var include = (i & 1 << index) != 0;

                            if (include)
                            {
                                if (alternative.Symbols[j] is OptionalExpression opt)
                                {
                                    newexp.Symbols.AddRange(opt.Inside[0].Symbols); //TODO
                                }
                                else
                                {
                                    newexp.Symbols.Add(alternative.Symbols[j]);
                                }

                            }
                        }
                        else
                        {
                            newexp.Symbols.Add(alternative.Symbols[j]);
                        }
                    }

                    if (newexp.Symbols.Count != 0 && newexp.Symbols.Any(s => s is not Empty)
                        || newexp.Symbols.Count == 1 && (newexp.Symbols[0] is not ProductionIdentifier || newexp.Symbols[0] is ProductionIdentifier p && p.Value != production.Name.Value))
                    {
                        yield return newexp;
                    }

                    if (newexp.Symbols.Count == 0 && production.Name.Value == _start)
                    {
                        yield return new Expression(new Empty());
                    }
                }

                // remove duplicates
                yield break;
            }
        }
    }
}
