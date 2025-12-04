using System;

namespace BasketOptionPricer
{
    public class MomentMatchingPricer
    {
        public static double Price(BasketOption option)
        {
            Basket basket = option.Basket;
            double maturity = option.Maturity;
            double riskFreeRate = basket.RiskFreeRate;
            
            double A0 = basket.GetBasketValue();
            double M1 = CalculateFirstMoment(basket, maturity);
            double M2 = CalculateSecondMoment(basket, maturity);
            
            if (M2 <= M1 * M1)
            {
                M2 = M1 * M1 * 1.0001;
            }
            
            double sigmaHatSquared = (1.0 / maturity) * Math.Log(M2 / (M1 * M1));
            double muHat = (1.0 / maturity) * Math.Log(M1 / A0);
            double sigmaHat = Math.Sqrt(sigmaHatSquared);
            
            return CalculateOptionPrice(M1, option.Strike, riskFreeRate, sigmaHat, maturity, option.Type);
        }
        
        private static double CalculateFirstMoment(Basket basket, double maturity)
        {
            double M1 = 0;
            
            for (int i = 0; i < basket.Stocks.Count; i++)
            {
                Stock stock = basket.Stocks[i];
                double ai = basket.Weights[i];
                double Si0 = stock.SpotPrice;
                double qi = stock.DividendRate;
                double r = basket.RiskFreeRate;
                
                double Fi0 = Si0 * Math.Exp((r - qi) * maturity);
                M1 += ai * Fi0;
            }
            
            return M1;
        }
        
        private static double CalculateSecondMoment(Basket basket, double maturity)
        {
            double M2 = 0;
            
            for (int i = 0; i < basket.Stocks.Count; i++)
            {
                for (int j = 0; j < basket.Stocks.Count; j++)
                {
                    Stock stockI = basket.Stocks[i];
                    Stock stockJ = basket.Stocks[j];
                    
                    double ai = basket.Weights[i];
                    double aj = basket.Weights[j];
                    double Si0 = stockI.SpotPrice;
                    double Sj0 = stockJ.SpotPrice;
                    double qi = stockI.DividendRate;
                    double qj = stockJ.DividendRate;
                    double sigmaI = stockI.Volatility;
                    double sigmaJ = stockJ.Volatility;
                    double rhoIJ = basket.CorrelationMatrix[i, j];
                    double r = basket.RiskFreeRate;
                    
                    double Fi0 = Si0 * Math.Exp((r - qi) * maturity);
                    double Fj0 = Sj0 * Math.Exp((r - qj) * maturity);
                    
                    double integralSigmaIJ = sigmaI * sigmaJ * maturity;
                    double covarianceTerm = Math.Exp(rhoIJ * integralSigmaIJ);
                    
                    M2 += ai * aj * Fi0 * Fj0 * covarianceTerm;
                }
            }
            
            return M2;
        }
        
        private static double CalculateOptionPrice(double M1, double strike, double riskFreeRate, double sigmaHat, double maturity, OptionType optionType)
        {
            if (sigmaHat <= 0 || maturity <= 0)
            {
                switch (optionType)
                {
                    case OptionType.Call:
                        return Math.Max(M1 - strike, 0) * Math.Exp(-riskFreeRate * maturity);
                    case OptionType.Put:
                        return Math.Max(strike - M1, 0) * Math.Exp(-riskFreeRate * maturity);
                }
            }
            
            double sigmaHatSqrtT = sigmaHat * Math.Sqrt(maturity);
            double d1 = (Math.Log(M1 / strike) + 0.5 * sigmaHat * sigmaHat * maturity) / sigmaHatSqrtT;
            double d2 = d1 - sigmaHatSqrtT;
            
            double discountFactor = Math.Exp(-riskFreeRate * maturity);
            
            switch (optionType)
            {
                case OptionType.Call:
                    return discountFactor * (M1 * MathUtils.NormalCdf(d1) - strike * MathUtils.NormalCdf(d2));
                case OptionType.Put:
                    return discountFactor * (strike * MathUtils.NormalCdf(-d2) - M1 * MathUtils.NormalCdf(-d1));
                default:
                    throw new ArgumentException("Invalid option type");
            }
        }
    }
}