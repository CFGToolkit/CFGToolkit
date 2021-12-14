namespace CFGToolkit.Grammar.Structure
{
    public class OptionalExpression : ISymbol
    {
        public Expressions Inside { get; set; }

        public override string ToString()
        {
            return "[" + Inside + "]";
        }
    }
}
