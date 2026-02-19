using System;
using System.Collections.Generic;
using System.Linq;

namespace BasketOptionPricer
{

    public class StockH2
    {
        public string Name { get; }
        public double SpotPrice { get; }
        public DeterministicVolatilityModel VolatilityModel { get; }
        public double DividendRate { get; } // Remains constant in H2
        
        public StockH2(string name, double spotPrice, DeterministicVolatilityModel volatilityModel, double dividendRate)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            SpotPrice = spotPrice > 0 ? spotPrice : throw new ArgumentException("Spot price must be positive", nameof(spotPrice));
            VolatilityModel = volatilityModel ?? throw new ArgumentNullException(nameof(volatilityModel));
            DividendRate = dividendRate >= 0 ? dividendRate : throw new ArgumentException("Dividend rate must be positive", nameof(dividendRate));
        }
        
        public StockH2(string name, double spotPrice, double constantVolatility, double dividendRate)
            : this(name, spotPrice, new DeterministicVolatilityModel(constantVolatility), dividendRate)
        {
        }
    }
    
    public class BasketH2
    {
        public List<StockH2> Stocks { get; }
        public double[] Weights { get; }
        public double[,] CorrelationMatrix { get; }
        public DeterministicRateModel RateModel { get; }
        
        public BasketH2(List<StockH2> stocks, double[] weights, double[,] correlationMatrix, DeterministicRateModel rateModel)
        {
            Stocks = stocks ?? throw new ArgumentNullException(nameof(stocks));
            Weights = weights ?? throw new ArgumentNullException(nameof(weights));
            CorrelationMatrix = correlationMatrix ?? throw new ArgumentNullException(nameof(correlationMatrix));
            RateModel = rateModel ?? throw new ArgumentNullException(nameof(rateModel));
            
            ValidateInputs();
        }
        
        public BasketH2(List<StockH2> stocks, double[] weights, double[,] correlationMatrix, double constantRate)
            : this(stocks, weights, correlationMatrix, new DeterministicRateModel(constantRate))
        {
        }
        
        private void ValidateInputs()
        {
            if (Stocks.Count == 0)
                throw new ArgumentException("Basket must contain at least one asset");
            
            if (Weights.Length != Stocks.Count)
                throw new ArgumentException("Number of weights must equal number of assets");
            
            if (Math.Abs(Weights.Sum() - 1.0) > 1e-6)
                throw new ArgumentException("Sum of weights must equal 1");
            
            int n = Stocks.Count;
            if (CorrelationMatrix.GetLength(0) != n || CorrelationMatrix.GetLength(1) != n)
                throw new ArgumentException("Correlation matrix must be n×n");
            
            // Correlation matrix validation
            for (int i = 0; i < n; i++)
            {
                if (Math.Abs(CorrelationMatrix[i, i] - 1.0) > 1e-6)
                    throw new ArgumentException($"Diagonal correlation [{i},{i}] must be 1");
                
                for (int j = i + 1; j < n; j++)
                {
                    if (Math.Abs(CorrelationMatrix[i, j] - CorrelationMatrix[j, i]) > 1e-6)
                        throw new ArgumentException("Correlation matrix must be symmetric");
                    
                    if (Math.Abs(CorrelationMatrix[i, j]) > 1.0)
                        throw new ArgumentException("Correlations must be between -1 and 1");
                }
            }
        }

        public double GetBasketValue()
        {
            double value = 0;
            for (int i = 0; i < Stocks.Count; i++)
            {
                value += Weights[i] * Stocks[i].SpotPrice;
            }
            return value;
        }
    }
    
    public class BasketOptionH2
    {
        public BasketH2 Basket { get; }
        public OptionType Type { get; }
        public double Strike { get; }
        public double Maturity { get; }
        
        public BasketOptionH2(BasketH2 basket, OptionType type, double strike, double maturity)
        {
            Basket = basket ?? throw new ArgumentNullException(nameof(basket));
            Type = type;
            Strike = strike > 0 ? strike : throw new ArgumentException("Strike must be positive", nameof(strike));
            Maturity = maturity > 0 ? maturity : throw new ArgumentException("Maturity must be positive", nameof(maturity));
        }
        
        public double CalculatePayoff(double basketValue)
        {
            return Type switch
            {
                OptionType.Call => Math.Max(basketValue - Strike, 0),
                OptionType.Put => Math.Max(Strike - basketValue, 0),
                _ => throw new ArgumentException("Unsupported option type")
            };
        }
    }
}