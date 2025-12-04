using System;

namespace BasketOptionPricer
{
    public enum OptionType
    {
        Call,
        Put
    }
    
    public class BasketOption
    {
        public Basket Basket { get; set; }
        public OptionType Type { get; set; }
        public double Strike { get; set; }
        public double Maturity { get; set; }
        
        public BasketOption(Basket basket, OptionType type, double strike, double maturity)
        {
            if (strike <= 0)
                throw new ArgumentException("Strike must be positive");
            if (maturity <= 0)
                throw new ArgumentException("Maturity must be positive");
                
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