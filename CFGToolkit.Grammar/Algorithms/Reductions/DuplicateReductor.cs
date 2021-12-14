using CFGToolkit.Grammar.Structure;
using System.Collections.Generic;

namespace CFGToolkit.Grammar.Algorithms.Reductions
{
    public class DuplicateReductor : ExpressionReductor
    {
        private Dictionary<Production, HashSet<string>> _expressions = new Dictionary<Production, HashSet<string>>();

        public int Skipped { get; private set; } = 0;

        public override void Precompute(Grammar input)
        {
            _expressions.Clear();
        }

        public override IEnumerable<Expression> TransformAlternative(Production production, Queue<Production> queue, Expression alternative)
        {
            var key = alternative.ToString();
            if (!_expressions.ContainsKey(production))
            {
                _expressions[production] = new HashSet<string>();
            }

            if (_expressions[production].Contains(key))
            {
                Skipped++;
                yield break;
            }
            else
            {
                _expressions[production].Add(key);
                yield return alternative;
            }
        }
    }
}
