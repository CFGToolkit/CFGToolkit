using CFGToolkit.Grammar.Structure;
using CFGToolkit.ParserCombinator;
using CFGToolkit.ParserCombinator.Input;
using System.Collections.Generic;
using System.Linq;

namespace CFGToolkit.Grammar.Readers.W3CEBNF
{
    public class Parsers
    {
        public static IParser<CharToken, string> Identifier = Parser.Regex(@"[$A-Z_a-z][\-0-9A-Z_a-z]*");

        private static IParser<CharToken, string> Literal = Parser.Regex(@"'((?:\\.|[^\\'])*)'");

        public static IParser<CharToken, string> Char = Parser.AnyChar().Select(r => r.ToString());

        public static IParser<CharToken, string> CharRange =
                from _x1 in Char.Token()
                from _x2 in Parser.Char('-', true)
                from _x3 in Char.Except(Parser.Char(']'))
                select _x1.ToString() + _x2.ToString() + _x3.ToString();

        public static IParser<CharToken, string> CharCode = Parser.Regex(@"#x[0-9a-fA-F]+");

        public static IParser<CharToken, string> CharCodeRange =
         from _x1 in CharCode
         from _x2 in Parser.Char('-', true)
         from _x3 in CharCode
         select _x1 + " - " + _x3;

        public static IParser<CharToken, string> CharClass =
            from _x1 in Parser.Char('[')
            from _x2 in Parser.Char('^').Optional()
            from _x3 in CharCode.XOr(CharRange).XOr(CharCodeRange).XOr(Char.Except(Parser.Char(']'))).Repeat(1, null)
            from _x4 in Parser.Char(']')
            select _x1.ToString() + (_x2.IsEmpty ? "" : _x2.Get()) + string.Join(string.Empty, _x3) + _x4.ToString();

        public static IParser<CharToken, ISymbol> Item =

                (from _x1 in Parser.Char('(').TokenWithoutNewLines()
                 from _x2 in Alternatives
                 from _x3 in Parser.Char(')').TokenWithoutNewLines()
                 select (ISymbol)new GroupExpression { Inside = _x2 })
            .Or(
                from _x1 in Literal
                select (ISymbol)new Literal(_x1)
                )
            .Or(
                from _x1 in Identifier
                select (ISymbol)new Literal(_x1)
               )
            .Or(
                from _x1 in CharClass
                select (ISymbol)new Pattern() { Value = _x1 }
               )
            .Or(
                from _x1 in CharCode
                select (ISymbol)new Pattern() { Value = _x1 }
               );

        public static IParser<CharToken, ISymbol> AlternativeExpressionItem =
            (from _x1 in Item
             from _x2 in Parser.Char('?').TokenWithoutNewLines()
             select (ISymbol)new OptionalExpression() { Inside = new Expressions(new Expression(_x1)) })
            .XOr(
                from _x1 in Item
                from _x2 in Parser.Char('+').TokenWithoutNewLines()
                select (ISymbol)new ManyExpression() { Inside = new Expressions(new Expression(_x1)), AtLeastOnce = true }
            )
            .XOr(
                from _x1 in Item
                from _x2 in Parser.Char('*').TokenWithoutNewLines()
                select (ISymbol)new ManyExpression() { Inside = new Expressions(new Expression(_x1)), AtLeastOnce = false }
            )
            .XOr(
                from _x1 in Item
                select _x1
            );

        public static IParser<CharToken, List<ISymbol>> AlternativeExpression = AlternativeExpressionItem.TokenWithoutNewLines().Repeat(1, null);

        public static IParser<CharToken, Expressions> Alternatives =
            AlternativeExpression
            .DelimitedBy(Parser.String("|").Token())
            .Select(p => new Expressions(p.Select(r => new Expression(r))));

        public static IParser<CharToken, Production> Statement =
            from _x1 in Identifier
            from _x2 in Parser.String("::=", true)
            from _x3 in Alternatives
            select new Production() { Name = _x1, Alternatives = _x3 };

        public static IParser<CharToken, List<Production>> Productions =
                Statement.Token().Many();

        public static IParser<CharToken, Grammar> Grammar =
                from _x1 in Productions.End()
                select new Grammar() { Productions = _x1.ToDictionary(i => i.Name.Value, i => i) };
    }
}
