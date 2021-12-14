using CFGToolkit.Grammar.Structure;
using System.Collections.Generic;
using System.Linq;

namespace CFGToolkit.Grammar.Algorithms.Graphs
{
    public class GrammarSort
    {
        public static IEnumerable<Production> Sort(Grammar input)
        {
            var graph = new Graph<string>();

            foreach (var production in input.Productions)
            {
                graph.Nodes.Add(production.Key);

                foreach (var alt in production.Value.Alternatives)
                {
                    var uses = alt.Where(s => s is ProductionIdentifier p);

                    foreach (ProductionIdentifier use in uses)
                    {
                        graph.Edges.Add(new Edge<string>(use.Value, production.Key));
                    }
                }
            }

            var top = new TopologicalSort();
            var sorted = top.GetSorted(graph).ToList(); // try to sort what is possible, ignore the cycles

            var sortedProductions = input.Productions.OrderBy(p => sorted.IndexOf(p.Key) >= 0 ? sorted.IndexOf(p.Key) : int.MaxValue).Select(p => p.Value);

            return sortedProductions;
        }
    }
}
