using System;

namespace BasketOptionPricer
{
    public class MonteCarloResult
    {
        public double Price { get; set; }
        public double StandardError { get; set; }
        public double Variance { get; set; }
        
        public MonteCarloResult(double price, double standardError, double variance)
        {
            Price = price;
            StandardError = standardError;
            Variance = variance;
        }
    }
    
    public class MonteCarloPricer
    {
        private Random random;
        
        public MonteCarloPricer()
        {
            random = new Random();
        }
        
        public MonteCarloPricer(int seed)
        {
            random = new Random(seed);
        }
        
        public MonteCarloResult Price(BasketOption option, int simulations)
        {
            Basket basket = option.Basket;
            double maturity = option.Maturity;
            double riskFreeRate = basket.RiskFreeRate;
            
            double[,] choleskyMatrix = CholeskyDecomposition(basket.CorrelationMatrix);
            
            double[] payoffs = new double[simulations];
            double sumPayoffs = 0;
            double sumSquaredPayoffs = 0;
            
            for (int sim = 0; sim < simulations; sim++)
            {
                double[] correlatedRandoms = GenerateCorrelatedRandoms(choleskyMatrix, basket.Stocks.Count);
                
                double basketValue = 0;
                for (int i = 0; i < basket.Stocks.Count; i++)
                {
                    Stock stock = basket.Stocks[i];
                    double drift = (riskFreeRate - stock.DividendRate - 0.5 * stock.Volatility * stock.Volatility) * maturity;
                    double diffusion = stock.Volatility * Math.Sqrt(maturity) * correlatedRandoms[i];
                    double stockPrice = stock.SpotPrice * Math.Exp(drift + diffusion);
                    basketValue += basket.Weights[i] * stockPrice;
                }
                
                double payoff = option.Payoff(basketValue);
                payoffs[sim] = payoff;
                sumPayoffs += payoff;
                sumSquaredPayoffs += payoff * payoff;
            }
            
            double meanPayoff = sumPayoffs / simulations;
            double variance = (sumSquaredPayoffs / simulations) - (meanPayoff * meanPayoff);
            double standardError = Math.Sqrt(variance / simulations);
            
            double discountedPrice = Math.Exp(-riskFreeRate * maturity) * meanPayoff;
            double discountedStandardError = Math.Exp(-riskFreeRate * maturity) * standardError;
            
            return new MonteCarloResult(discountedPrice, discountedStandardError, variance);
        }
        
        private double[,] CholeskyDecomposition(double[,] matrix)
        {
            int n = matrix.GetLength(0);
            double[,] cholesky = new double[n, n];
            
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    if (i == j)
                    {
                        double sum = 0;
                        for (int k = 0; k < j; k++)
                        {
                            sum += cholesky[j, k] * cholesky[j, k];
                        }
                        cholesky[j, j] = Math.Sqrt(matrix[j, j] - sum);
                    }
                    else
                    {
                        double sum = 0;
                        for (int k = 0; k < j; k++)
                        {
                            sum += cholesky[i, k] * cholesky[j, k];
                        }
                        cholesky[i, j] = (matrix[i, j] - sum) / cholesky[j, j];
                    }
                }
            }
            
            return cholesky;
        }
        
        private double[] GenerateCorrelatedRandoms(double[,] choleskyMatrix, int size)
        {
            double[] independentRandoms = new double[size];
            for (int i = 0; i < size; i++)
            {
                independentRandoms[i] = GenerateStandardNormal();
            }
            
            double[] correlatedRandoms = new double[size];
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    correlatedRandoms[i] += choleskyMatrix[i, j] * independentRandoms[j];
                }
            }
            
            return correlatedRandoms;
        }
        
        private double GenerateStandardNormal()
        {
            double u1 = random.NextDouble();
            double u2 = random.NextDouble();
            return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
        }
    }
}