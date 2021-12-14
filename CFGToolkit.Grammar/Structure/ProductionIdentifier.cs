namespace CFGToolkit.Grammar.Structure
{
    public class ProductionIdentifier : ISymbol
    {
        public ProductionIdentifier(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public override string ToString()
        {
            return Value;
        }

        public static implicit operator ProductionIdentifier(string v)
        {
            return new ProductionIdentifier(v);
        }
    }
}
