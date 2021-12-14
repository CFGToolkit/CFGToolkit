using System;

namespace CFGToolkit.ParserCombinatorGenerator
{
    public class ClassStaticMember
    {
        public string Type { get; set; }

        public string Name { get; set; }

        public string Logic { get; set; }

        public override string ToString()
        {
            return "public static " +  Type + " " + Name + " = " + Environment.NewLine + Logic;
        }
    }
}
