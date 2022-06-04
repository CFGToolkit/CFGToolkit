using CFGToolkit.GrammarDefinition;
using System.Collections.Generic;
using System.Text;

namespace CFGToolkit.ParserCombinatorGenerator
{
    public abstract class BaseGenerator
    {
        public abstract List<ClassStaticMember> GeneratePasers(Grammar grammar);

        public string GenerateFile(string @namespace, string className, Grammar grammar)
        {
            string result =
                $@"using CFGToolkit.AST;
using CFGToolkit.ParserCombinator;
using CFGToolkit.ParserCombinator.Input;
using CFGToolkit.ParserCombinator.Values;
using CFGToolkit.ParserCombinator.Parsers;
using System;
using System.Collections.Generic;
using System.Threading;

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

        private string GenerateNonTerminals(Grammar grammar)
        {
            var builder = new StringBuilder();

            foreach (var production in grammar.Productions)
            {
                builder.AppendLine(Repeat(" ", 12) + "public const string " + GetParserName(production.Key) + " = \"" + production.Key + "\";");
            }

            return builder.ToString();
        }

        private string CreateParsers(Grammar grammar)
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
        protected static string GetParserName(string productionName)
        {
            var s = productionName.Replace("[pattern]", "");

            if (s == "string")
            {
                s = "@" + s;
            }

            if (s.StartsWith("$"))
            {
                s = s.Substring(1);
            }

            return s;
        }
    }
}
