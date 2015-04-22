using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CorePlugins.MathEvaluator.Symbols;

namespace CorePlugins.MathEvaluator
{
    sealed class Tokenizer
    {

        #region Private Fields

        private List<TokenDefinition> _tokenDefinitions;

        #endregion

        #region Singleton Constructor

        private static Tokenizer _instance;

        public static Tokenizer Instance
        {
            get { return _instance ?? (_instance = new Tokenizer()); }
        }
        
        private Tokenizer()
        {
            AddTokenDefinitions();
        }
        
        #endregion

        #region Public Methods

        public Queue<Symbol> Tokenize(string str)
        {
            var result = new Queue<Symbol>();
            int n = _tokenDefinitions.Count;

            while (str.Length > 0)
            {
                bool consumed = false;
                for (int i = 0; i < n; i++)
                {
                    TokenDefinition td = _tokenDefinitions[i];
                    Match m = td.Pattern.Match(str);
                    if (m.Success)
                    {
                        consumed = true;
                        Symbol newSymbol = td.Generator(m.Captures[0].Value);
                        if (newSymbol != null)
                        {
                            result.Enqueue(newSymbol);
                        }
                        str = str.Substring(m.Length);
                        break;
                    }
                }
                // undefined syntax
                if (!consumed) {
                    throw new Exception(string.Format("Unknown syntax near \"{0}\"", str));
                }
            }
            // and end symbol
            result.Enqueue(new Symbol(Symbol.EndId));
            return result;
        }

        #endregion


        #region Private Methods

        private void AddTokenDefinitions()
        {
            _tokenDefinitions = new List<TokenDefinition>();
            _tokenDefinitions.Add(new TokenDefinition(
                "whitespace",
                new Regex(@"^(\s+)"),
                m => null // ignore white space
            ));
            _tokenDefinitions.Add(new TokenDefinition(
                "comma",
                new Regex(@"^(,)"),
                m => new Symbol(m)
            ));
            _tokenDefinitions.Add(new TokenDefinition(
                "operator",
                new Regex(@"^([+\-*\/\^%]|>>|<<)"),
                m =>
                {
                    switch (m)
                    {
                        case "-":
                        case "+":
                            return new PreInfixOperator(m);
                        case "^":
                            return new InfixROperator(m);
                        default:
                            return new InfixOperator(m);
                    }
                }
            ));
            _tokenDefinitions.Add(new TokenDefinition(
                "lbracket",
                new Regex(@"^(\()"),
                m => new LBracketSymbol()
            ));
            _tokenDefinitions.Add(new TokenDefinition(
                "rbracket",
                new Regex(@"^(\))"),
                m => new Symbol(m)
            ));
            _tokenDefinitions.Add(new TokenDefinition(
                "hex",
                new Regex(@"^(0x[0-9a-f]+)", RegexOptions.IgnoreCase),
                m =>
                {
                    int value;
                    if (int.TryParse(m, out value))
                    {
                        return new LiteralSymbol(value);
                    }
                    throw new Exception(string.Format("{0} is out of supported integer range.", m));
                }
            ));
            _tokenDefinitions.Add(new TokenDefinition(
                "float",
                new Regex(@"^(\d*\.\d+(?:e[+-]*\d+)*)", RegexOptions.IgnoreCase),
                m =>
                {
                    double value;
                    if (double.TryParse(m, out value))
                    {
                        return new LiteralSymbol(value);
                    }
                    throw new Exception(string.Format("{0} is not a supported floating-point number.", m));
                }
            ));
            _tokenDefinitions.Add(new TokenDefinition(
                "integer",
                new Regex(@"^(\d+)"),
                m =>
                {
                    int value;
                    if (int.TryParse(m, out value))
                    {
                        return new LiteralSymbol(value);
                    }
                    throw new Exception(string.Format("{0} is not a supported integer.", m));
                }
            ));
            _tokenDefinitions.Add(new TokenDefinition(
                "name",
                new Regex(@"^(\w+)"),
                m =>
                {
                    string lm = m.ToLower();
                    switch (lm)
                    {
                        case "e":
                            return new LiteralSymbol(Math.E);
                        case "pi":
                            return new LiteralSymbol(Math.PI);
                        default:
                            return new NameSymbol(m);
                    }
                }
                ));
        }

        #endregion

        #region Internal Class

        class TokenDefinition
        {
            private readonly string _name;
            private readonly Regex _pattern;
            private readonly Func<string, Symbol> _generator;

            public string Name { get { return _name; } }
            public Regex Pattern { get { return _pattern; } }
            public Func<string, Symbol> Generator { get { return _generator; } }

            public TokenDefinition(string tName, Regex tPattern, Func<string, Symbol> tGen)
            {
                _name = tName;
                _pattern = tPattern;
                _generator = tGen;
            }
        }

        #endregion

    }

}
