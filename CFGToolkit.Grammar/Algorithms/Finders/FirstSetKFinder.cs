using CFGToolkit.Grammar.Structure;
using System.Collections.Generic;

namespace CFGToolkit.Grammar.Algorithms.Finders
{
    public class FirstSetKFinder
    {
        public Dictionary<string, HashSet<string>> FindFirst(Grammar grammar, int k)
        {
            var result = new Dictionary<string, HashSet<string>>();

            var memo = new Dictionary<string, HashSet<string>>();

            foreach (var productionName in grammar.Productions.Keys)
            {
                result[productionName] = FindFirst(grammar, productionName, k, memo);
            }

            return result;
        }


        private HashSet<string> FindFirst(Grammar grammar, string productionName, int k, Dictionary<string, HashSet<string>> memo)
        {
            var result = new HashSet<string>();
            var production = grammar.Productions[productionName];

            for (var i = 0; i < production.Alternatives.Count; i++)
            {
                var expression = production.Alternatives[i];

                HashSet<string> tmp = FindFirst(grammar, expression, k, memo);

                Merge(result, tmp);
            }

            return result;
        }


        public HashSet<string> FindFirst(Grammar grammar, string productionName, int k)
        {
            return FindFirst(grammar, productionName, k, new Dictionary<string, HashSet<string>>());
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

        public HashSet<string> FindFirst(Grammar grammar, Expression expression, int k, Dictionary<string, HashSet<string>> memo)
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

                tmp = FindFirst(grammar, expression, i, k, memo);

                i++;
            }
            while (tmp.Contains(null) && i < expression.Count);

            Merge(result, tmp);

            return result;
        }

        private HashSet<string> FindFirst(Grammar grammar, Expression expression, int i, int k, Dictionary<string, HashSet<string>> memo)
        {
            var result = new HashSet<string>();

            if (k == 0 || i > expression.Count - 1)
            {
                return result;
            }

            if (k == 1)
            {
                if (expression.Symbols[i] is Literal or Pattern)
                {
                    result.Add(expression.Symbols[i].ToString());
                }

                if (expression.Symbols[i] is ProductionIdentifier p)
                {
                    if (!memo.ContainsKey(p.Value + "|1"))
                    {
                        var tmp = FindFirst(grammar, p.Value, k, memo);
                        Merge(result, tmp);
                        memo[p.Value + "|1"] = result;
                    }
                    else
                    {
                        return memo[p.Value + "|1"];
                    }
                }

                if (expression.Symbols[i] is ManyExpression m && m.Inside.Count == 1 && m.Inside[0].Symbols[0] is ProductionIdentifier p2)
                {
                    if (!memo.ContainsKey(p2.Value + "|1"))
                    {
                        var tmp = FindFirst(grammar, p2.Value, k, memo);
                        Merge(result, tmp);

                        if (!result.Contains(null))
                        {
                            result.Add(null);
                        }

                        memo[p2.Value + "|1"] = result;
                    }
                    else
                    {
                        return memo[p2.Value + "|1"];
                    }
                }

                if (expression.Symbols[i] is OptionalExpression o && o.Inside.Count == 1 && o.Inside[0].Symbols[0] is ProductionIdentifier p3)
                {
                    if (!memo.ContainsKey(p3.Value + "|1"))
                    {
                        var tmp = FindFirst(grammar, p3.Value, k, memo);
                        Merge(result, tmp);

                        if (!result.Contains(null))
                        {
                            result.Add(null);
                        }

                        memo[p3.Value + "|1"] = result;
                    }
                    else
                    {
                        return memo[p3.Value + "|1"];
                    }
                }
            }

            if (k > 1)
            {
                if (expression.Symbols[i] is Literal or Pattern)
                {
                    var tail = FindFirst(grammar, expression, i + 1, k - 1, memo);
                    tail.Remove(null);

                    if (tail.Count > 0)
                    {
                        foreach (var tailItem in tail)
                        {
                            result.Add(expression.Symbols[i].ToString() + "@@" + tailItem ?? "null"); //TODO
                        }
                    }
                    else
                    {
                        result.Add(expression.Symbols[i].ToString()); //TODO
                    }
                }

                if (expression.Symbols[i] is ProductionIdentifier p)
                {
                    if (!memo.ContainsKey(p.Value + "|" + k))
                    {
                        var tmp = FindFirst(grammar, p.Value, k, memo);

                        foreach (var tmpItem in tmp)
                        {
                            var len = tmpItem != null ? tmpItem.Split("@@").Length : 0;

                            if (len < k)
                            {
                                var tail = FindFirst(grammar, expression, i + 1, k - len, memo);
                                tail.Remove(null);

                                if (tail.Count > 0)
                                {
                                    foreach (var tailItem in tail)
                                    {
                                        result.Add(tmpItem + "@@" + (tailItem ?? "null")); //TODO
                                    }
                                }
                                else
                                {
                                    result.Add(tmpItem); //TODO
                                }
                            }
                            else
                            {
                                Merge(result, tmp);
                            }
                        }

                        memo[p.Value + "|" + k] = result;
                    }
                    else
                    {
                        return memo[p.Value + "|" + k] = result;
                    }
                }

                if (expression.Symbols[i] is ManyExpression m && m.Inside.Count == 1 && m.Inside[0].Symbols[0] is ProductionIdentifier p2)
                {
                    if (!memo.ContainsKey(p2.Value + "|" + k))
                    {
                        var tmp = FindFirst(grammar, p2.Value, k, memo);

                        foreach (var tmpItem in tmp)
                        {
                            var len = tmpItem != null ? tmpItem.Split("@@").Length : 0;

                            if (len < k)
                            {
                                var tail = FindFirst(grammar, expression, i + 1, k - len, memo);
                                tail.Remove(null);

                                if (tail.Count > 0)
                                {
                                    foreach (var tailItem in tail)
                                    {
                                        result.Add(tmpItem + "@@" + (tailItem ?? "null")); //TODO
                                    }
                                }
                                else
                                {
                                    result.Add(tmpItem); //TODO
                                }
                            }
                            else
                            {
                                Merge(result, tmp);
                            }
                        }

                        if (!result.Contains(null))
                        {
                            result.Add(null);
                        }

                        memo[p2.Value + "|" + k] = result;
                    }
                    else
                    {
                        return memo[p2.Value + "|" + k] = result;
                    }
                }

                if (expression.Symbols[i] is OptionalExpression o && o.Inside.Count == 1 && o.Inside[0].Symbols[0] is ProductionIdentifier p3)
                {
                    if (!memo.ContainsKey(p3.Value + "|" + k))
                    {
                        var tmp = FindFirst(grammar, p3.Value, k, memo);

                        foreach (var tmpItem in tmp)
                        {
                            var len = tmpItem != null ? tmpItem.Split("@@").Length : 0;

                            if (len < k)
                            {
                                var tail = FindFirst(grammar, expression, i + 1, k - len, memo);
                                tail.Remove(null);

                                if (tail.Count > 0)
                                {
                                    foreach (var tailItem in tail)
                                    {
                                        result.Add(tmpItem + "@@" + (tailItem ?? "null")); //TODO
                                    }
                                }
                                else
                                {
                                    result.Add(tmpItem); //TODO
                                }
                            }
                            else
                            {
                                Merge(result, tmp);
                            }
                        }
                        if (!result.Contains(null))
                        {
                            result.Add(null);
                        }

                        memo[p3.Value + "|" + k] = result;
                    }
                    else
                    {
                        return memo[p3.Value + "|" + k] = result;
                    }
                }
            }
            return result;

        }
    }
}
