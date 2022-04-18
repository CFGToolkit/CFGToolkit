using CFGToolkit.Grammar.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CFGToolkit.ParserCombinatorGenerator
{
    public class CharParsersGenerator : BaseGenerator
    {
        private Dictionary<string, (string, bool, long)> _stringParsers = new Dictionary<string, (string, bool, long)>();
        private Dictionary<string, (string, bool, long)> _charParsers = new Dictionary<string, (string, bool, long)>();
        private Dictionary<string, (string, bool, long)> _regexParsers = new Dictionary<string, (string, bool, long)>();
        private HashSet<long> _usedIds = new HashSet<long>();

        public override List<ClassStaticMember> GeneratePasers(Grammar.Grammar grammar)
        {
            var result = new List<ClassStaticMember>();

            foreach (var production in grammar.Productions.Values)
            {
                result.Add(GenerateParser(grammar, production));
            }

            foreach (var stringParser in _stringParsers)
            {
                result.Add(GenerateStringParser(stringParser.Key, stringParser.Value));
            }

            foreach (var charParser in _charParsers)
            {
                result.Add(GenerateCharParser(charParser.Key, charParser.Value));
            }

            foreach (var regexParser in _regexParsers)
            {
                result.Add(GenerateRegexParser(regexParser.Key, regexParser.Value));
            }

            return result;
        }

        private ClassStaticMember GenerateRegexParser(string name, (string, bool, long) parser)
        {
            StringBuilder logic = new StringBuilder();
            logic.Append("Parser.Regex(\"" + (parser.Item1) + $"\", {parser.Item2.ToString().ToLower()}).Cached({parser.Item3})");
            logic.Append(@".Tag(""keyword"")");
            return new ClassStaticMember { Name = name, Logic = "  new Lazy<IParser<CharToken, string>>(() => " + logic.ToString() + ");", Type = "Lazy<IParser<CharToken, string>>" };
        }


        private ClassStaticMember GenerateStringParser(string name, (string, bool, long) parser)
        {
            StringBuilder logic = new StringBuilder();
            logic.Append("Parser.String(\"" + (parser.Item1) + $"\", true).Cached({parser.Item3})");
            logic.Append(@".Tag(""keyword"")");
            return new ClassStaticMember { Name = name, Logic = "  new Lazy<IParser<CharToken, string>>(() => " + logic.ToString() + ");", Type = "Lazy<IParser<CharToken, string>>" };
        }

        private ClassStaticMember GenerateCharParser(string name, (string, bool, long) parser)
        {
            StringBuilder logic = new StringBuilder();

            if (parser.Item1 == "'")
            {
                parser.Item1 = "\'";
            }
            logic.Append("Parser.Char('" + (parser.Item1) + $"', true).Cached({parser.Item3})");
            logic.Append(@".Tag(""keyword"")");
            return new ClassStaticMember { Name = name, Logic = "  new Lazy<IParser<CharToken, char>>(() => " + logic.ToString() + ");", Type = "Lazy<IParser<CharToken, char>>" };
        }

        private ClassStaticMember GenerateParser(Grammar.Grammar grammar, Production production)
        {
            StringBuilder logic = new StringBuilder();
            int index = 0;
            bool isXor = production.Tags.ContainsKey("xor");

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

        private string GenerateParserLogic(Grammar.Grammar grammar, Production production, Expression expression, int index, bool single)
        {
            bool nodeTokenize = production.Tags.ContainsKey("nodeTokenize");
            bool tokenTokenize = !production.Tags.ContainsKey("!tokenTokenize");

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
                        string greedy = grammar.Productions[o.Value].Tags.ContainsKey("lazy") ? "false" : "true";
                        list.Add(Ref(GetParserName((o.Value))) + $".Many(greedy: {greedy})");
                    }
                    else
                    {
                        throw new System.Exception("Unsupported grammar");
                    }
                }

                if (symbol is OptionalExpression optional)
                {
                    var onlySymbol = optional.Inside.ToArray()[0].Symbols[0];
                    if (onlySymbol is ProductionIdentifier o)
                    {
                        string greedy = grammar.Productions[o.Value].Tags.ContainsKey("greedy") ? "true" : "false";

                        list.Add(Ref(GetParserName((o.Value))) + $".Optional(greedy: true)");
                    }
                    else
                    {
                        throw new System.Exception("Unsupported grammar");
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
                        list.Add(Ref(GetStringParser(literal.Value, tokenTokenize)));
                    }
                    else
                    {
                        list.Add(Ref(GetCharParser(literal.Value, tokenTokenize)));
                    }
                }

                if (symbol is Pattern pattern)
                {
                    list.Add(Ref(GetRegexParser(pattern.Value, tokenTokenize)));
                }

                count++;
            }

            StringBuilder result = new StringBuilder();
            result.Append("Parser.Sequence<CharToken, SyntaxNode>(\"" + GetParserName(production.Name.Value) + (single? "" : ("#" + index)) + "\", ");
            result.Append($"(args) => CreateSyntaxNode({nodeTokenize.ToString().ToLower()}, {tokenTokenize.ToString().ToLower()}, nameof(" + GetParserName(production.Name.Value) + "), args), ");

            for (var i = 0; i < count; i++)
            {
                result.Append("new Lazy<IParser<CharToken>>(() => " + list[i]);
                if (i != count - 1)
                {
                    result.Append("), ");
                }
            };

            result.Append("))");

            if (nodeTokenize)
            {
                result.Append(".Token()");
            }

            foreach (var tag in production.Tags)
            {
                if (tag.Value != null)
                {
                    result.Append($@".Tag(""{tag.Key}"", ""{tag.Value}"")");
                }
                else
                {
                    result.Append($@".Tag(""{tag.Key}"")");
                }
            }

            result.Append($@".Tag(""index"", ""{index}"")");
            result.Append($@".Tag(""nt"", NonTerminals." + production.Name + ")");
            return result.ToString();
        }
        private static string Ref(string productionName)
        {
            return $"{productionName}.Value";
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
        private string GetStringParser(string keyword, bool tokenize)
        {
            var name = GetKeywordParserName(keyword, tokenize, out var hash);

            if (!_stringParsers.ContainsKey(name))
            {
                _stringParsers.Add(name, (keyword, tokenize, hash));
                if (_usedIds.Contains(hash))
                {
                    throw new System.Exception("A problem with hashing");
                }
                _usedIds.Add(hash);
            }
            return name;
        }

        private string GetRegexParser(string keyword, bool tokenize)
        {
            var name = GetKeywordParserName(keyword, tokenize, out var hash);

            if (!_regexParsers.ContainsKey(name))
            {
                _regexParsers.Add(name, (keyword, tokenize, hash));

                if (_usedIds.Contains(hash))
                {
                    throw new System.Exception("A problem with hashing");
                }
                _usedIds.Add(hash);

            }
            return name;
        }

        private string GetCharParser(string keyword, bool tokenize)
        {
            var name = GetKeywordParserName(keyword, tokenize, out var hash);

            if (!_charParsers.ContainsKey(name))
            {
                _charParsers.TryAdd(name, (keyword, tokenize, hash));

                if (_usedIds.Contains(hash))
                {
                    throw new System.Exception("A problem with hashing");
                }
                _usedIds.Add(hash);
            }
            return name;
        }

        private string GetKeywordParserName(string keyword, bool tokenize, out long hash)
        {
            hash = Math.Abs((keyword + tokenize).GetHashCode());
         
            return "_keyword_" + hash + "_" + tokenize;
        }
    }
}
