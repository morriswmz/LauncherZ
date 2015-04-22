namespace CorePlugins.MathEvaluator.Symbols
{
    /// <summary>
    /// Represents a literal symbol.
    /// </summary>
    sealed class LiteralSymbol : Symbol
    {
        public double Value { get; set; }

        public LiteralSymbol(double value)
            : base(Symbol.LiteralId)
        {
            Value = value;
        }

        public override Symbol NUD(ExpressionParser exp)
        {
            return this;
        }

    }
}
