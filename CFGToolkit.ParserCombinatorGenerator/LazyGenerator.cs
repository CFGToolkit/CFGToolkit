using CFGToolkit.Grammar.Structure;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CFGToolkit.ParserCombinatorGenerator
{
    public class LazyGenerator : BaseGenerator
    {
        public override List<ClassStaticMember> GeneratePasers(Grammar.Grammar grammar)
        {
            var result = new List<ClassStaticMember>();

            foreach (var production in grammar.Productions.Values)
            {
                result.Add(GenerateParser(grammar, production));
            }
            result.Add(new ClassStaticMember()
            {
                Name = "CreateSyntaxNode",
                Type = "Func<bool, string, (string valueParserName, object value)[], SyntaxNode>",
                Logic = @"(tokenize, name, args) => {
                    var result = new (string valueParserName, object value)[args.Length];
                    for (var i = 0; i < args.Length; i++)
                    {
                        var res = args[i].value;
        
                        if (res is IOption<object> c)
                        {
                            result[i].value = new SyntaxNodeOption(c.GetOrDefault());
                        }
                        else
                        {
                            result[i].value = res;
                        }

                        result[i].valueParserName = args[i].valueParserName;
                    }
                    return new SyntaxNode(name, tokenize, result);
                };"
            });
            return result;
        }

        private ClassStaticMember GenerateParser(Grammar.Grammar grammar, Production production)
        {
            var spaces = " ";
            StringBuilder logic = new StringBuilder();
            int index = 0;
            foreach (var alternative in production.Alternatives)
            {
                var parser = GenerateParserLogic(grammar, production, alternative, index);

                if (parser == "") continue;

                if (index != 0)
                {
                    if (production.Attributes.Contains("xor"))
                    {
                        logic.Append($"\r\n{spaces}  .XOr(" + parser);
                    }
                    else
                    {
                        logic.Append($"\r\n{spaces}  .Or(" + parser);
                    }
                    
                }
                else
                {
                    logic.Clear();
                    logic.Append(spaces + parser);
                }
                index++;
            }

            for (var i = 0; i < index - 1; i++)
            {
                logic.Append(")");
            }

            return new ClassStaticMember { Name = GetParserName(production.Name.Value), Logic = "  new Lazy<IParser<CharToken, SyntaxNode>>(() => " + logic.ToString() + ".Named(\"" + production.Name + "\"));", Type = "Lazy<IParser<CharToken, SyntaxNode>>" };
        }

        private static string GetParserName(string productionName)
        {
            var s = productionName.Replace("[no-token]", "").Replace("[pattern]", "");

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

        private string GenerateParserLogic(Grammar.Grammar grammar, Production production, Expression expression, int index)
        {
            bool noToken = production.Attributes.Contains("no-token");
            string tokenized = !noToken ? "true" : "false";
            int count = 0;
            var list = new List<string>();

            foreach (var symbol in expression.Symbols)
            {
                var @var = "_" + count;

                if (symbol is Empty)
                {
                    list.Add("Parse.Return(string.Empty)");
                }
                if (symbol is ManyExpression many)
                {
                    var onlySymbol = many.Inside.ToArray()[0].Symbols[0];

                    if (onlySymbol is ProductionIdentifier o)
                    {
                        bool noToken2 = grammar.Productions[o.Value].Attributes.Contains("no-token");
                        string greedy = grammar.Productions[o.Value].Attributes.Contains("lazy") ? "false" : "true";

                        list.Add(Ref(GetParserName((o.Value))) + $".Many(greedy: {greedy})" + (noToken && noToken2 ? "" : ".Token()"));
                    }
                    else if (onlySymbol is Literal)
                    {
                        list.Add("Parse.String(\"" + (((Literal)onlySymbol).Value) + "\").Text()" + $".Many(greedy: true)" + (noToken ? "" : ".Token()"));
                    }
                }

                if (symbol is OptionalExpression optional)
                {
                    var onlySymbol = optional.Inside.ToArray()[0].Symbols[0];
                    if (onlySymbol is ProductionIdentifier o)
                    {
                        bool noToken2 = grammar.Productions[o.Value].Attributes.Contains("no-token");
                        string greedy = grammar.Productions[o.Value].Attributes.Contains("greedy") ? "true" : "false";

                        list.Add(Ref(GetParserName((o.Value))) + $".Optional(greedy: {greedy})" + (noToken && noToken2 ? "" : ".Token()"));
                    }
                    else if (onlySymbol is Literal)
                    {
                        list.Add("Parse.String(\"" + (((Literal)onlySymbol).Value) + $"\", {tokenized}).Text()" + ".Optional()");
                    }
                }

                if (symbol is ProductionIdentifier p)
                {
                    bool noToken2 = grammar.Productions[p.Value].Attributes.Contains("no-token");

                    list.Add(Ref(GetParserName(p.Value)) + (noToken && noToken2 ? "" : ".Token()"));
                }

                if (symbol is Literal literal)
                {
                    if (literal.Value.Length > 1)
                    {
                        list.Add("Parse.String(\"" + (literal.Value) + $"\", {tokenized}).Text()");
                    }
                    else
                    {
                        if (literal.Value == "'")
                        {
                            list.Add($"Parse.Char('\\'', {tokenized})");
                        }
                        else
                        {
                            list.Add("Parse.Char('" + (literal.Value) + $"', {tokenized})");
                        }                        
                    }
                }

                if (symbol is Pattern pattern)
                {
                    list.Add("Parse.Regex(\"" + pattern.Value + "\")" + (noToken ? "" : ".Token()"));
                }

                count++;
            }

            StringBuilder result = new StringBuilder();
            result.Append("Parse.Sequence<CharToken, SyntaxNode>(\"" + GetParserName(production.Name.Value) +  "#" + index + "\", ");
            result.Append($"(args) => CreateSyntaxNode({tokenized}, nameof(" + GetParserName(production.Name.Value) + "), args), ");

            for (var i = 0; i < count; i++)
            {
                result.Append("new Lazy<IParser<CharToken>>(() => " + list[i]);
                if (i != count - 1)
                {
                    result.Append("), ");
                }
            };

            result.Append("))");

            return result.ToString();
        }
        private static string Ref(string productionName)
        {
            return $"{productionName}.Value";
        }
    }
}
