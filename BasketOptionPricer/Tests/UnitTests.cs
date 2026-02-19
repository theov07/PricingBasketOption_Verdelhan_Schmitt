using System;
using System.Collections.Generic;

namespace BasketOptionPricer
{
    // Unit tests to verify everything works correctly
    public static class UnitTests
    {
        public static void RunAllTests()
        {
            Console.WriteLine("Unit Tests");
            Console.WriteLine("==========");
            
            var tests = new Dictionary<string, Func<bool>>
            {
                { "Normal CDF test", TestNormalCdf },
                { "Black-Scholes test", TestBlackScholes },
                { "Stock construction", TestStockConstruction },
                { "Basket construction", TestBasketConstruction },
                { "Moment Matching pricer", TestMomentMatchingPricer },
                { "H2 Models", TestH2Models },
                { "Pricing consistency", TestPricingConsistency }
            };
            
            int passed = 0;
            int total = tests.Count;
            
            foreach (var test in tests)
            {
                Console.Write($"   {test.Key}... ");
                try
                {
                    if (test.Value())
                    {
                        Console.WriteLine("OK");
                        passed++;
                    }
                    else
                    {
                        Console.WriteLine("FAILED");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: {ex.Message}");
                }
            }
            
            Console.WriteLine();
            Console.WriteLine($"Results: {passed}/{total} tests passed");
            
            if (passed == total)
                Console.WriteLine("All tests OK!");
            else
                Console.WriteLine("Some tests failed");
        }
        
        private static bool TestNormalCdf()
        {
            // Test normal distribution CDF
            double result1 = MathUtils.NormalCdf(0.0);
            double result2 = MathUtils.NormalCdf(-1.96);
            double result3 = MathUtils.NormalCdf(1.96);
            
            // N(0) = 0.5, N(-1.96) ≈ 0.025, N(1.96) ≈ 0.975
            return Math.Abs(result1 - 0.5) < 1e-6 &&
                   Math.Abs(result2 - 0.025) < 1e-3 &&
                   Math.Abs(result3 - 0.975) < 1e-3;
        }
        
        private static bool TestBlackScholes()
        {
            // Test Black-Scholes formula
            double spot = 100.0;
            double strike = 100.0;
            double rate = 0.05;
            double volatility = 0.20;
            double maturity = 1.0;
            
            double callPrice = MathUtils.BlackScholesPrice(spot, strike, rate, volatility, maturity, OptionType.Call);
            double putPrice = MathUtils.BlackScholesPrice(spot, strike, rate, volatility, maturity, OptionType.Put);
            
            // Vérification put-call parity: C - P = S - K*e^(-rT)
            double expectedDiff = spot - strike * Math.Exp(-rate * maturity);
            double actualDiff = callPrice - putPrice;
            
            return Math.Abs(actualDiff - expectedDiff) < 1e-10 && callPrice > 0 && putPrice > 0;
        }
        
        private static bool TestStockConstruction()
        {
            // Test stock construction
            try
            {
                var stock = new Stock("Test", 100.0, 0.20, 0.02);
                
                return stock.Name == "Test" &&
                       Math.Abs(stock.SpotPrice - 100.0) < 1e-10 &&
                       Math.Abs(stock.Volatility - 0.20) < 1e-10 &&
                       Math.Abs(stock.DividendRate - 0.02) < 1e-10;
            }
            catch
            {
                return false;
            }
        }
        
        private static bool TestBasketConstruction()
        {
            // Test basket construction
            try
            {
                var stocks = new List<Stock>
                {
                    new Stock("A", 100.0, 0.20, 0.02),
                    new Stock("B", 120.0, 0.25, 0.015)
                };
                
                double[] weights = { 0.6, 0.4 };
                double[,] correlation = { { 1.0, 0.3 }, { 0.3, 1.0 } };
                double riskFreeRate = 0.01933; // €STR ECB 23/01/2026
                
                var basket = new Basket(stocks, weights, correlation, riskFreeRate);
                
                double expectedValue = 0.6 * 100.0 + 0.4 * 120.0; // 108.0
                double actualValue = basket.GetBasketValue();
                
                return Math.Abs(actualValue - expectedValue) < 1e-10;
            }
            catch
            {
                return false;
            }
        }
        
        private static bool TestMomentMatchingPricer()
        {
            // Test Moment Matching pricer
            try
            {
                var stock = new Stock("Test", 100.0, 0.20, 0.02);
                var basket = new Basket(new List<Stock> { stock }, new double[] { 1.0 }, 
                                      new double[,] { { 1.0 } }, 0.01933); // €STR ECB
                
                var callOption = new BasketOption(basket, OptionType.Call, 105.0, 1.0);
                var putOption = new BasketOption(basket, OptionType.Put, 95.0, 1.0);
                
                double callPrice = MomentMatchingPricer.Price(callOption);
                double putPrice = MomentMatchingPricer.Price(putOption);
                
                // Consistency checks
                return callPrice > 0 && putPrice > 0 && 
                       callPrice < basket.GetBasketValue() && // Call < Spot
                       putPrice < callOption.Strike; // Put < Strike for ITM put
            }
            catch
            {
                return false;
            }
        }
        
        private static bool TestH2Models()
        {
            // Test H2 models (deterministic parameters)
            try
            {
                // Test DeterministicRateModel
                var rateModel = new DeterministicRateModel();
                rateModel.AddRatePoint(0.0, 0.02);
                rateModel.AddRatePoint(1.0, 0.04);
                
                double rate05 = rateModel.GetRate(0.5);
                bool rateTest = Math.Abs(rate05 - 0.03) < 1e-10; // Linear interpolation
                
                // Test DeterministicVolatilityModel
                var volModel = new DeterministicVolatilityModel();
                volModel.AddVolatilityPoint(0.0, 0.20);
                volModel.AddVolatilityPoint(1.0, 0.30);
                
                double vol05 = volModel.GetVolatility(0.5);
                bool volTest = Math.Abs(vol05 - 0.25) < 1e-10; // Linear interpolation
                
                // Test StockH2
                var stockH2 = new StockH2("Test", 100.0, volModel, 0.02);
                bool stockH2Test = stockH2.Name == "Test" && Math.Abs(stockH2.SpotPrice - 100.0) < 1e-10;
                
                return rateTest && volTest && stockH2Test;
            }
            catch
            {
                return false;
            }
        }
        
        private static bool TestPricingConsistency()
        {
            // Test pricing consistency
            try
            {
                var stocks = new List<Stock>
                {
                    new Stock("A", 100.0, 0.20, 0.02),
                    new Stock("B", 110.0, 0.25, 0.015)
                };
                
                double[] weights = { 0.5, 0.5 };
                double[,] correlation = { { 1.0, 0.4 }, { 0.4, 1.0 } };
                var basket = new Basket(stocks, weights, correlation, 0.01933); // €STR ECB
                
                // Test with different strikes
                var atmCall = new BasketOption(basket, OptionType.Call, basket.GetBasketValue(), 1.0);
                var otmCall = new BasketOption(basket, OptionType.Call, basket.GetBasketValue() * 1.1, 1.0);
                var itmCall = new BasketOption(basket, OptionType.Call, basket.GetBasketValue() * 0.9, 1.0);
                
                double atmPrice = MomentMatchingPricer.Price(atmCall);
                double otmPrice = MomentMatchingPricer.Price(otmCall);
                double itmPrice = MomentMatchingPricer.Price(itmCall);
                
                // Consistency checks: ITM > ATM > OTM
                return itmPrice > atmPrice && atmPrice > otmPrice && otmPrice > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}