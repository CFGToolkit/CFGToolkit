using System.Collections.Generic;
using System.Text;

namespace CFGToolkit.ParserCombinatorGenerator
{
    public abstract class BaseGenerator
    {
        public abstract List<ClassStaticMember> GeneratePasers(Grammar.Grammar grammar);

        public string GenerateFile(string @namespace, string className, Grammar.Grammar grammar)
        {
            string result =
                $@"using CFGToolkit.AST;
using CFGToolkit.ParserCombinator;
using CFGToolkit.ParserCombinator.Input;
using CFGToolkit.ParserCombinator.Values;
using System;
using System.Collections.Generic;

namespace {@namespace}
{{
    public partial class {className}
    {{
{CreateParsers(grammar)}

        public class NonTerminals
        {{
{GenerateNonTerminals(grammar)}
        }}

    }}
}}";

            return result;

        }

        private string GenerateNonTerminals(Grammar.Grammar grammar)
        {
            var builder = new StringBuilder();

            foreach (var production in grammar.Productions)
            {
                builder.AppendLine(Repeat(" ", 12) + "public const string " + production.Key + " = \"" + production.Key + "\";");
            }

            return builder.ToString();
        }

        private string CreateParsers(Grammar.Grammar grammar)
        {
            var builder = new StringBuilder();

            var parsers = GeneratePasers(grammar);

            foreach (var parser in  parsers)
            {
                var lines = parser.ToString().Split("\r\n");
                foreach (var line in lines)
                {
                    builder.AppendLine(Repeat(" ", 8) + line);
                }

                builder.AppendLine();
            }

            return builder.ToString();
        }

        private string Repeat(string str, int i)
        {
            string result = ""; //TODO

            for (var j = 0; j < i; j++)
            {
                result += str;
            }

            return result;
        }
    }
}
