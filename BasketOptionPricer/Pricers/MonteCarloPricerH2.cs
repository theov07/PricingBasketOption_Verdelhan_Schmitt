using System;

namespace BasketOptionPricer
{
    /// Monte Carlo simulation result with variance information
    public class MonteCarloResultH2
    {
        public double Price { get; set; }
        public double StandardError { get; set; }
        public double Variance { get; set; }
        public double ControlVariateAdjustment { get; set; } = 0.0;
        public double VarianceReduction { get; set; } = 0.0;
    }
    
    /// Monte Carlo pricer for H2 approach with deterministic parameters
    public class MonteCarloPricerH2
    {
        private readonly Random _random;
        private readonly int _seed;
        
        public MonteCarloPricerH2(int seed = 42)
        {
            _seed = seed;
            _random = new Random(seed);
        }
        
        /// Price the option via Monte Carlo with deterministic parameters
        public MonteCarloResultH2 Price(BasketOptionH2 option, int numSimulations = 100000, bool useControlVariate = false)
        {
            BasketH2 basket = option.Basket;
            double maturity = option.Maturity;
            int numAssets = basket.Stocks.Count;
            
            // Précalcul des paramètres
            double[] spots = new double[numAssets];
            double[] dividends = new double[numAssets];
            double[] weights = new double[numAssets];
            
            for (int i = 0; i < numAssets; i++)
            {
                spots[i] = basket.Stocks[i].SpotPrice;
                dividends[i] = basket.Stocks[i].DividendRate;
                weights[i] = basket.Weights[i];
            }
            
            // Décomposition de Cholesky pour les corrélations
            double[,] choleskyMatrix = MathUtils.CholeskyDecomposition(basket.CorrelationMatrix);
            
            // Simulation
            double sum = 0.0;
            double sumSquared = 0.0;
            double controlVariateSum = 0.0;
            double controlVariateSumSquared = 0.0;
            double covarianceSum = 0.0;
            
            for (int sim = 0; sim < numSimulations; sim++)
            {
                // Génération des nombres aléatoires corrélés
                double[] randomNumbers = GenerateCorrelatedRandomNumbers(choleskyMatrix, numAssets);
                
                // Evolution des prix avec paramètres déterministes
                double[] finalPrices = SimulatePaths(basket, randomNumbers, maturity);
                
                // Calcul de la valeur du panier
                double basketValue = 0.0;
                for (int i = 0; i < numAssets; i++)
                {
                    basketValue += weights[i] * finalPrices[i];
                }
                
                // Payoff de l'option
                double payoff = option.CalculatePayoff(basketValue);
                double discountedPayoff = payoff * basket.RateModel.GetDiscountFactor(maturity);
                
                sum += discountedPayoff;
                sumSquared += discountedPayoff * discountedPayoff;
                
                if (useControlVariate)
                {
                    double controlValue = CalculateControlVariate(basket, finalPrices, option.Strike, option.Type);
                    controlVariateSum += controlValue;
                    controlVariateSumSquared += controlValue * controlValue;
                    covarianceSum += discountedPayoff * controlValue;
                }
            }
            
            // Calcul des statistiques
            double price = sum / numSimulations;
            double variance = (sumSquared / numSimulations - price * price);
            double standardError = Math.Sqrt(variance / numSimulations);
            
            var result = new MonteCarloResultH2
            {
                Price = price,
                StandardError = standardError,
                Variance = variance
            };
            
            // Adjustment par variable de contrôle si utilisée
            if (useControlVariate && numSimulations > 1)
            {
                ApplyControlVariateReduction(result, sum, controlVariateSum, 
                    controlVariateSumSquared, covarianceSum, numSimulations);
            }
            
            return result;
        }

        private double[] SimulatePaths(BasketH2 basket, double[] randomNumbers, double maturity)
        {
            int numAssets = basket.Stocks.Count;
            int numSteps = Math.Max(252, (int)(maturity * 365));
            double dt = maturity / numSteps;
            
            double[] prices = new double[numAssets];
            
            // Initialization
            for (int i = 0; i < numAssets; i++)
            {
                prices[i] = basket.Stocks[i].SpotPrice;
            }
            
            // Euler discretization evolution
            for (int step = 0; step < numSteps; step++)
            {
                double t = step * dt;
                double rate = basket.RateModel.GetRate(t);
                
                for (int i = 0; i < numAssets; i++)
                {
                    double dividend = basket.Stocks[i].DividendRate;
                    double volatility = basket.Stocks[i].VolatilityModel.GetVolatility(t);
                    
                    // dS = S * (r - q) * dt + S * σ(t) * dW
                    double drift = (rate - dividend) * dt;
                    double diffusion = volatility * Math.Sqrt(dt) * randomNumbers[i];
                    
                    prices[i] *= Math.Exp(drift - 0.5 * volatility * volatility * dt + diffusion);
                }
                
                // Generate new random numbers for next step
                if (step < numSteps - 1)
                {
                    randomNumbers = GenerateCorrelatedRandomNumbers(
                        MathUtils.CholeskyDecomposition(basket.CorrelationMatrix), numAssets);
                }
            }
            
            return prices;
        }
        
        /// Generate correlated random numbers via Cholesky decomposition
        private double[] GenerateCorrelatedRandomNumbers(double[,] choleskyMatrix, int numAssets)
        {
            double[] independentRandom = new double[numAssets];
            double[] correlatedRandom = new double[numAssets];
            
            // Generate independent N(0,1) random numbers
            for (int i = 0; i < numAssets; i++)
            {
                independentRandom[i] = MathUtils.GenerateNormalRandom(_random);
            }
            
            // Apply Cholesky matrix
            for (int i = 0; i < numAssets; i++)
            {
                correlatedRandom[i] = 0;
                for (int j = 0; j <= i; j++)
                {
                    correlatedRandom[i] += choleskyMatrix[i, j] * independentRandom[j];
                }
            }
            
            return correlatedRandom;
        }
        
        /// Calculate control variate (geometric mean of basket)
        private double CalculateControlVariate(BasketH2 basket, double[] finalPrices, double strike, OptionType optionType)
        {
            // Use geometric mean as control variate
            double geometricMean = 1.0;
            for (int i = 0; i < finalPrices.Length; i++)
            {
                geometricMean *= Math.Pow(finalPrices[i], basket.Weights[i]);
            }
            
            return optionType switch
            {
                OptionType.Call => Math.Max(geometricMean - strike, 0),
                OptionType.Put => Math.Max(strike - geometricMean, 0),
                _ => 0
            };
        }
        
        /// Apply variance reduction via control variate
        private void ApplyControlVariateReduction(MonteCarloResultH2 result, double sum, 
            double controlVariateSum, double controlVariateSumSquared, 
            double covarianceSum, int numSimulations)
        {
            double controlMean = controlVariateSum / numSimulations;
            double controlVariance = controlVariateSumSquared / numSimulations - controlMean * controlMean;
            double covariance = covarianceSum / numSimulations - result.Price * controlMean;
            
            if (controlVariance > 1e-10)
            {
                double beta = covariance / controlVariance;
                double adjustedPrice = result.Price - beta * controlMean;
                double varianceReduction = beta * beta * controlVariance;
                
                result.ControlVariateAdjustment = adjustedPrice - result.Price;
                result.Price = adjustedPrice;
                result.VarianceReduction = varianceReduction / result.Variance * 100;
                result.Variance = Math.Max(result.Variance - varianceReduction, 0);
                result.StandardError = Math.Sqrt(result.Variance / numSimulations);
            }
        }
    }
}