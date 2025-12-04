using System;
using System.Collections.Generic;
using System.Linq;

namespace BasketOptionPricer
{
    public class Basket
    {
        public List<Stock> Stocks { get; set; }
        public double[] Weights { get; set; }
        public double[,] CorrelationMatrix { get; set; }
        public double RiskFreeRate { get; set; }
        
        public Basket(List<Stock> stocks, double[] weights, double[,] correlationMatrix, double riskFreeRate)
        {
            if (stocks.Count != weights.Length)
                throw new ArgumentException("Number of stocks must equal number of weights");
                
            if (correlationMatrix.GetLength(0) != correlationMatrix.GetLength(1) || correlationMatrix.GetLength(0) != stocks.Count)
                throw new ArgumentException("Correlation matrix dimensions must match number of stocks");
                
            if (Math.Abs(weights.Sum() - 1.0) > 1e-6)
                throw new ArgumentException("Weights must sum to 1");
                
            Stocks = stocks;
            Weights = weights;
            CorrelationMatrix = correlationMatrix;
            RiskFreeRate = riskFreeRate;
        }
        
        public double GetBasketValue()
        {
            double basketValue = 0;
            for (int i = 0; i < Stocks.Count; i++)
            {
                basketValue += Weights[i] * Stocks[i].SpotPrice;
            }
            return basketValue;
        }
    }
}