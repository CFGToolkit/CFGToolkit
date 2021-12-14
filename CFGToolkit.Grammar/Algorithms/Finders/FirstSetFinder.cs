using CFGToolkit.Grammar.Structure;
using System.Collections.Generic;

namespace CFGToolkit.Grammar.Algorithms.Finders
{
    public class FirstSetFinder
    {
        public Dictionary<string, HashSet<string>> FindFirst(Grammar grammar)
        {
            var result = new Dictionary<string, HashSet<string>>();

            foreach (var productionName in grammar.Productions.Keys)
            {
                result[productionName] = FindFirst(grammar, productionName, new HashSet<string>());
            }

            return result;
        }

        public HashSet<string> FindFirst(Grammar grammar, string productionName)
        {
            return FindFirst(grammar, productionName, new HashSet<string>());
        }

        private HashSet<string> FindFirst(Grammar grammar, string productionName, HashSet<string> visited)
        {
            var result = new HashSet<string>();
            var production = grammar.Productions[productionName];
            visited.Add(productionName);

            for (var i = 0; i < production.Alternatives.Count; i++)
            {
                var expression = production.Alternatives[i];

                HashSet<string> tmp = FindFirst(grammar, expression, visited);

                Merge(result, tmp);

            }

            return result;
        }

        private static void Merge(HashSet<string> result, HashSet<string> tmp)
        {
            foreach (var item in tmp)
            {
                if (!result.Contains(item))
                {
                    result.Add(item);
                }
            }
        }

        public HashSet<string> FindFirst(Grammar grammar, Expression expression)
        {
            return FindFirst(grammar, expression, new HashSet<string>());
        }

        private HashSet<string> FindFirst(Grammar grammar, Expression expression, HashSet<string> visited)
        {
            var result = new HashSet<string>();

            HashSet<string> tmp = null;
            int i = 0;
            do
            {
                if (tmp != null)
                {
                    tmp.Remove(null);
                    Merge(result, tmp);
                }

                tmp = FindFirst(grammar, expression, i, visited);

                i++;
            }
            while (tmp.Contains(null) && i < expression.Count);

            Merge(result, tmp);

            return result;
        }

        private HashSet<string> FindFirst(Grammar grammar, Expression expression, int i, HashSet<string> visited)
        {
            var result = new HashSet<string>();

            if (expression.Symbols[i] is Literal or Pattern)
            {
                result.Add(expression.Symbols[i].ToString());
            }

            if (expression.Symbols[i] is ProductionIdentifier p && !visited.Contains(p.Value))
            {
                var tmp = FindFirst(grammar, p.Value, visited);
                Merge(result, tmp);
            }

            if (expression.Symbols[i] is ManyExpression m && m.Inside.Count == 1 && m.Inside[0].Symbols[0] is ProductionIdentifier p2 && !visited.Contains(p2.Value))
            {
                var tmp = FindFirst(grammar, p2.Value, visited);
                Merge(result, tmp);

                if (!result.Contains(null))
                {
                    result.Add(null);
                }
            }

            if (expression.Symbols[i] is OptionalExpression o && o.Inside.Count == 1 && o.Inside[0].Symbols[0] is ProductionIdentifier p3 && !visited.Contains(p3.Value))
            {
                var tmp = FindFirst(grammar, p3.Value, visited);
                Merge(result, tmp);

                if (!result.Contains(null))
                {
                    result.Add(null);
                }
            }
            return result;
        }
    }
}
