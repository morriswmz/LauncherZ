using System;
using System.Collections.Generic;

namespace CorePlugins.MathEvaluator.Symbols
{

    /// <summary>
    /// Represents "("
    /// This can be either a function call or normal bracket.
    /// </summary>
    sealed class LBracketSymbol : Symbol
    {
        public string FunctionName { get; set; }
        public List<Symbol> Arguments { get; set; }

        public LBracketSymbol()
            : base("(")
        {
        }

        public LBracketSymbol(List<Symbol> args):base("(")
        {
            Arguments = args;
        }

        public override Symbol LED(ExpressionParser exp, Symbol left)
        {
            if (left is NameSymbol)
            {
                FunctionName = (left as NameSymbol).Name;
                Arguments = new List<Symbol>();
                if (exp.CurrentSymbolId != ")")
                {
                    while (true)
                    {
                        Arguments.Add(exp.ParseExpression(0));
                        if (exp.CurrentSymbolId != ",") break;
                        exp.Expect(",");
                    }
                }
                exp.Expect(")");
                return this;
            }
            else
            {
                throw new Exception("Function name expeced before \"(\"");
            }
        }

        public override Symbol NUD(ExpressionParser exp)
        {
            Symbol subExpression = exp.ParseExpression(0);
            exp.Expect(")");
            return subExpression;
        }

    }
}
