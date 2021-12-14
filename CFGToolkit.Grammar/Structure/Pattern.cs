namespace CFGToolkit.Grammar.Structure
{
    public class Pattern : ISymbol
    {
        public string Value { get; set; }

        public override string ToString()
        {
            return Value;
        }
    }
}
