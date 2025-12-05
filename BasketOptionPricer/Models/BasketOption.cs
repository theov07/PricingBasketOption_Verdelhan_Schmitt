using System;

namespace BasketOptionPricer
{
    // Types d'options possibles
    public enum OptionType
    {
        Call,
        Put
    }
    
    // Classe pour une option sur panier
    public class BasketOption
    {
        public Basket Basket { get; set; }
        public OptionType Type { get; set; } // call ou put
        public double Strike { get; set; } // prix d'exercice K
        public double Maturity { get; set; } // maturité T
        
        public BasketOption(Basket basket, OptionType type, double strike, double maturity)
        {
            // vérifications basiques
            if (strike <= 0)
                throw new ArgumentException("Strike doit être positif");
            if (maturity <= 0)
                throw new ArgumentException("Maturité doit être positive");
                
            Basket = basket;
            Type = type;
            Strike = strike;
            Maturity = maturity;
        }
        
        public double Payoff(double basketValue)
        {
            switch (Type)
            {
                case OptionType.Call:
                    return Math.Max(basketValue - Strike, 0);
                case OptionType.Put:
                    return Math.Max(Strike - basketValue, 0);
                default:
                    throw new ArgumentException("Invalid option type");
            }
        }
    }
}