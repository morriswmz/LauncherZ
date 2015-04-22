using System;
using System.Collections.Generic;
using CorePlugins.MathEvaluator.Symbols;

namespace CorePlugins.MathEvaluator
{
    sealed class ExpressionParser
    {
        private static ExpressionParser _instance;

        private Queue<Symbol> _symbols;
        private Symbol _current;

        private Dictionary<string, int> _lbpTable;
        private Dictionary<string, int> _ledpTable;
        private Dictionary<string, int> _nudpTable;

        public static ExpressionParser Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ExpressionParser();
                }
                return _instance;
            }
        }

        public Dictionary<string, int> LBPTable
        {
            get { return _lbpTable; }
        }

        public Dictionary<string, int> LEDPTable
        {
            get { return _ledpTable; }
        }

        public Dictionary<string, int> NUDPTable
        {
            get { return _nudpTable; }
        }

        public string CurrentSymbolId
        {
            get { return _current.Id; }
        }

        private ExpressionParser()
        {
            InitSymbolPowerTable();
        }

        public Symbol ParseExpression(int rbp)
        {
            if (_symbols.Count > 0)
            {
                Symbol previous = _current;
                _current = _symbols.Dequeue();
                Symbol left = previous.NUD(this);
                while (rbp < _lbpTable[_current.Id])
                {
                    previous = _current;
                    _current = _symbols.Dequeue();
                    left = previous.LED(this, left);
                }
                return left;
            }
            else
            {
                throw new Exception("Unexpected end of expression.");
            }
        }

        /// <summary>
        /// Clears the symbol queue.
        /// </summary>
        public void Clear()
        {
            if (_symbols != null)
            {
                _symbols.Clear();
                _symbols = null;
            }
        }

        public void Expect(string id)
        {
            if (id == null) throw new ArgumentNullException("id");
            if (_current.Id != id)
            {
                throw new Exception(string.Format("Expected {0}", id));
            }
            _current = _symbols.Dequeue();
        }

        public void LoadSymbols(Queue<Symbol> sym)
        {
            Clear();
            _symbols = sym;
            _current = _symbols.Dequeue();
        }

        private void InitSymbolPowerTable()
        {
            _lbpTable = new Dictionary<string, int>();
            _ledpTable = new Dictionary<string, int>();
            _nudpTable = new Dictionary<string, int>();

            _lbpTable.Add(Symbol.LiteralId, 0);
            _lbpTable.Add(Symbol.NameId, 0);
            _lbpTable.Add(Symbol.EndId, 0);
            _lbpTable.Add(",", 0);

            _lbpTable.Add("<<", 100);
            _lbpTable.Add(">>", 100);
            _lbpTable.Add("+", 110);
            _lbpTable.Add("-", 110);
            _lbpTable.Add("*", 120);
            _lbpTable.Add("/", 120);
            _lbpTable.Add("%", 120);
            _lbpTable.Add("^", 130);
            _lbpTable.Add("(", 150);
            _lbpTable.Add(")", 0);

            _ledpTable.Add("<<", 100);
            _ledpTable.Add(">>", 100);
            _ledpTable.Add("+", 110);
            _ledpTable.Add("-", 110);
            _ledpTable.Add("*", 120);
            _ledpTable.Add("/", 120);
            _ledpTable.Add("%", 120);
            _ledpTable.Add("^", 140);

            _nudpTable.Add("+", 130);
            _nudpTable.Add("-", 130);
        }

    }
}
