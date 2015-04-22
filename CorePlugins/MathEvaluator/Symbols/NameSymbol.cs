namespace CorePlugins.MathEvaluator.Symbols
{
    /// <summary>
    /// Represents a name.
    /// e.g. function name
    /// </summary>
    sealed class NameSymbol : Symbol
    {
        public string Name { get; set; }

        public NameSymbol()
            : base(Symbol.NameId)
        {
        }

        public NameSymbol(string name)
            : base(Symbol.NameId)
        {
            Name = name;
        }

        public override Symbol NUD(ExpressionParser exp)
        {
            return this;
        }

    }
}
