using System;

namespace CorePlugins.MathEvaluator.Symbols
{
    class Symbol
    {
        private readonly string id;

        public const string LiteralId = "(l)";
        public const string NameId = "(n)";
        public const string EndId = "(e)";

        public string Id { get { return id; } }
        
        public Symbol(string sid)
        {
            id = sid;
        }

        public virtual Symbol NUD(ExpressionParser exp)
        {
            throw new Exception(string.Format("Syntax error near {0}.", id));
        }

        public virtual Symbol LED(ExpressionParser exp, Symbol left)
        {
            throw new Exception(string.Format("Unknown operator {0}", id));
        }

    }

}
