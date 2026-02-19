using System;
using System.Collections.Generic;

namespace BasketOptionPricer
{
    /// Tests fonctionnels pour valider les scénarios
    public static class FunctionalTests
    {
        public static void RunAllTests()
        {
            Console.WriteLine("🔧 TESTS FONCTIONNELS");
            Console.WriteLine("═══════════════════════");
            
            var tests = new Dictionary<string, Func<bool>>
            {
                { "Scénario 1: Panier 2 actifs ATM", TestTwoAssetBasketATM },
                { "Scénario 2: Panier 3 actifs diversifié", TestThreeAssetDiversified },
                { "Scénario 3: Convergence H1 vs H2", TestH1H2Convergence },
                { "Scénario 4: Monte Carlo vs Moment Matching", TestMonteCarloVsMomentMatching },
                { "Scénario 5: Réduction de variance", TestVarianceReduction },
                { "Scénario 6: Sensibilité à la corrélation", TestCorrelationSensitivity },
                { "Scénario 7: Paramètres déterministes H2", TestDeterministicParametersH2 },
                { "Scénario 8: Put-Call relationship", TestPutCallRelationship }
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
                        Console.WriteLine("✅ PASSED");
                        passed++;
                    }
                    else
                    {
                        Console.WriteLine("❌ FAILED");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ ERROR: {ex.Message}");
                }
            }
            
            Console.WriteLine();
            Console.WriteLine($"Summary: {passed}/{total} tests passed ({passed * 100.0 / total:F1}%)");
            Console.WriteLine(passed == total ? "✅ ALL TESTS PASSED" : "❌ SOME TESTS FAILED");
        }
        
        private static bool TestTwoAssetBasketATM()
        {
            // Test with 2-asset At-The-Money basket
            var stocks = new List<Stock>
            {
                new Stock("Asset1", 100.0, 0.20, 0.02),
                new Stock("Asset2", 120.0, 0.25, 0.015)
            };
            
            double[] weights = { 0.5, 0.5 };
            double[,] correlation = { { 1.0, 0.3 }, { 0.3, 1.0 } };
            var basket = new Basket(stocks, weights, correlation, 0.01933); // €STR ECB 23/01/2026
            
            double basketValue = basket.GetBasketValue(); // 110.0
            var callOption = new BasketOption(basket, OptionType.Call, basketValue, 1.0);
            var putOption = new BasketOption(basket, OptionType.Put, basketValue, 1.0);
            
            double callPrice = MomentMatchingPricer.Price(callOption);
            double putPrice = MomentMatchingPricer.Price(putOption);
            
            // Consistency checks
            return callPrice > 0 && putPrice > 0 && 
                   callPrice > putPrice && // Call ATM > Put ATM with r > 0
                   callPrice < basketValue && // Call < Spot
                   Math.Abs(callPrice - putPrice) < basketValue * 0.2;
        }
        
        private static bool TestThreeAssetDiversified()
        {
            // Test with 3-asset diversified basket
            var stocks = new List<Stock>
            {
                new Stock("Tech", 150.0, 0.30, 0.01),
                new Stock("Finance", 80.0, 0.20, 0.03),
                new Stock("Energy", 120.0, 0.35, 0.02)
            };
            
            double[] weights = { 0.5, 0.3, 0.2 };
            double[,] correlation = new double[,]
            {
                { 1.0, 0.4, 0.2 },
                { 0.4, 1.0, 0.3 },
                { 0.2, 0.3, 1.0 }
            };
            
            var basket = new Basket(stocks, weights, correlation, 0.01933); // €STR ECB
            var option = new BasketOption(basket, OptionType.Call, basket.GetBasketValue() * 1.1, 1.5);
            
            double price = MomentMatchingPricer.Price(option);
            
            // Consistency test with OTM option
            return price > 0 && price < basket.GetBasketValue() * 0.3;
        }
        
