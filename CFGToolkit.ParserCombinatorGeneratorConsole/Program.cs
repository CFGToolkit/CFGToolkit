﻿using CFGToolkit.GrammarDefinition;
using CFGToolkit.GrammarDefinition.Algorithms.Finders;
using CFGToolkit.GrammarDefinition.Algorithms.Reductions;
using CFGToolkit.ParserCombinatorGenerator;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace CFGToolkit.ParserCombinatorGeneratorConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 4)
            {
                System.Console.WriteLine("CFGToolkit.Console.exe <path> <type> <namespace> <className>");
                return;
            }

            var path = args[0];
            var type = args[1];
            var @namespace = args[2];
            var @className = args[3];

            var txt = File.ReadAllText(path);
            Stopwatch s = new Stopwatch();
            s.Start();

            Grammar grammar = null;
            switch (type)
            {
                case "bnf":
                    grammar = new GrammarDefinition.Readers.BNF.Reader().Read(txt);
                    break;
                case "w3c":
                    grammar = new GrammarDefinition.Readers.W3CEBNF.Reader().Read(txt);
                    break;
                default:
                    System.Console.WriteLine("Unknown type of parser. Allowed values: bnf and w3c");
                    return;
            }

            s.Stop();
            System.Console.WriteLine(s.Elapsed);
            var reductor = new CombinedReductor(new ExpressionReductor[] { new ManySingleReductor(), new OptionalSingleReductor() });

            var reducted = reductor.Reduct(grammar);
            var lefts = new LeftRecursionFinder().FindLeftRecursion(reducted, false).ToList();
            var leftReductor = new LeftRecursionReductor();
            var withoutLeft = leftReductor.Reduct(reducted, p => lefts.Contains(p));
            var emptyReductor = new EmptyReductor();

            var withPrefixReductor = new PrefixReductor();
            var withPrefix = withPrefixReductor.Reduct(withoutLeft);

            var generator = new CharParsersGenerator();
            var @class = generator.GenerateFile(@namespace, @className, withPrefix);

            string outputPath = path + ".generated.cs";
            File.WriteAllText(outputPath, @class);
            System.Console.WriteLine("File created: " + outputPath);
        }
    }
}
