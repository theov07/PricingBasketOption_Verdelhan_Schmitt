using System;

namespace BasketOptionPricer
{
    // Class to represent a financial asset
    public class Stock
    {
        public string Name { get; set; } // asset name
        public double SpotPrice { get; set; } // current price S0
        public double Volatility { get; set; } // volatility sigma
        public double DividendRate { get; set; } // dividend rate
        
        // Simple constructor
        public Stock(string name, double spotPrice, double volatility, double dividendRate)
        {
            Name = name;
            SpotPrice = spotPrice;
            Volatility = volatility;
            DividendRate = dividendRate;
        }
    }
}