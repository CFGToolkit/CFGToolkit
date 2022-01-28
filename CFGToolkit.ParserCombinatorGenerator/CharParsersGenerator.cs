using CFGToolkit.Grammar.Structure;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CFGToolkit.ParserCombinatorGenerator
{
    public class CharParsersGenerator : BaseGenerator
    {
        public override List<ClassStaticMember> GeneratePasers(Grammar.Grammar grammar)
        {
            var result = new List<ClassStaticMember>();

            foreach (var production in grammar.Productions.Values)
            {
                result.Add(GenerateParser(grammar, production));
            }
            return result;
        }

        private ClassStaticMember GenerateParser(Grammar.Grammar grammar, Production production)
        {
            StringBuilder logic = new StringBuilder();
            int index = 0;
            bool isXor = production.Attributes.Contains("xor");

            if (production.Alternatives.Count == 1)
            {
                var parser = GenerateParserLogic(grammar, production, production.Alternatives[0], index, true);
                logic.Append("   " + parser);
            }
            else
            {
                if (isXor)
                {
                    logic.Append(" Parser.XOr(\"" + production.Name + "\", ");
                }
                else
                {
                    logic.Append(" Parser.Or(\"" + production.Name + "\", ");
                }

                foreach (var alternative in production.Alternatives)
                {
                    var parser = GenerateParserLogic(grammar, production, alternative, index, false);
                    logic.Append("   " + parser);

                    if (production.Alternatives.ToList().IndexOf(alternative) != production.Alternatives.Count - 1)
                    {
                        logic.Append(",\r\n");
                    }
                    index++;
                }

                logic.Append(")");
            }

            return new ClassStaticMember { Name = GetParserName(production.Name.Value), Logic = "  new Lazy<IParser<CharToken, SyntaxNode>>(() => " + logic.ToString() +");", Type = "Lazy<IParser<CharToken, SyntaxNode>>" };
        }

        private static string GetParserName(string productionName)
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

        private string GenerateParserLogic(Grammar.Grammar grammar, Production production, Expression expression, int index, bool single)
        {
            bool tokenize = production.Attributes.Contains("tokenize");
            string tokenizeBool = tokenize ? "true" : "false";

            int count = 0;
            var list = new List<string>();

            foreach (var symbol in expression.Symbols)
            {
                var @var = "_" + count;

                if (symbol is Empty)
                {
                    list.Add("Parser.Return(string.Empty)");
                }
                if (symbol is ManyExpression many)
                {
                    var onlySymbol = many.Inside.ToArray()[0].Symbols[0];

                    if (onlySymbol is ProductionIdentifier o)
                    {
                        string greedy = grammar.Productions[o.Value].Attributes.Contains("lazy") ? "false" : "true";
                        list.Add(Ref(GetParserName((o.Value))) + $".Many(greedy: {greedy})");
                    }
                    else if (onlySymbol is Literal)
                    {
                        list.Add("Parser.String(\"" + (((Literal)onlySymbol).Value) + "\")" + $".Many(greedy: true)");
                    }
                }

                if (symbol is OptionalExpression optional)
                {
                    var onlySymbol = optional.Inside.ToArray()[0].Symbols[0];
                    if (onlySymbol is ProductionIdentifier o)
                    {
                        string greedy = grammar.Productions[o.Value].Attributes.Contains("greedy") ? "true" : "false";

                        list.Add(Ref(GetParserName((o.Value))) + $".Optional(greedy: true)");
                    }
                    else if (onlySymbol is Literal)
                    {
                        list.Add("Parser.String(\"" + (((Literal)onlySymbol).Value) + $"\", true)" + ".Optional()");
                    }
                }

                if (symbol is ProductionIdentifier p)
                {
                    list.Add(Ref(GetParserName(p.Value)));
                }

                if (symbol is Literal literal)
                {
                    if (literal.Value.Length > 1)
                    {
                        list.Add("Parser.String(\"" + (literal.Value) + $"\", true)");
                    }
                    else
                    {
                        if (literal.Value == "'")
                        {
                            list.Add($"Parser.Char('\\'', true)");
                        }
                        else
                        {
                            list.Add("Parser.Char('" + (literal.Value) + $"', true)");
                        }
                    }
                }

                if (symbol is Pattern pattern)
                {
                    list.Add("Parser.Regex(\"" + pattern.Value + "\")" + (tokenize ? ".Token()" : ""));
                }

                count++;
            }

            StringBuilder result = new StringBuilder();
            result.Append("Parser.Sequence<CharToken, SyntaxNode>(\"" + GetParserName(production.Name.Value) + (single? "" : ("#" + index)) + "\", ");
            result.Append($"(args) => CreateSyntaxNode({tokenizeBool}, nameof(" + GetParserName(production.Name.Value) + "), args), ");

            for (var i = 0; i < count; i++)
            {
                result.Append("new Lazy<IParser<CharToken>>(() => " + list[i]);
                if (i != count - 1)
                {
                    result.Append("), ");
                }
            };

            result.Append("))");

            if (tokenize)
            {
                result.Append(".Token()");
            }

            return result.ToString();
        }
        private static string Ref(string productionName)
        {
            return $"{productionName}.Value";
        }
    }
}
