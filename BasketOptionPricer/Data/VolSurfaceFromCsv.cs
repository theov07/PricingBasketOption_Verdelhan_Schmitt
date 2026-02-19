using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace BasketOptionPricer.Data
{
    public readonly struct VolSurfacePoint
    {
        public double Moneyness { get; }
        public double ImpliedVol { get; }

        public VolSurfacePoint(double moneyness, double impliedVol)
        {
            Moneyness = moneyness;
            ImpliedVol = impliedVol;
        }
    }

    public class VolSurfaceSlice
    {
        public double Maturity { get; }
        private readonly List<VolSurfacePoint> _points;

        public VolSurfaceSlice(double maturity)
        {
            Maturity = maturity;
            _points = new List<VolSurfacePoint>();
        }

        public void AddPoint(double moneyness, double impliedVol)
        {
            int idx = _points.FindIndex(p => Math.Abs(p.Moneyness - moneyness) < 1e-9);
            if (idx >= 0)
                _points[idx] = new VolSurfacePoint(moneyness, impliedVol);
            else
                _points.Add(new VolSurfacePoint(moneyness, impliedVol));
        }

        public void SortByMoneyness()
        {
            _points.Sort((a, b) => a.Moneyness.CompareTo(b.Moneyness));
        }

        public IReadOnlyList<VolSurfacePoint> Points => _points;

        public double InterpolateVol(double moneyness)
        {
            if (_points.Count == 0)
                throw new InvalidOperationException("No points in slice");

            if (_points.Count == 1)
                return _points[0].ImpliedVol;

            if (moneyness <= _points[0].Moneyness)
                return _points[0].ImpliedVol;

            if (moneyness >= _points[^1].Moneyness)
                return _points[^1].ImpliedVol;

            for (int i = 0; i < _points.Count - 1; i++)
            {
                if (moneyness >= _points[i].Moneyness && moneyness <= _points[i + 1].Moneyness)
                {
                    double m1 = _points[i].Moneyness;
                    double m2 = _points[i + 1].Moneyness;
                    double v1 = _points[i].ImpliedVol;
                    double v2 = _points[i + 1].ImpliedVol;
                    double w = (moneyness - m1) / (m2 - m1);
                    return v1 + w * (v2 - v1);
                }
            }
            return _points[^1].ImpliedVol;
        }
    }

    public class VolSurfaceFromCsv
    {
        private readonly SortedDictionary<double, VolSurfaceSlice> _slices;
        private readonly DateTime _valuationDate;
        private readonly string _underlying;

        public DateTime ValuationDate => _valuationDate;
        public string Underlying => _underlying;
        public IReadOnlyCollection<double> Maturities => _slices.Keys;

        public VolSurfaceFromCsv(string underlying = "SX5E")
        {
            _underlying = underlying;
            _slices = new SortedDictionary<double, VolSurfaceSlice>();
            _valuationDate = DateTime.MinValue;
        }

        public static VolSurfaceFromCsv LoadFromCsv(string filePath, string underlying = "SX5E")
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            var surface = new VolSurfaceFromCsv(underlying);
            var lines = File.ReadAllLines(filePath);
            DateTime? valDate = null;

            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(',');
                if (parts.Length < 5) continue;

                try
                {
                    var vd = DateTime.ParseExact(parts[0].Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    var exp = DateTime.ParseExact(parts[1].Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    var m = double.Parse(parts[3].Trim(), CultureInfo.InvariantCulture);
                    var vol = double.Parse(parts[4].Trim(), CultureInfo.InvariantCulture);

                    if (!valDate.HasValue) valDate = vd;
                    
                    double T = (exp - vd).TotalDays / 365.0;

                    if (!surface._slices.TryGetValue(T, out var slice))
                    {
                        slice = new VolSurfaceSlice(T);
                        surface._slices[T] = slice;
                    }
                    slice.AddPoint(m, vol);
                }
                catch (FormatException) { }
            }

            foreach (var slice in surface._slices.Values)
                slice.SortByMoneyness();

            if (valDate.HasValue)
            {
                var field = typeof(VolSurfaceFromCsv).GetField("_valuationDate", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(surface, valDate.Value);
            }

            return surface;
        }

        public double GetVolByMoneyness(double T, double moneyness)
        {
            if (_slices.Count == 0)
                throw new InvalidOperationException("Empty surface");

            var mats = _slices.Keys.ToList();

            if (mats.Count == 1)
                return _slices[mats[0]].InterpolateVol(moneyness);

            if (T <= mats[0])
                return _slices[mats[0]].InterpolateVol(moneyness);

            if (T >= mats[^1])
                return _slices[mats[^1]].InterpolateVol(moneyness);

            double T1 = mats[0], T2 = mats[1];
            for (int i = 0; i < mats.Count - 1; i++)
            {
                if (T >= mats[i] && T <= mats[i + 1])
                {
                    T1 = mats[i];
                    T2 = mats[i + 1];
                    break;
                }
            }

            double vol1 = _slices[T1].InterpolateVol(moneyness);
            double vol2 = _slices[T2].InterpolateVol(moneyness);
            double w = (T - T1) / (T2 - T1);
            return vol1 + w * (vol2 - vol1);
        }

        public double GetVolByStrike(double T, double strike, double forward)
        {
            if (forward <= 0)
                throw new ArgumentException("Forward <= 0");
            return GetVolByMoneyness(T, strike / forward);
        }

        public void PrintSummary()
        {
            Console.WriteLine($"Surface: {_underlying} | Date: {_valuationDate:yyyy-MM-dd} | {_slices.Count} maturités");
            foreach (var (T, slice) in _slices)
            {
                var pts = slice.Points;
                Console.WriteLine($"  T={T:F3}y | M=[{pts.Min(p => p.Moneyness):F2},{pts.Max(p => p.Moneyness):F2}] | ATM={slice.InterpolateVol(1.0)*100:F1}%");
            }
        }
    }
}
