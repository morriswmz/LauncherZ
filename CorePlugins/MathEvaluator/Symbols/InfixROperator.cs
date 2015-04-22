namespace CorePlugins.MathEvaluator.Symbols
{
    /// <summary>
    /// Represents an binary operator in the middle of the expression,
    /// but with right binding priority.
    /// e.g. ^
    /// </summary>
    sealed class InfixROperator : OperatorSymbol
    {

        public InfixROperator(string sid)
            : base(sid)
        {
        }

        public override Symbol LED(ExpressionParser exp, Symbol left)
        {
            First = left;
            Second = exp.ParseExpression(exp.LEDPTable[Id] - 1);
            return this;
        }
    }
}
