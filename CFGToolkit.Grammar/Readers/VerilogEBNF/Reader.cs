using CFGToolkit.ParserCombinator;
using System.Linq;

namespace CFGToolkit.Grammar.Readers.VerilogEBNF
{
    public class Reader : IReader
    {
        public Grammar Read(string txt)
        {
            var info = new GrammarInfo();
            var names = PreprocessorParsers.ProductionNames.Parse(txt);
            info.ProductionNames = new System.Collections.Generic.HashSet<string>(names.SelectMany(v => v));
            var grammars = GrammarParsers.Grammar(info).TryParse(txt);


            var grammar = grammars.Values.FirstOrDefault();

            return grammar.GetValue<Grammar>();
        }
    }
}
