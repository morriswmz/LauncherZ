namespace CorePlugins.MathEvaluator.Symbols
{
    /// <summary>
    /// Represents a binary operator which can be also used as prefix operator.
    /// e.g. +, -
    /// </summary>
    sealed class PreInfixOperator : OperatorSymbol
    {
        public PreInfixOperator(string sid)
            : base(sid)
        {
        }

        public override Symbol LED(ExpressionParser exp, Symbol left)
        {
            First = left;
            Second = exp.ParseExpression(exp.LEDPTable[Id]);
            return this;
        }

        public override Symbol NUD(ExpressionParser exp)
        {
            First = exp.ParseExpression(exp.NUDPTable[Id]);
            return this;
        }

    }
}
