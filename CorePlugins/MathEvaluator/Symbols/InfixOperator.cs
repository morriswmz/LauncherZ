namespace CorePlugins.MathEvaluator.Symbols
{
    /// <summary>
    /// Represents an binary operator in the middle of the expression.
    /// e.g. *, /
    /// </summary>
    sealed class InfixOperator : OperatorSymbol
    {

        public InfixOperator(string sid)
            : base(sid)
        {
        }

        public override Symbol LED(ExpressionParser exp, Symbol left)
        {
            First = left;
            Second = exp.ParseExpression(exp.LEDPTable[Id]);
            return this;
        }
    }
}
