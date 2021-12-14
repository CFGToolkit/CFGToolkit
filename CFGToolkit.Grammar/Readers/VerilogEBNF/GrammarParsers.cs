using CFGToolkit.Grammar.Structure;
using CFGToolkit.ParserCombinator;
using System.Collections.Generic;
using System.Linq;

namespace CFGToolkit.Grammar.Readers.VerilogEBNF
{
    public class GrammarParsers
    {
        public static GrammarInfo g { get; set; }

        public static IParser<CharToken, string> Identifier = Parse.RegexExt(@"[$A-Z_a-z][\-0-9A-Z_a-z]*", val => g.IsIdentifier(val));

        private static IParser<CharToken, string> LiteralWithQuotes = Parse.RegexExt(@"""((?:\\.|[^\\""])*)""", val => !g.IsIdentifier(val));

        private static IParser<CharToken, string> LiteralWithoutQuotes = Parse.RegexExt(@"[^ \{\}\|\[\]\r\n]+", val => !g.IsIdentifier(val));

        private static IParser<CharToken, string> Pattern = Parse.RegexExt(@"\[[^\r\n \|]+\]", val => !g.IsIdentifier(val.Trim('[', ']')));

        private static IParser<CharToken, OptionalExpression> OptionalExpression(HashSet<string> attributes)
        {
            return from a in Parse.Char('[').TokenWithoutNewLines()
                   from b in Expressions(attributes)
                   from c in Parse.Char(']').TokenWithoutNewLines()
                   select new OptionalExpression() { Inside = b };
        }

        public static IParser<CharToken, ManyExpression> RepeatedExpression(HashSet<string> attributes)
        {
            return from a in Parse.Char('{').TokenWithoutNewLines()
                   from b in Expressions(attributes)
                   from c in Parse.Char('}').TokenWithoutNewLines()
                   select new ManyExpression() { Inside = b };
        }


        public static IParser<CharToken, ISymbol> Symbol(HashSet<string> attributes) =>
            Pattern.Select(a => (ISymbol)new Pattern { Value = a })
            .XOr(OptionalExpression(attributes).Cast<CharToken, ISymbol, OptionalExpression>())
            .XOr(RepeatedExpression(attributes).Cast<CharToken, ISymbol, ManyExpression>())
            .XOr(Identifier.Select(a => (ISymbol)new ProductionIdentifier(a)))
            .XOr(LiteralWithQuotes.Select(a => (ISymbol)new Literal(a.Substring(1, a.Length - 2))))
            .XOr(LiteralWithoutQuotes.Select(a => (ISymbol)new Literal(a)));

        public static IParser<CharToken, Expression> Expression(HashSet<string> attributes)
        {
            if (attributes.Contains("pattern"))
            {
                return LiteralWithQuotes.Select(a => new Expression { Symbols = new List<ISymbol> { new Pattern() { Value = a.Substring(1, a.Length - 2) } } });
            }
            else
            {
                return Symbol(attributes).TokenWithoutNewLines().Many().Select(p => new Expression() { Symbols = new List<ISymbol>(p) });
            }
        }

        private static IParser<CharToken, Expressions> Expressions(HashSet<string> attributes)
            => Expression(attributes)
                .DelimitedBy(Parse.String("|").Token())
                .Select(p => new Expressions(p));

        public static IParser<CharToken, string> ProductionAttribute =
            from start in Parse.Char('[')
            from name in Parse.AnyChar().Except(Parse.Char(']')).Many().Text()
            from end in Parse.Char(']')
            select name;

        public static IParser<CharToken, Production> Production =
            from name in Identifier
            from attributes in ProductionAttribute.Many()
            from spaces1 in Parse.WhiteSpace.Many()
            from equal in Parse.String("::=")
            from spaces2 in Parse.WhiteSpace.Many()
            from expressions in Expressions(new HashSet<string>(attributes))
            from linesEnd in Parse.LineEnd.Many().Token()
            select new Production() { Attributes = new HashSet<string>(attributes), Name = new ProductionIdentifier(name), Alternatives = expressions };

        public static IParser<CharToken, Comment> Comment =
            from _1 in Parse.String("//")
            from _2 in Parse.AnyChar().Except(Parse.LineEnd).Many().Text()
            from _3 in Parse.LineEnd.Many().Token()
            select new Comment { Text = _2 };

        public static IParser<CharToken, IStatement> Statement =
            Production.Cast<CharToken, IStatement, Production>()
            .XOr(Comment.Cast<CharToken, IStatement, Comment>());

        public static IParser<CharToken, Grammar> Grammar(GrammarInfo info)
        {
            g = info;
            return
                from _x1 in Statement.Token().Many()
                from _x2 in Parse.String("#END")
                select new Grammar(_x1.Where(i => i is Production).Cast<Production>());
        }
    }
}
