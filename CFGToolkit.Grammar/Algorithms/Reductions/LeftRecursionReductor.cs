using CFGToolkit.Grammar.Structure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CFGToolkit.Grammar.Algorithms.Reductions
{
    public class LeftRecursionReductor : IReductor
    {
        public Grammar Reduct(Grammar input, Func<string, bool> filter = null)
        {
            var result = input.Clone();
            var productions = input.Productions.Keys.Where(k => filter == null || filter(k)).OrderBy(key => key).ToArray();

            for (var i = 0; i < productions.Length; i++)
            {
                var a_i = productions[i];

                if (i == 0)
                {
                    Eliminate(result, a_i);
                }
                else
                {
                    var a_js = productions.Take(i);
                    Replace(result, a_js, a_i);
                    Eliminate(result, a_i);
                }
            }

            return result;
        }

        private static void Replace(Grammar result, IEnumerable<string> a_js, string productionToReplace)
        {
            var production = result.Productions[productionToReplace];
            var alternatives = production.Alternatives.Clone();
            production.Alternatives = new Expressions();

            foreach (var alternative in alternatives)
            {
                bool found = false;
                foreach (var a_j in a_js)
                {
                    if (alternative.Symbols.First() is ProductionIdentifier p && p.Value == a_j)
                    {
                        found = true;
                        var newAlternatives = new Expressions();
                        foreach (var a_jAlternative in result.Productions[a_j].Alternatives)
                        {
                            var newExpression = new Expression(a_jAlternative.Symbols, alternative.Symbols.Skip(1));
                            if (newExpression.Symbols.Count == 0)
                            {

                            }
                            newAlternatives.Add(newExpression);
                        }

                        production.Alternatives.AddRange(newAlternatives);
                    }
                }

                if (!found)
                {
                    production.Alternatives.Add(alternative);
                }
            }
        }

        private static bool Eliminate(Grammar result, string productionName)
        {
            var production = result.Productions[productionName];

            var alfaProductions = production.Alternatives.Where(a => a.Symbols[0] is ProductionIdentifier p && p.Value == productionName).ToList();
            var betaProductions = production.Alternatives.Except(alfaProductions).ToList();

            if (alfaProductions.Any())
            {
                var alfaNewProduction = new Production() { Name = new ProductionIdentifier(productionName), Tags = new Dictionary<string, string>(production.Tags) };

                foreach (var beta in betaProductions)
                {
                    var betaExpression = new Expression(beta.Symbols, new ProductionIdentifier(productionName + "_prim"));
                    alfaNewProduction.Alternatives.Add(betaExpression);
                }

                var alfaPrimProduction = new Production() { Name = new ProductionIdentifier(productionName + "_prim"), Tags = new Dictionary<string, string>(production.Tags) };
                foreach (var alfa in alfaProductions)
                {
                    var alfaExpression = new Expression(alfa.Symbols.Skip(1), new ProductionIdentifier(alfaPrimProduction.Name.Value));
                    alfaPrimProduction.Alternatives.Add(alfaExpression);
                }
                alfaPrimProduction.Alternatives.Add(new Expression(new Empty()));

                result.Productions[productionName] = alfaNewProduction;
                result.Productions[alfaPrimProduction.Name.Value] = alfaPrimProduction;

                return true;
            }
            else
            {
                result.Productions[productionName] = production;

                return false;
            }
        }
    }
}
