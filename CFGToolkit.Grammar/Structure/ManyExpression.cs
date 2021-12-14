namespace CFGToolkit.Grammar.Structure
{
    public class ManyExpression : ISymbol
    {
        public Expressions Inside { get; set; }

        public bool AtLeastOnce { get; set; } = false;

        public override string ToString()
        {
            return "{" + Inside + "}";
        }
    }
}
