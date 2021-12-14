using System;

namespace CFGToolkit.Grammar.Algorithms.Graphs
{
    public class Edge<TNode>
        where TNode : IEquatable<TNode>
    {
        public Edge(TNode from, TNode to)
        {
            From = from;
            To = to;
        }

        public TNode From { get; set; }

        public TNode To { get; set; }
    }
}
