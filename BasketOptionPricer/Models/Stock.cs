using System;

namespace BasketOptionPricer
{
    // Classe pour représenter un actif financier
    public class Stock
    {
        public string Name { get; set; } // nom de l'actif
        public double SpotPrice { get; set; } // prix actuel S0
        public double Volatility { get; set; } // volatilité sigma
        public double DividendRate { get; set; } // taux de dividende
        
        // Constructeur simple
        public Stock(string name, double spotPrice, double volatility, double dividendRate)
        {
            Name = name;
            SpotPrice = spotPrice;
            Volatility = volatility;
            DividendRate = dividendRate;
        }
    }
}