        private static bool TestH1H2Convergence()
        {
            // Test H2 convergence to H1 with constant parameters
            var stockH1 = new Stock("Test", 100.0, 0.20, 0.02);
            var basketH1 = new Basket(new List<Stock> { stockH1 }, new double[] { 1.0 }, 
                                    new double[,] { { 1.0 } }, 0.01933); // €STR ECB
            var optionH1 = new BasketOption(basketH1, OptionType.Call, 105.0, 1.0);
            
            // H2 with equivalent constant parameters (€STR 1.933%)
            var volModel = new DeterministicVolatilityModel(0.20);
            var rateModel = new DeterministicRateModel(0.01933);
            var stockH2 = new StockH2("Test", 100.0, volModel, 0.02);
            var basketH2 = new BasketH2(new List<StockH2> { stockH2 }, new double[] { 1.0 }, 
                                      new double[,] { { 1.0 } }, rateModel);
            var optionH2 = new BasketOptionH2(basketH2, OptionType.Call, 105.0, 1.0);
            
            double priceH1 = MomentMatchingPricer.Price(optionH1);
            double priceH2 = MomentMatchingPricerH2.Price(optionH2);
            
            double relativeError = Math.Abs(priceH1 - priceH2) / priceH1 * 100;
            
            return relativeError < 1.0;
        }
        
        private static bool TestMonteCarloVsMomentMatching()
        {
            // Monte Carlo vs Moment Matching comparison
            var vol1 = new DeterministicVolatilityModel(0.20);
            var vol2 = new DeterministicVolatilityModel(0.25);
            var rateModel = new DeterministicRateModel(0.01933); // €STR ECB
            
            var stocksH2 = new List<StockH2>
            {
                new StockH2("Asset1", 100.0, vol1, 0.02),
                new StockH2("Asset2", 110.0, vol2, 0.015)
            };
            
            double[] weights = { 0.6, 0.4 };
            double[,] correlation = { { 1.0, 0.4 }, { 0.4, 1.0 } };
            
            var basketH2 = new BasketH2(stocksH2, weights, correlation, rateModel);
            var optionH2 = new BasketOptionH2(basketH2, OptionType.Call, 105.0, 1.0);
            
            double mmPrice = MomentMatchingPricerH2.Price(optionH2);
            
            var mcPricer = new MonteCarloPricerH2(42);
            var mcResult = mcPricer.Price(optionH2, 100000, false);
            
            double relativeError = Math.Abs(mmPrice - mcResult.Price) / mmPrice * 100;
            
            return relativeError < 5.0 && mcResult.StandardError < 0.1;
        }
        
        private static bool TestVarianceReduction()
        {
            // Monte Carlo variance reduction test
            var vol = new DeterministicVolatilityModel(0.20);
            var rateModel = new DeterministicRateModel(0.01933); // €STR ECB
            var stockH2 = new StockH2("Test", 100.0, vol, 0.02);
            
            var basketH2 = new BasketH2(new List<StockH2> { stockH2 }, new double[] { 1.0 }, 
                                      new double[,] { { 1.0 } }, rateModel);
            var optionH2 = new BasketOptionH2(basketH2, OptionType.Call, 105.0, 1.0);
            
            var mcPricer = new MonteCarloPricerH2(42);
            var resultStandard = mcPricer.Price(optionH2, 50000, false);
            var resultWithCV = mcPricer.Price(optionH2, 50000, true);
            
            // Variance reduction should decrease standard error
            return resultWithCV.StandardError < resultStandard.StandardError &&
                   resultWithCV.VarianceReduction > 30.0;
        }
        
