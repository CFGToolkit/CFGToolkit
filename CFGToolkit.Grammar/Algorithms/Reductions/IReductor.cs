using System;

namespace CFGToolkit.Grammar.Algorithms.Reductions
{
    public interface IReductor
    {
        Grammar Reduct(Grammar input, Func<string, bool> filter = null);
    }
}