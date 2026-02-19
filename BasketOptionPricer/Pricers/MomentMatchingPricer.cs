using System;

namespace BasketOptionPricer
{
    // H1 Pricer - Moment Matching based on Brigo et al.
    public class MomentMatchingPricer
    {
        public static double Price(BasketOption option)
        {
            Basket basket = option.Basket;
            double T = option.Maturity;
            double r = basket.RiskFreeRate;
            
            // calculate basket moments
            double basketValue = basket.GetBasketValue();
            double moment1 = CalculateFirstMoment(basket, T);
            double moment2 = CalculateSecondMoment(basket, T);
            
            if (moment2 <= moment1 * moment1)
            {
                moment2 = moment1 * moment1 * 1.0001;
            }
            
            // equivalent lognormal parameters
            double sigmaSquared = (1.0 / T) * Math.Log(moment2 / (moment1 * moment1));
            double mu = (1.0 / T) * Math.Log(moment1 / basketValue);
            double sigma = Math.Sqrt(sigmaSquared);
            
            return CalculateOptionPrice(moment1, option.Strike, r, sigma, T, option.Type);
        }
        
        // First moment calculation E[B_T]
        private static double CalculateFirstMoment(Basket basket, double T)
        {
            double result = 0;
            
            for (int i = 0; i < basket.Stocks.Count; i++)
            {
                Stock asset = basket.Stocks[i];
                double weight = basket.Weights[i];
                double S0 = asset.SpotPrice;
                double q = asset.DividendRate;
                double r = basket.RiskFreeRate;
                
                // forward price
                double forward = S0 * Math.Exp((r - q) * T);
                result += weight * forward;
            }
            
            return result;
        }
        
        // Second moment calculation E[B_T^2] with correlations
        private static double CalculateSecondMoment(Basket basket, double T)
        {
            double result = 0;
            
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
                    
                    double Fi0 = Si0 * Math.Exp((r - qi) * T);
                    double Fj0 = Sj0 * Math.Exp((r - qj) * T);
                    
                    double integralSigmaIJ = sigmaI * sigmaJ * T;
                    double covarianceTerm = Math.Exp(rhoIJ * integralSigmaIJ);
                    
                    resultat += ai * aj * Fi0 * Fj0 * covarianceTerm;
                }
            }
            
            return result;
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