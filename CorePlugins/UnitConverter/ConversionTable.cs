using System;
using System.Collections.Generic;


namespace CorePlugins.UnitConverter
{
    /// <summary>
    /// Describes a conversion table for a specific dimension.
    /// </summary>
    public class ConversionTable
    {
        private const double Tolerance = 1e-8;

        private readonly string _dimension;
        private readonly Dictionary<string, int> _unitMap; 
        private double[,] _definitionTable;
        private double[,] _computedTable;
        private bool _isComplete = false;
        private bool _isConsistent = false;
        private bool _needUpdate = true;

        public ConversionTable(string dimension, IEnumerable<string> units)
        {
            _dimension = dimension;
            _unitMap = new Dictionary<string, int>();
            var idx = 0;
            foreach (var unit in units)
            {
                _unitMap.Add(unit, idx);
                idx++;
            }
            InitializeDefinitionTable(_unitMap.Count);
            _isComplete = false;
            _isComplete = false;
        }

        /// <summary>
        /// Gets if all pairs of units are convertible in this table.
        /// </summary>
        public bool IsComplete
        {
            get
            {
                if (_needUpdate)
                {
                    UpdateTable();
                }
                return _isComplete;
            }
        }

        /// <summary>
        /// Gets if this conversion table is consistent.
        /// An inconsistency occurs when two different conversion path lead to different results
        /// (e.g., 1a = 10b, 1b = 10c, however, 1a = 20c instead of 1a = 100c)
        /// </summary>
        public bool IsConsistent
        {
            get
            {
                if (_needUpdate)
                {
                    UpdateTable();
                }
                return _isConsistent;
            }
        }

        /// <summary>
        /// Gets the size of the table (number of columns/rows).
        /// </summary>
        public int Size { get { return _unitMap.Count; } }

        /// <summary>
        /// Gets the unit dimension (e.g., length, area, speed, etc.) for this table.
        /// </summary>
        public string Dimension { get { return _dimension; } }

        public bool TryGetFactor(string fromUnit, string toUnit, out double result)
        {
            if (fromUnit == null)
                throw new ArgumentNullException("fromUnit");
            if (toUnit == null)
                throw new ArgumentNullException("toUnit");
            int fromId, toId;
            if (!_unitMap.TryGetValue(fromUnit, out fromId))
            {
                result = default(double);
                return false;
            }
            if (!_unitMap.TryGetValue(toUnit, out toId))
            {
                result = default(double);
                return false;
            }
            if (fromId == toId)
            {
                result = 1.0;
                return true;
            }
            // needs actual conversion
            if (_needUpdate)
            {
                UpdateTable();
            }
            if (!_isConsistent)
            {
                result = default(double);
                return false;
            }
            double factor = _computedTable[fromId, toId];
            if (double.IsNaN(factor))
            {
                result = default(double);
                return false;
            }
            result = factor;
            return true;
        }

        public bool TryConvert(string fromUnit, string toUnit, double quantity, out double result)
        {
            double factor;
            if (TryGetFactor(fromUnit, toUnit, out factor))
            {
                result = factor*quantity;
                return true;
            }
            result = default(double);
            return false;
        }

        public void SetFactor(string fromUnit, string toUnit, double factor)
        {
            if (fromUnit == null)
                throw new ArgumentNullException("fromUnit");
            if (toUnit == null)
                throw new ArgumentNullException("toUnit");
            if (fromUnit == toUnit)
                throw new ArgumentException("From and to cannot be the same unit.");
            if (double.IsNaN(factor))
                throw new ArgumentException("Factor must be a number.");
            if (Math.Abs(factor) <= double.Epsilon)
                throw new ArgumentException("Factor cannot be 0.");

            int fromId = _unitMap[fromUnit];
            int toId = _unitMap[toUnit];
            _definitionTable[fromId, toId] = factor;
            _definitionTable[toId, fromId] = 1.0/factor;
            _needUpdate = true;
        }

