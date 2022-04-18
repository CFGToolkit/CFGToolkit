using CFGToolkit.ParserCombinator;
using CFGToolkit.ParserCombinator.Input;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CFGToolkit.Grammar.Readers.VerilogEBNF
{
    public class PreprocessorParsers
    {
        public static IParser<CharToken, string> Identifier = Parser.Regex(@"[$A-Z_a-z][\-0-9A-Z_a-z]*").Named(nameof(Identifier));
        
        public static IParser<CharToken, KeyValuePair<string, string>> ProductionTag = (
            from start in Parser.Char('[')
            from name in Parser.AnyChar().Except(Parser.Char(']').Or(Parser.Char('='))).Many().Text()
            from end in Parser.Char(']')
            select new KeyValuePair<string,string>(name, null))
            .Or(
                from start in Parser.Char('[')
                from name in Parser.AnyChar().Except(Parser.Char(']').Or(Parser.Char('='))).Many().Text()
                from _ in Parser.Char('=')
                from value in Parser.AnyChar().Except(Parser.Char(']').Or(Parser.Char('='))).Many().Text()
                from end in Parser.Char(']')
                select new KeyValuePair<string, string>(name, value)
            ).Named(nameof(ProductionTag));

        public static IParser<CharToken, string> Production =
            (from name in Identifier
             from tags in ProductionTag.Many()
             from spaces1 in Parser.WhiteSpace.Many()
             from equal in Parser.String("::=")
             from spaces2 in Parser.WhiteSpace.Many()
             from @else in Parser.Regex("((?!(\r?\n){2}).)+", RegexOptions.Singleline)
             from lines in Parser.LineEnd.Many()
             select name).Named(nameof(Production));

        public static IParser<CharToken, string> Comment = (
            from _1 in Parser.String("//")
            from _2 in Parser.AnyChar().Except(Parser.LineEnd).Many().Text()
            from lines in Parser.LineEnd.Token().Many()
            select _2).Named(nameof(Comment));

        public static IParser<CharToken, (string, bool)> Statement =
            (from s in Production.Select(c => (c, true))
            .Or(Comment.Select(c => (c, false)))
             select s).Named(nameof(Statement));

        public static IParser<CharToken, IEnumerable<string>> ProductionNames =
            from _x1 in Statement.Token().Many()
            from _x2 in Parser.String("#END")
            select _x1.Where(a => a.Item2).Select(a => a.Item1);
    }
}
