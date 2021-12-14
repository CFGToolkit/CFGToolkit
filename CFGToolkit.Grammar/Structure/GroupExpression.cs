namespace CFGToolkit.Grammar.Structure
{
    public class GroupExpression : ISymbol
    {
        public Expressions Inside { get; set; }

        public override string ToString()
        {
            return "{" + Inside + "}";
        }
    }
}
