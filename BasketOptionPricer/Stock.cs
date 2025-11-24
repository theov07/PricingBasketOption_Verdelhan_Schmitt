using System;

namespace BasketOptionPricer
{
    public class Stock
    {
        public string Name { get; set; }
        public double SpotPrice { get; set; }
        public double Volatility { get; set; }
        public double DividendRate { get; set; }
        
        public Stock(string name, double spotPrice, double volatility, double dividendRate)
        {
            Name = name;
            SpotPrice = spotPrice;
            Volatility = volatility;
            DividendRate = dividendRate;
        }
    }
}