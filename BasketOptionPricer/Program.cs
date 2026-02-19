using System;
using System.Collections.Generic;
using BasketOptionPricer.Data;

namespace BasketOptionPricer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Basket Option Pricing - Verdelhan & Schmitt");
            Console.WriteLine("===========================================");
            Console.WriteLine();
            Console.WriteLine("Main menu:");
            Console.WriteLine("1. Demo H1 vs H2");
            Console.WriteLine("2. Interactive mode");
            Console.WriteLine("3. Unit tests");
            Console.WriteLine("4. Functional tests");
            Console.WriteLine("5. Vol Surface Test (Bloomberg OVDV)");
            Console.WriteLine("6. Exit");
            Console.WriteLine();
            
            while (true)
            {
                Console.Write("Your choice (1-6): ");
                string choice = Console.ReadLine()?.Trim();
                
                switch (choice)
                {
                    case "1":
                        RunDemonstration();
                        break;
                    case "2":
                        InteractiveInterface.RunInteractiveMode();
                        break;
                    case "3":
                        RunUnitTests();
                        break;
                    case "4":
                        RunFunctionalTests();
                        break;
                    case "5":
                        TestVolSurface();
                        break;
                    case "6":
                        Console.WriteLine("Goodbye!");
                        return;
                    default:
                        Console.WriteLine("Invalid choice.");
                        continue;
                }
                
                Console.WriteLine("\nReturning to main menu...");
                if (Environment.UserInteractive && !Console.IsInputRedirected)
                {
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                }
                else
                {
                    System.Threading.Thread.Sleep(1000);
                }
                Console.Clear();
                Console.WriteLine("═══════════════════════════════════════════════════════════════════");
                Console.WriteLine("       BASKET OPTION PRICING - VERDELHAN & SCHMITT - M2 272");
                Console.WriteLine("═══════════════════════════════════════════════════════════════════");
                Console.WriteLine();
                Console.WriteLine("Choose usage mode:");
                Console.WriteLine("1. Automatic demonstration (H1 vs H2)");
                Console.WriteLine("2. Interactive interface (manual input)");
                Console.WriteLine("3. Unit tests");
                Console.WriteLine("4. Functional tests");
                Console.WriteLine("5. Vol Surface Test (Bloomberg OVDV)");
                Console.WriteLine("6. Exit");
                Console.WriteLine();
            }
        }

        static void TestVolSurface()
        {
            Console.Clear();
            Console.WriteLine("════════════════════════════════════════════════════════════════════");
            Console.WriteLine("          VOLATILITY SURFACE TEST - BLOOMBERG OVDV");
            Console.WriteLine("════════════════════════════════════════════════════════════════════\n");

            string csvPath = "../ SX5E_OVDV_2026-01-28_MID.csv";
            
            try
            {
                var surface = VolSurfaceFromCsv.LoadFromCsv(csvPath, "SX5E");
                surface.PrintSummary();

                Console.WriteLine("\n--- Interpolation ATM (moneyness = 1.0) ---");
                foreach (double T in new[] { 0.01, 0.05, 0.10, 0.25, 0.40 })
                {
                    double vol = surface.GetVolByMoneyness(T, 1.0);
                    Console.WriteLine($"  T = {T:F2}y -> σ = {vol * 100:F2}%");
                }

                Console.WriteLine("\n--- Smile de volatilité (T = 0.25y) ---");
                double forward = 5990.0;
                foreach (double m in new[] { 0.80, 0.90, 0.95, 1.00, 1.05, 1.10, 1.20 })
                {
                    double vol = surface.GetVolByMoneyness(0.247, m);
                    double strike = m * forward;
                    Console.WriteLine($"  K = {strike:F0} (m = {m:F2}) -> σ = {vol * 100:F2}%");
                }

                Console.WriteLine("\n--- Test GetVolByStrike ---");
                double testVol = surface.GetVolByStrike(0.25, 6000, 5990);
                Console.WriteLine($"  GetVolByStrike(T=0.25, K=6000, F=5990) = {testVol * 100:F2}%");

                Console.WriteLine("\n✓ Surface loaded and interpolation OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Check that file {csvPath} exists.");
            }

            Console.WriteLine("\n════════════════════════════════════════════════════════════════════");
        }
        
        static void RunUnitTests()
        {
            Console.Clear();;
            Console.WriteLine("════════════════════════════════════════════════════════════════════");
            Console.WriteLine("                           UNIT TESTS");
            Console.WriteLine("════════════════════════════════════════════════════════════════════");
            Console.WriteLine();
            
            UnitTests.RunAllTests();
            
            Console.WriteLine();
            Console.WriteLine("════════════════════════════════════════════════════════════════════");
            Console.WriteLine("                        END OF UNIT TESTS");
            Console.WriteLine("════════════════════════════════════════════════════════════════════");
        }
        
        static void RunFunctionalTests()
        {
            Console.Clear();
            Console.WriteLine("════════════════════════════════════════════════════════════════════");
            Console.WriteLine("                        FUNCTIONAL TESTS");
            Console.WriteLine("════════════════════════════════════════════════════════════════════");
            Console.WriteLine();
            
            FunctionalTests.RunAllTests();
            
            Console.WriteLine();
            Console.WriteLine("════════════════════════════════════════════════════════════════════");
            Console.WriteLine("                     END OF FUNCTIONAL TESTS");
            Console.WriteLine("════════════════════════════════════════════════════════════════════");
        }
        
        static void RunDemonstration()
        {
            Console.Clear();
            Console.WriteLine("════════════════════════════════════════════════════════════════════");
            Console.WriteLine("                        AUTOMATIC DEMONSTRATION");
            Console.WriteLine("════════════════════════════════════════════════════════════════════");
            Console.WriteLine("Implementation: Moment Matching (Brigo et al. - sections 3.2-3.3)");
            Console.WriteLine();
            
            // Demonstration H1 vs H2 approach
            DemonstrateH1Approach();
            Console.WriteLine();
            DemonstrateH2Approach();
            
            Console.WriteLine();
            Console.WriteLine("════════════════════════════════════════════════════════════════════");
            Console.WriteLine("                        END OF DEMONSTRATION");
            Console.WriteLine("════════════════════════════════════════════════════════════════════");
        }
        
        static void DemonstrateH1Approach()
        {
            Console.WriteLine("🔶 H1 APPROACH: Constant parameters (fixed r, σ)");
            Console.WriteLine("─────────────────────────────────────────────────");
            
            // Basket configuration
            var stocks = new List<Stock>
            {
                new Stock("Stock A", 100.0, 0.20, 0.02),
                new Stock("Stock B", 120.0, 0.25, 0.015),
                new Stock("Stock C", 80.0, 0.18, 0.025)
            };
            
            double[] weights = { 0.5, 0.3, 0.2 };
            double[,] correlation = new double[,]
            {
                { 1.0, 0.3, 0.2 },
                { 0.3, 1.0, 0.4 },
                { 0.2, 0.4, 1.0 }
            };
            
            // €STR rate ECB as of 23/01/2026: 1.933%
            var basket = new Basket(stocks, weights, correlation, 0.01933);
            double basketValue = basket.GetBasketValue();
            
            var callOption = new BasketOption(basket, OptionType.Call, basketValue * 1.05, 1.0);
            var putOption = new BasketOption(basket, OptionType.Put, basketValue * 0.95, 1.0);
            
            Console.WriteLine($"• Initial basket value: {basketValue:F2} €");
            Console.WriteLine($"• Call Strike (105%): {callOption.Strike:F2} €");
            Console.WriteLine($"• Put Strike (95%): {putOption.Strike:F2} €");
            Console.WriteLine($"• Maturity: {callOption.Maturity} year");
            Console.WriteLine($"• Risk-free rate: {basket.RiskFreeRate:P1}");
            Console.WriteLine();
            
            // Pricing
            double callPrice = MomentMatchingPricer.Price(callOption);
            double putPrice = MomentMatchingPricer.Price(putOption);
            
            Console.WriteLine("H1 Results:");
            Console.WriteLine($"├─ Call Price: {callPrice:F4} €");
            Console.WriteLine($"└─ Put Price:  {putPrice:F4} €");
            
            // Black-Scholes validation for single asset
            ValidateBlackScholes();
        }
        
        static void DemonstrateH2Approach()
        {
            Console.WriteLine("🔷 H2 APPROACH: Deterministic parameters (r(t), σ(t))");
            Console.WriteLine("──────────────────────────────────────────────────────");
            
            // €STR rate curve ECB as of 23/01/2026: 1.933% (constant for simplification)
            var rateModel = new DeterministicRateModel();
            rateModel.AddRatePoint(0.0, 0.01933);  // €STR at t=0
            rateModel.AddRatePoint(0.5, 0.01933);  // €STR at t=0.5
            rateModel.AddRatePoint(1.0, 0.01933);  // €STR at t=1
            
            Console.WriteLine("Rate curve r(t):");
            Console.WriteLine($"├─ r(0) = {rateModel.GetRate(0.0):P2}");
            Console.WriteLine($"├─ r(0.5) = {rateModel.GetRate(0.5):P2}");
            Console.WriteLine($"└─ r(1) = {rateModel.GetRate(1.0):P2}");
            Console.WriteLine();
            
            // Deterministic volatilities
            var vol1 = new DeterministicVolatilityModel();
            vol1.AddVolatilityPoint(0.0, 0.20);
            vol1.AddVolatilityPoint(0.5, 0.25);
            vol1.AddVolatilityPoint(1.0, 0.22);
            
            var vol2 = new DeterministicVolatilityModel();
            vol2.AddVolatilityPoint(0.0, 0.18);
            vol2.AddVolatilityPoint(1.0, 0.28);
            
            Console.WriteLine("Volatilities σ(t):");
            Console.WriteLine($"├─ Stock A: σ(0)={vol1.GetVolatility(0.0):P1} → σ(1)={vol1.GetVolatility(1.0):P1}");
            Console.WriteLine($"└─ Stock B: σ(0)={vol2.GetVolatility(0.0):P1} → σ(1)={vol2.GetVolatility(1.0):P1}");
            Console.WriteLine();
            
            // H2 Basket
            var stocksH2 = new List<StockH2>
            {
                new StockH2("Stock A", 100.0, vol1, 0.02),
                new StockH2("Stock B", 120.0, vol2, 0.015)
            };
            
            double[] weights = { 0.6, 0.4 };
            double[,] correlation = { { 1.0, 0.3 }, { 0.3, 1.0 } };
            
            var basketH2 = new BasketH2(stocksH2, weights, correlation, rateModel);
            var callH2 = new BasketOptionH2(basketH2, OptionType.Call, 110.0, 1.0);
            var putH2 = new BasketOptionH2(basketH2, OptionType.Put, 105.0, 1.0);
            
            Console.WriteLine($"• Initial basket value: {basketH2.GetBasketValue():F2} €");
            Console.WriteLine($"• Call Strike: {callH2.Strike:F2} €");
            Console.WriteLine($"• Put Strike: {putH2.Strike:F2} €");
            Console.WriteLine();
            
            // H2 Pricing
            double callPriceH2 = MomentMatchingPricerH2.Price(callH2);
            double putPriceH2 = MomentMatchingPricerH2.Price(putH2);
            
            Console.WriteLine("H2 Results:");
            Console.WriteLine($"├─ Call Price: {callPriceH2:F4} €");
            Console.WriteLine($"└─ Put Price:  {putPriceH2:F4} €");
            Console.WriteLine();
            
            // Demonstrate H2 → H1 convergence
            DemonstrateConvergence();
            
            // Monte Carlo with variance reduction
            DemonstrateVarianceReduction(callH2);
        }
        
        static void ValidateBlackScholes()
        {
            Console.WriteLine();
            Console.WriteLine("Black-Scholes validation (single asset limit case):");
            
            var singleStock = new Stock("Test", 100.0, 0.20, 0.02);
            var singleBasket = new Basket(new List<Stock> { singleStock }, new double[] { 1.0 }, 
                                        new double[,] { { 1.0 } }, 0.01933);
            var testCall = new BasketOption(singleBasket, OptionType.Call, 100.0, 1.0);
            
            double mmPrice = MomentMatchingPricer.Price(testCall);
            double bsPrice = MathUtils.BlackScholesPrice(100.0, 100.0, 0.01933 - 0.02, 0.20, 1.0, OptionType.Call);
            
            Console.WriteLine($"├─ Moment Matching: {mmPrice:F6} €");
            Console.WriteLine($"├─ Black-Scholes:   {bsPrice:F6} €");
            Console.WriteLine($"└─ Difference: {Math.Abs(mmPrice - bsPrice):E}");
        }
        
        static void DemonstrateConvergence()
        {
            Console.WriteLine("H2 → H1 Convergence (constant parameters):");
            
            // H1
            var stockH1 = new Stock("Test", 100.0, 0.20, 0.02);
            var basketH1 = new Basket(new List<Stock> { stockH1 }, new double[] { 1.0 }, 
                                    new double[,] { { 1.0 } }, 0.01933);
            var optionH1 = new BasketOption(basketH1, OptionType.Call, 105.0, 1.0);
            
            // H2 with equivalent constant parameters (€STR 1.933%)
            var stockH2 = new StockH2("Test", 100.0, 0.20, 0.02);
            var basketH2 = new BasketH2(new List<StockH2> { stockH2 }, new double[] { 1.0 }, 
                                      new double[,] { { 1.0 } }, 0.01933);
            var optionH2 = new BasketOptionH2(basketH2, OptionType.Call, 105.0, 1.0);
            
            double priceH1 = MomentMatchingPricer.Price(optionH1);
            double priceH2 = MomentMatchingPricerH2.Price(optionH2);
            double error = Math.Abs(priceH1 - priceH2) / priceH1 * 100;
            
            Console.WriteLine($"├─ H1 Price: {priceH1:F6} €");
            Console.WriteLine($"├─ H2 Price: {priceH2:F6} €");
            Console.WriteLine($"└─ Relative error: {error:F4}% ✓");
            Console.WriteLine();
        }
        
        static void DemonstrateVarianceReduction(BasketOptionH2 option)
        {
            Console.WriteLine("Monte Carlo with variance reduction:");
            
            var mcPricer = new MonteCarloPricerH2(42);
            var resultStandard = mcPricer.Price(option, 50000, false);
            var resultControlVariate = mcPricer.Price(option, 50000, true);
            
            Console.WriteLine($"├─ Standard MC: {resultStandard.Price:F4} € (σ = {resultStandard.StandardError:F4})");
            Console.WriteLine($"├─ MC with CV:  {resultControlVariate.Price:F4} € (σ = {resultControlVariate.StandardError:F4})");
            Console.WriteLine($"└─ Variance reduction: {resultControlVariate.VarianceReduction:F1}% ✓");
        }
    }
}
