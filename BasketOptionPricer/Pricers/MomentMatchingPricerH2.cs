using System;

namespace BasketOptionPricer
{
    public class MomentMatchingPricerH2
    {
        public static double Price(BasketOptionH2 option)
        {
            BasketH2 basket = option.Basket;
            double maturity = option.Maturity;
            DeterministicRateModel rateModel = basket.RateModel;
            
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
            
            return CalculateOptionPrice(M1, option.Strike, rateModel, sigmaHat, maturity, option.Type);
        }
        
        private static double CalculateFirstMoment(BasketH2 basket, double maturity)
        {
            double M1 = 0;
            
            for (int i = 0; i < basket.Stocks.Count; i++)
            {
                StockH2 stock = basket.Stocks[i];
                double ai = basket.Weights[i];
                double Si0 = stock.SpotPrice;
                double qi = stock.DividendRate;
                
                double rateIntegral = basket.RateModel.IntegrateRate(maturity);
                double Fi0 = Si0 * Math.Exp(rateIntegral - qi * maturity);
                M1 += ai * Fi0;
            }
            
            return M1;
        }
        
        private static double CalculateSecondMoment(BasketH2 basket, double maturity)
        {
            double M2 = 0;
            
            for (int i = 0; i < basket.Stocks.Count; i++)
            {
                for (int j = 0; j < basket.Stocks.Count; j++)
                {
                    StockH2 stockI = basket.Stocks[i];
                    StockH2 stockJ = basket.Stocks[j];
                    
                    double ai = basket.Weights[i];
                    double aj = basket.Weights[j];
                    double rhoIJ = basket.CorrelationMatrix[i, j];
                    
                    double rateIntegral = basket.RateModel.IntegrateRate(maturity);
                    double Fi0 = stockI.SpotPrice * Math.Exp(rateIntegral - stockI.DividendRate * maturity);
                    double Fj0 = stockJ.SpotPrice * Math.Exp(rateIntegral - stockJ.DividendRate * maturity);
                    
                    double covarianceIntegral = CalculateCovarianceIntegral(
                        stockI.VolatilityModel, stockJ.VolatilityModel, maturity);
                    
                    double covarianceTerm = Math.Exp(rhoIJ * covarianceIntegral);
                    
                    M2 += ai * aj * Fi0 * Fj0 * covarianceTerm;
                }
            }
            
            return M2;
        }
        
        private static double CalculateCovarianceIntegral(DeterministicVolatilityModel volI, 
            DeterministicVolatilityModel volJ, double T, int numSteps = 1000)
        {
            if (T <= 0) return 0.0;
            
            double dt = T / numSteps;
            double integral = 0.0;
            
            for (int k = 0; k <= numSteps; k++)
            {
                double t = k * dt;
                double sigmaI = volI.GetVolatility(t);
                double sigmaJ = volJ.GetVolatility(t);
                double product = sigmaI * sigmaJ;
                
                if (k == 0 || k == numSteps)
                    integral += 0.5 * product * dt;
                else
                    integral += product * dt;
            }
            
            return integral;
        }
        
        private static double CalculateOptionPrice(double M1, double strike, 
            DeterministicRateModel rateModel, double sigmaHat, double maturity, OptionType optionType)
        {
            if (sigmaHat <= 0 || maturity <= 0)
            {
                double intrinsicDiscountFactor = rateModel.GetDiscountFactor(maturity);
                return optionType switch
                {
                    OptionType.Call => Math.Max(M1 - strike, 0) * intrinsicDiscountFactor,
                    OptionType.Put => Math.Max(strike - M1, 0) * intrinsicDiscountFactor,
                    _ => throw new ArgumentException("Unsupported option type")
                };
            }
            
            double sigmaHatSqrtT = sigmaHat * Math.Sqrt(maturity);
            double d1 = (Math.Log(M1 / strike) + 0.5 * sigmaHat * sigmaHat * maturity) / sigmaHatSqrtT;
            double d2 = d1 - sigmaHatSqrtT;
            
            double discountFactor = rateModel.GetDiscountFactor(maturity);
            
            return optionType switch
            {
                OptionType.Call => discountFactor * (M1 * MathUtils.NormalCdf(d1) - strike * MathUtils.NormalCdf(d2)),
                OptionType.Put => discountFactor * (strike * MathUtils.NormalCdf(-d2) - M1 * MathUtils.NormalCdf(-d1)),
                _ => throw new ArgumentException("Unsupported option type")
            };
        }
    }
}