        public void UpdateTable()
        {
            if (!_needUpdate)
            {
                return;
            }
            int n = Size;
            _computedTable = new double[n, n];
            Array.Copy(_definitionTable, 0, _computedTable, 0, _definitionTable.Length);
            _isComplete = true;
            for (var i = 0; i < n; i++)
            {
                for (var j = i + 1; j < n; j++)
                {
                    if (double.IsNaN(_computedTable[i, j]))
                    {
                        // infer possible conversions from i to j
                        InferConversion(i, j);
                        if (double.IsNaN(_computedTable[i, j]))
                        {
                            // found isolated nodes
                            _isComplete = false;
                        }
                    }
                }
            }
            _isConsistent = CheckConsistency();
            _needUpdate = false;
        }

        private void InferConversion(int fromId, int toId)
        {
            int n = Size;
            var visitFlags = new bool[n];
            Array.Clear(visitFlags, 0, n);
            visitFlags[fromId] = true;
            var visitQueue = new Queue<IdFactorPair>();
            for (var i = 0; i < n; i++)
            {
                if (i != fromId && !double.IsNaN(_computedTable[fromId, i]))
                {
                    visitQueue.Enqueue(new IdFactorPair {Factor = _computedTable[fromId, i], Id = i});
                }
            }
            // search for connection
            while (visitQueue.Count > 0)
            {
                IdFactorPair pair = visitQueue.Dequeue();
                if (pair.Id == toId)
                {
                    // found
                    _computedTable[fromId, toId] = pair.Factor;
                    _computedTable[toId, fromId] = 1.0/pair.Factor;
                    break;
                }
                if (double.IsNaN(_computedTable[fromId, pair.Id]))
                {
                    // conveniently update computed factors along the path
                    _computedTable[fromId, pair.Id] = pair.Factor;
                    _computedTable[pair.Id, fromId] = 1.0/pair.Factor;
                }
                visitFlags[pair.Id] = true;
                for (var i = 0; i < n; i++)
                {
                    if (!visitFlags[i] && !double.IsNaN(_computedTable[pair.Id, i]))
                    {
                        visitQueue.Enqueue(new IdFactorPair {Factor = _computedTable[pair.Id, i]*pair.Factor, Id = i});
                    }
                }
            }
        }

        private bool CheckConsistency()
        {
            // for a consistent conversion table F, F(i,j) = F(i,k)F(k,j) for all k
            int n = Size;
            for (var i = 0; i < n; i++)
            {
                for (var j = i + 1; j < n; j++)
                {
                    if (double.IsNaN(_computedTable[i, j]))
                    {
                        if (!double.IsNaN(_computedTable[j, i]))
                        {
                            return false;
                        }
                        for (int k = 0; k < n; k++)
                        {
                            if (!(double.IsNaN(_computedTable[i, k]) || double.IsNaN(_computedTable[k, j])))
                            {
                                return false;
                            }
                            if (!(double.IsNaN(_computedTable[i, k]) || double.IsNaN(_computedTable[k, j])))
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        if (double.IsNaN(_computedTable[j, i]))
                        {
                            return false;
                        }
                        for (int k = 0; k < n; k++)
                        {
                            if (!double.IsNaN(_computedTable[i, k]) && !double.IsNaN(_computedTable[k, j]) &&
                                Math.Abs(_computedTable[i, k] * _computedTable[k, j] / _computedTable[i, j] - 1.0) > Tolerance)
                            {
                                return false;
                            }
                            if (!double.IsNaN(_computedTable[j, k]) && !double.IsNaN(_computedTable[k, i]) &&
                                Math.Abs(_computedTable[j, k] * _computedTable[k, i] / _computedTable[j, i] - 1.0) > Tolerance)
                            {
                                return false;
                            }
                        }
                    }
                    
                } 
            }
            return true;
        }

        private void InitializeDefinitionTable(int size)
        {
            _definitionTable = new double[size, size];
            for (var i = 0; i < size; i++)
            {
                for (var j = 0; j < size; j++)
                {
                    _definitionTable[i, j] = i == j ? 1 : double.NaN;
                }
            }
        }

        struct IdFactorPair
        {
            public int Id;
            public double Factor;
        }

    }
}