        private static bool TestCorrelationSensitivity()
        {
            // Correlation sensitivity test
            var stocks = new List<Stock>
            {
                new Stock("Asset1", 100.0, 0.20, 0.02),
                new Stock("Asset2", 100.0, 0.20, 0.02)
            };
            
            double[] weights = { 0.5, 0.5 };
            
            // Low correlation
            double[,] corrLow = { { 1.0, 0.1 }, { 0.1, 1.0 } };
            var basketLow = new Basket(stocks, weights, corrLow, 0.01933); // €STR ECB
            var optionLow = new BasketOption(basketLow, OptionType.Call, 100.0, 1.0);
            double priceLow = MomentMatchingPricer.Price(optionLow);
            
            // High correlation
            double[,] corrHigh = { { 1.0, 0.9 }, { 0.9, 1.0 } };
            var basketHigh = new Basket(stocks, weights, corrHigh, 0.01933); // €STR ECB
            var optionHigh = new BasketOption(basketHigh, OptionType.Call, 100.0, 1.0);
            double priceHigh = MomentMatchingPricer.Price(optionHigh);
            
            return priceHigh > priceLow && (priceHigh - priceLow) / priceLow > 0.05;
        }
        
        private static bool TestDeterministicParametersH2()
        {
            // Test with truly deterministic (non-constant) parameters
            var rateModel = new DeterministicRateModel();
            rateModel.AddRatePoint(0.0, 0.02);
            rateModel.AddRatePoint(1.0, 0.04);
            
            var volModel = new DeterministicVolatilityModel();
            volModel.AddVolatilityPoint(0.0, 0.15);
            volModel.AddVolatilityPoint(1.0, 0.25);
            
            var stockH2 = new StockH2("Test", 100.0, volModel, 0.02);
            var basketH2 = new BasketH2(new List<StockH2> { stockH2 }, new double[] { 1.0 }, 
                                      new double[,] { { 1.0 } }, rateModel);
            
            var optionH2 = new BasketOptionH2(basketH2, OptionType.Call, 105.0, 1.0);
            double price = MomentMatchingPricerH2.Price(optionH2);
            
            // Comparison with constant parameters (€STR)
            var constantRate = new DeterministicRateModel(0.01933);
            var constantVol = new DeterministicVolatilityModel(0.20);
            var stockConstant = new StockH2("Test", 100.0, constantVol, 0.02);
            var basketConstant = new BasketH2(new List<StockH2> { stockConstant }, new double[] { 1.0 }, 
                                            new double[,] { { 1.0 } }, constantRate);
            var optionConstant = new BasketOptionH2(basketConstant, OptionType.Call, 105.0, 1.0);
            double priceConstant = MomentMatchingPricerH2.Price(optionConstant);
            
            return Math.Abs(price - priceConstant) / priceConstant > 0.01;
        }
        
        private static bool TestPutCallRelationship()
        {
            // Test Put-Call relationship
            var stocks = new List<Stock>
            {
                new Stock("Test", 100.0, 0.20, 0.02)
            };
            
            var basket = new Basket(stocks, new double[] { 1.0 }, new double[,] { { 1.0 } }, 0.01933); // €STR ECB
            
            // ATM Options
            var callATM = new BasketOption(basket, OptionType.Call, 100.0, 1.0);
            var putATM = new BasketOption(basket, OptionType.Put, 100.0, 1.0);
            
            double callPriceATM = MomentMatchingPricer.Price(callATM);
            double putPriceATM = MomentMatchingPricer.Price(putATM);
            
            // ITM/OTM Options
            var callITM = new BasketOption(basket, OptionType.Call, 95.0, 1.0);
            var putOTM = new BasketOption(basket, OptionType.Put, 95.0, 1.0);
            
            double callPriceITM = MomentMatchingPricer.Price(callITM);
            double putPriceOTM = MomentMatchingPricer.Price(putOTM);
            
            // Consistency checks
            return callPriceATM > putPriceATM && // Call ATM > Put ATM with positive rate
                   callPriceITM > callPriceATM && // Call ITM > Call ATM
                   putPriceOTM < putPriceATM && // Put OTM < Put ATM
                   callPriceITM > putPriceOTM; // Call ITM > Put OTM
        }
    }
}