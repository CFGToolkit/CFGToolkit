using CFGToolkit.ParserCombinator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CFGToolkit.Grammar.Readers.VerilogEBNF
{
    public class PreprocessorParsers
    {
        public static IParser<CharToken, string> Identifier = Parse.Regex(@"[$A-Z_a-z][\-0-9A-Z_a-z]*").Named(nameof(Identifier));
        public static IParser<CharToken, string> ProductionAttribute = (
            from start in Parse.Char('[')
            from name in Parse.AnyChar().Except(Parse.Char(']')).Many().Text()
            from end in Parse.Char(']')
            select name).Named(nameof(ProductionAttribute));

        public static IParser<CharToken, string> Production =
            (from name in Identifier
             from attributes in ProductionAttribute.Many()
             from spaces1 in Parse.WhiteSpace.Many()
             from equal in Parse.String("::=")
             from spaces2 in Parse.WhiteSpace.Many()
             from @else in Parse.Regex("((?!(\r?\n){2}).)+", RegexOptions.Singleline)
             from lines in Parse.LineEnd.Many()
             select name).Named(nameof(Production));

        public static IParser<CharToken, string> Comment = (
            from _1 in Parse.String("//")
            from _2 in Parse.AnyChar().Except(Parse.LineEnd).Many().Text()
            from lines in Parse.LineEnd.Token().Many()
            select _2).Named(nameof(Comment));

        public static IParser<CharToken, (string, bool)> Statement =
            (from s in Production.Select(c => (c, true))
            .Or(Comment.Select(c => (c, false)))
             select s).Named(nameof(Statement));

        public static IParser<CharToken, IEnumerable<string>> ProductionNames =
            from _x1 in Statement.Token().Many()
            from _x2 in Parse.String("#END")
            select _x1.Where(a => a.Item2).Select(a => a.Item1);
    }
}
