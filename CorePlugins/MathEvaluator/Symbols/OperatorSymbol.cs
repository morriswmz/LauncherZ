namespace CorePlugins.MathEvaluator.Symbols
{
    /// <summary>
    /// Represents an operator
    /// </summary>
    class OperatorSymbol : Symbol
    {
        public Symbol First { get; set; }
        public Symbol Second { get; set; }

        public OperatorSymbol(string sid)
            : base(sid)
        {
        }
    }
}
