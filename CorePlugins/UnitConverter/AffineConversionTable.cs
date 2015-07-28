using System;
using System.Collections.Generic;

namespace CorePlugins.UnitConverter
{
    /// <summary>
    /// Describes a conversion table for a specific dimension.
    /// Conversion is done via affine transformation.
    /// </summary>
    public class AffineConversionTable : IConversionProvider
    {
        private const double Tolerance = 1e-8;
        private const int MaxTableSize = 1024;
        private static readonly Func<double, double> IdentityConverter = x => x;

        private readonly string _dimension;
        private readonly Dictionary<Unit, int> _unitMap; 
        // stores slope values. NaN means undefined.
        private double[,] _slopeTable;
        // stores y-intercept values. No NaNs in this table since it is sufficient to mark undefined conversions
        // with NaNs in slope table.
        private double[,] _yInterceptTable;
        // we pack fromId and toId into one int32 since maximum number of units allowed is much less than 2^12
        private Dictionary<int, AffineCoefficientDefinition> _userDefinitions =
            new Dictionary<int, AffineCoefficientDefinition>();
        private bool _isComplete = false;
        private bool _isConsistent = false;
        private bool _needUpdate = true;

        public AffineConversionTable(string dimension, IEnumerable<Unit> units)
        {
            _dimension = dimension;
            _unitMap = new Dictionary<Unit, int>();
            var idx = 0;
            foreach (var unit in units)
            {
                _unitMap.Add(unit, idx);
                idx++;
                if (idx > MaxTableSize)
                {
                    throw new ArgumentException(
                        string.Format("Too many units. Maximum number of units allowed is {0}.", MaxTableSize));
                }
            }
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

        public bool TryProvideConverter(Unit fromUnit, Unit toUnit, out Func<double, double> converter)
        {
            if (fromUnit == null || toUnit == null
                || !fromUnit.Dimension.Equals(Dimension) || !toUnit.Dimension.Equals(Dimension))
            {
                converter = null;
                return false;
            }
            int fromId, toId;
            if (!_unitMap.TryGetValue(fromUnit, out fromId))
            {
                converter = null;
                return false;
            }
            if (!_unitMap.TryGetValue(toUnit, out toId))
            {
                converter = null;
                return false;
            }
            if (fromId == toId)
            {
                converter = IdentityConverter;
                return true;
            }
            // needs actual conversion
            if (_needUpdate)
            {
                UpdateTable();
            }
            if (!_isConsistent)
            {
                converter = null;
                return false;
            }
            double slope = _slopeTable[fromId, toId];
            if (double.IsNaN(slope))
            {
                converter = null;
                return false;
            }
            double yIntercept = _yInterceptTable[fromId, toId];
            converter = x => slope*x + yIntercept;
            return true;
        }

        public void SetConversionCoefficients(Unit fromUnit, Unit toUnit, double slope, double yIntercept)
        {
            if (fromUnit == null)
                throw new ArgumentNullException("fromUnit");
            if (toUnit == null)
                throw new ArgumentNullException("toUnit");
            if (fromUnit.Equals(toUnit))
                throw new ArgumentException("From and to cannot be the same unit.");
            if (double.IsNaN(slope))
                throw new ArgumentException("Slope must be a number.");
            if (Math.Abs(slope) <= double.Epsilon)
                throw new ArgumentException("Slope cannot be 0.");
            if (double.IsNaN(yIntercept))
                throw new ArgumentException("Y-intercept must be a number.");

            int fromId, toId;
            if (!_unitMap.TryGetValue(fromUnit, out fromId))
                throw new ArgumentException(string.Format("Unit {0} is not defined in the table.", fromUnit));
            if (!_unitMap.TryGetValue(toUnit, out toId))
                throw new ArgumentException(string.Format("Unit {0} is not defined in the table.", toUnit));

            int packedId = fromId | (toId << 12);
            AffineCoefficientDefinition coeff;
            if (_userDefinitions.TryGetValue(packedId, out coeff))
            {
                coeff.Slope = slope;
                coeff.YIntercept = yIntercept;
            }
            else
            {
                _userDefinitions[packedId] = new AffineCoefficientDefinition(slope, yIntercept);
            }
            _needUpdate = true;
        }

        public void UpdateTable()
        {
            if (!_needUpdate)
            {
                return;
            }
            int n = Size;
            InitializeTableFromUserDefinitions();
            _isComplete = true;
            for (var i = 0; i < n; i++)
            {
                for (var j = i + 1; j < n; j++)
                {
                    if (double.IsNaN(_slopeTable[i, j]))
                    {
                        // infer possible conversions from i to j
                        InferConversion(i, j);
                        if (double.IsNaN(_slopeTable[i, j]))
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
            var visitQueue = new Queue<IdCoefficientTuple>();
            for (var i = 0; i < n; i++)
            {
                if (i != fromId && !double.IsNaN(_slopeTable[fromId, i]))
                {
                    visitQueue.Enqueue(new IdCoefficientTuple(i, _slopeTable[fromId, i], _yInterceptTable[fromId, i]));
                }
            }
            // search for connection
            while (visitQueue.Count > 0)
            {
                IdCoefficientTuple tuple = visitQueue.Dequeue();
                if (tuple.Id == toId)
                {
                    // found
                    SetComputedCoeffientsInPair(fromId, toId, tuple.Slope, tuple.YIntercept);
                    break;
                }
                if (double.IsNaN(_slopeTable[fromId, tuple.Id]))
                {
                    // conveniently update computed factors along the path
                    SetComputedCoeffientsInPair(fromId, tuple.Id, tuple.Slope, tuple.YIntercept);
                }
                visitFlags[tuple.Id] = true;
                for (var i = 0; i < n; i++)
                {
                    if (!visitFlags[i] && !double.IsNaN(_slopeTable[tuple.Id, i]))
                    {
                        // origin  -> current : (ax + b)
                        // current -> next    : (cx + d)
                        // origin  -> next    : (acx + bc + d)
                        double newSlope = _slopeTable[tuple.Id, i]*tuple.Slope;
                        double newYIntercept = _slopeTable[tuple.Id, i]*tuple.YIntercept + _yInterceptTable[tuple.Id, i];
                        visitQueue.Enqueue(new IdCoefficientTuple(i, newSlope, newYIntercept));
                    }
                }
            }
        }

        private bool CheckConsistency()
        {
            // for a consistent conversion table F, F_{ij}(x) = F_{kj}(F_{ik}(x)) for all k
            int n = Size;
            for (var i = 0; i < n; i++)
            {
                for (var j = i + 1; j < n; j++)
                {
                    if (double.IsNaN(_slopeTable[i, j]))
                    {
                        if (!double.IsNaN(_slopeTable[j, i]))
                        {
                            return false;
                        }
                        // if x = ab = NaN, at least one of a,b is NaN
                        for (int k = 0; k < n; k++)
                        {
                            if (!(double.IsNaN(_slopeTable[i, k]) || double.IsNaN(_slopeTable[k, j])))
                            {
                                return false;
                            }
                            if (!(double.IsNaN(_slopeTable[i, k]) || double.IsNaN(_slopeTable[k, j])))
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        if (double.IsNaN(_slopeTable[j, i]))
                        {
                            return false;
                        }
                        for (int k = 0; k < n; k++)
                        {
                            if (!double.IsNaN(_slopeTable[i, k]) && !double.IsNaN(_slopeTable[k, j]) &&
                                CheckConvertiblePairConsistency(i, k, j))
                            {
                                return false;
                            }
                            if (!double.IsNaN(_slopeTable[j, k]) && !double.IsNaN(_slopeTable[k, i]) &&
                                CheckConvertiblePairConsistency(j, k, i))
                            {
                                return false;
                            }
                        }
                    }
                    
                } 
            }
            return true;
        }

        private bool CheckConvertiblePairConsistency(int fromId, int midId, int toId)
        {
            // from -> mid : (ax + b)
            // mid  -> to  : (cx + d)
            // from -> to  : (ex + f)
            // should have : e = ca, f = cb + d
            double a = _slopeTable[fromId, midId];
            double b = _yInterceptTable[fromId, midId];
            double c = _slopeTable[midId, toId];
            double d = _yInterceptTable[midId, toId];
            double e = _slopeTable[fromId, toId];
            double f = _yInterceptTable[fromId, toId];
            return (Math.Abs(c*a/e - 1.0) > Tolerance || Math.Abs((c*b + d)/f - 1.0) > Tolerance);
        }

        private void SetComputedCoeffientsInPair(int fromId, int toId, double slope, double yIntercept)
        {
            _slopeTable[fromId, toId] = slope;
            _yInterceptTable[fromId, toId] = yIntercept;
            _slopeTable[toId, fromId] = 1.0/slope;
            _yInterceptTable[toId, fromId] = -yIntercept/slope;
        }

        private void InitializeTableFromUserDefinitions()
        {
            int n = Size;
            _slopeTable = new double[n, n];
            _yInterceptTable = new double[n, n];
            for (var i = 0; i < n; i++)
            {
                for (var j = 0; j < n; j++)
                {
                    _slopeTable[i, j] = i == j ? 1.0 : double.NaN;
                    _yInterceptTable[i, j] = 0.0;
                }
            }
            foreach (var pair in _userDefinitions)
            {
                SetComputedCoeffientsInPair(
                    pair.Key & 0x00000fff,
                    (pair.Key & 0x00fff000) >> 12,
                    pair.Value.Slope,
                    pair.Value.YIntercept);
            }
        }

        sealed class IdCoefficientTuple
        {
            public int Id { get; private set; }
            public double Slope { get; private set; }
            public double YIntercept { get; private set; }

            public IdCoefficientTuple(int id, double slope, double yIntercept)
            {
                Id = id;
                Slope = slope;
                YIntercept = yIntercept;
            }
        }

        sealed class AffineCoefficientDefinition
        {
            public double Slope { get; set; }
            public double YIntercept { get; set; }

            public AffineCoefficientDefinition(double slope, double yIntercept)
            {
                Slope = slope;
                YIntercept = yIntercept;
            }
        }

    }
}
