using System;
using System.Collections.Generic;
using System.Linq;

namespace BasketOptionPricer
{

    public class DeterministicRateModel
    {
        private readonly List<(double time, double rate)> _ratePoints;
        
        public DeterministicRateModel()
        {
            _ratePoints = new List<(double, double)>();
        }
        
        public DeterministicRateModel(double constantRate)
        {
            _ratePoints = new List<(double, double)>
            {
                (0.0, constantRate),
                (100.0, constantRate) // Grande maturité pour interpolation
            };
        }
        
        public void AddRatePoint(double time, double rate)
        {
            _ratePoints.Add((time, rate));
            _ratePoints.Sort((a, b) => a.time.CompareTo(b.time));
        }
        
        public double GetRate(double time)
        {
            if (_ratePoints.Count == 0)
                throw new InvalidOperationException("No rate points defined");
            
            if (time <= _ratePoints.First().time)
                return _ratePoints.First().rate;
            
            if (time >= _ratePoints.Last().time)
                return _ratePoints.Last().rate;
            
            // Interpolation linéaire
            for (int i = 0; i < _ratePoints.Count - 1; i++)
            {
                var (t1, r1) = _ratePoints[i];
                var (t2, r2) = _ratePoints[i + 1];
                
                if (time >= t1 && time <= t2)
                {
                    double weight = (time - t1) / (t2 - t1);
                    return r1 + weight * (r2 - r1);
                }
            }
            
            return _ratePoints.Last().rate;
        }
        
        public double IntegrateRate(double T, int numSteps = 1000)
        {
            if (T <= 0) return 0.0;
            
            double dt = T / numSteps;
            double integral = 0.0;
            
            for (int i = 0; i <= numSteps; i++)
            {
                double t = i * dt;
                double rate = GetRate(t);
                
                if (i == 0 || i == numSteps)
                    integral += 0.5 * rate * dt;
                else
                    integral += rate * dt;
            }
            
            return integral;
        }
        
        public double GetDiscountFactor(double T)
        {
            return Math.Exp(-IntegrateRate(T));
        }
    }
    
    public class DeterministicVolatilityModel
    {
        private readonly List<(double time, double volatility)> _volPoints;
        
        public DeterministicVolatilityModel()
        {
            _volPoints = new List<(double, double)>();
        }
        
        public DeterministicVolatilityModel(double constantVolatility)
        {
            _volPoints = new List<(double, double)>
            {
                (0.0, constantVolatility),
                (100.0, constantVolatility)
            };
        }
        
        public void AddVolatilityPoint(double time, double volatility)
        {
            _volPoints.Add((time, volatility));
            _volPoints.Sort((a, b) => a.time.CompareTo(b.time));
        }
        
        public double GetVolatility(double time)
        {
            if (_volPoints.Count == 0)
                throw new InvalidOperationException("No volatility points defined");
            
            if (time <= _volPoints.First().time)
                return _volPoints.First().volatility;
            
            if (time >= _volPoints.Last().time)
                return _volPoints.Last().volatility;
            
            // Interpolation linéaire
            for (int i = 0; i < _volPoints.Count - 1; i++)
            {
                var (t1, vol1) = _volPoints[i];
                var (t2, vol2) = _volPoints[i + 1];
                
                if (time >= t1 && time <= t2)
                {
                    double weight = (time - t1) / (t2 - t1);
                    return vol1 + weight * (vol2 - vol1);
                }
            }
            
            return _volPoints.Last().volatility;
        }
        
        public double IntegrateVariance(double T, int numSteps = 1000)
        {
            if (T <= 0) return 0.0;
            
            double dt = T / numSteps;
            double integral = 0.0;
            
            for (int i = 0; i <= numSteps; i++)
            {
                double t = i * dt;
                double vol = GetVolatility(t);
                double variance = vol * vol;
                
                if (i == 0 || i == numSteps)
                    integral += 0.5 * variance * dt;
                else
                    integral += variance * dt;
            }
            
            return integral;
        }
    }
}