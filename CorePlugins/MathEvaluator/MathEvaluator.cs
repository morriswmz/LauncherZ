using System;
using System.Collections.Generic;
using System.Linq;
using CorePlugins.MathEvaluator.Symbols;

namespace CorePlugins.MathEvaluator
{
    
    public sealed class MathEvaluator
    {
        private Dictionary<string, Func<double, double>> _prefixEvaluators;
        private Dictionary<string, Func<double, double, double>> _infixEvaluators;
        private Dictionary<string, Func<List<double>, double>> _functions;

        private static readonly double Log2 = Math.Log(2.0);

        private Random _rand;

        private static MathEvaluator _instance;

        public static MathEvaluator Instance
        {
            get { return _instance ?? (_instance = new MathEvaluator()); }
        }

        private MathEvaluator()
        {
            InitEvaluators();
        }

        public bool TryEvaluate(string str, out double result)
        {
            try
            {
                Queue<Symbol> symbols = Tokenizer.Instance.Tokenize(str);
                ExpressionParser.Instance.LoadSymbols(symbols);
                Symbol root = ExpressionParser.Instance.ParseExpression(0);
                result = Evaluate(root); 
                return !double.IsNaN(result);
            }
            catch (Exception ex)
            {
                ExpressionParser.Instance.Clear();
                result = double.NaN;
                return false;
            }

        }

        private double Evaluate(Symbol root)
        {
            if (root is LiteralSymbol)
            {
                return (root as LiteralSymbol).Value;
            }
            if (root is OperatorSymbol)
            {
                var pio = root as OperatorSymbol;
                if (pio.Second == null)
                {
                    // prefix only
                    return _prefixEvaluators[pio.Id](Evaluate(pio.First));
                }
                else
                {
                    return _infixEvaluators[pio.Id](Evaluate(pio.First), Evaluate(pio.Second));
                }
            }
            if (root is LBracketSymbol)
            {
                var lbs = root as LBracketSymbol;
                if (_functions.ContainsKey(lbs.FunctionName))
                {
                    // as function call
                    List<double> args = lbs.Arguments.Select(Evaluate).ToList();
                    return _functions[lbs.FunctionName](args);
                }
                throw new Exception(string.Format("Undefined function name \"{0}\"", lbs.FunctionName));
            }
            throw new Exception(string.Format("Unable to evaluate symbol \"{0}\"", root.Id));
        }

        private void InitEvaluators()
        {
            _prefixEvaluators = new Dictionary<string, Func<double, double>>
            {
                {"+", d => d},
                {"-", d => -d}
            };

            _infixEvaluators = new Dictionary<string, Func<double, double, double>>
            {
                {"+", (a, b) => a + b},
                {"-", (a, b) => a - b},
                {"*", (a, b) => a*b},
                {"/", (a, b) => a/b},
                {"%", (a, b) => a%b},
                {"^", Math.Pow},
                {">>", (a, b) => (double) ((int) a >> (int) b)},
                {"<<", (a, b) => (double) ((int) a << (int) b)}
            };

            _rand = new Random();
            _functions = new Dictionary<string, Func<List<double>, double>>
            {
                {"sin", args => Math.Sin(args[0])},
                {"cos", args => Math.Cos(args[0])},
                {"tan", args => Math.Tan(args[0])},
                {"sinh", args => Math.Sinh(args[0])},
                {"cosh", args => Math.Cosh(args[0])},
                {"tanh", args => Math.Tanh(args[0])},
                {"arcsin", args => Math.Asin(args[0])},
                {"arccos", args => Math.Acos(args[0])},
                {"arctan", args => Math.Atan(args[0])},
                {"log2", args => (Math.Log(args[0])/Log2)},
                {"log10", args => Math.Log10(args[0])},
                {"exp", args => Math.Exp(args[0])},
                {"sqrt", args => Math.Sqrt(args[0])},
                {"floor", args => Math.Floor(args[0])},
                {"round", args => Math.Round(args[0])},
                {"log", args =>
                    {
                        if (args.Count == 1)
                        {
                            return Math.Log(args[0]);
                        }
                        else
                        {
                            return Math.Log(args[0])/Math.Log(args[1]);
                        }
                    }
                },
                {"rand", args =>
                    {
                        switch (args.Count)
                        {
                            case 0:
                                return _rand.NextDouble();
                            case 1:
                                return _rand.Next((int) args[0]);
                            default:
                                var res = _rand.Next((int) (args[1] - args[0]));
                                res += (int) args[0];
                                return (double) res;
                        }
                    }
                }
            };
        }

        
    }
}
