using System;

namespace BasketOptionPricer
{
    public static class MathUtils
    {
        public static double NormalCdf(double x)
        {
            return 0.5 * (1.0 + Erf(x / Math.Sqrt(2.0)));
        }
        
        private static double Erf(double x)
        {
            double a1 = 0.254829592;
            double a2 = -0.284496736;
            double a3 = 1.421413741;
            double a4 = -1.453152027;
            double a5 = 1.061405429;
            double p = 0.3275911;
            
            int sign = x < 0 ? -1 : 1;
            x = Math.Abs(x);
            
            double t = 1.0 / (1.0 + p * x);
            double y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-x * x);
            
            return sign * y;
        }
        
        public static double BlackScholesPrice(double s, double k, double r, double sigma, double t, OptionType optionType)
        {
            double d1 = (Math.Log(s / k) + (r + 0.5 * sigma * sigma) * t) / (sigma * Math.Sqrt(t));
            double d2 = d1 - sigma * Math.Sqrt(t);
            
            switch (optionType)
            {
                case OptionType.Call:
                    return s * NormalCdf(d1) - k * Math.Exp(-r * t) * NormalCdf(d2);
                case OptionType.Put:
                    return k * Math.Exp(-r * t) * NormalCdf(-d2) - s * NormalCdf(-d1);
                default:
                    throw new ArgumentException("Invalid option type");
            }
        }
    }
}