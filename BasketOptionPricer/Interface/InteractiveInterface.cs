using System;
using System.Collections.Generic;
using System.Globalization;

namespace BasketOptionPricer
{
    public static class InteractiveInterface
    {
        public static void RunInteractiveMode()
        {
            Console.Clear();
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine("           INTERACTIVE BASKET OPTION PRICER");
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine();

            try
            {
                // 1. Input basket composition
                var basketData = GetBasketComposition();
                
                // 2. Input financial parameters
                var financialParams = GetFinancialParameters(basketData.stocks.Count);
                
                // 3. Input option parameters
                var optionParams = GetOptionParameters(basketData);
                
                // 4. Choose pricing method
                var pricingMethod = ChoosePricingMethod();
                
                // 5. Calculate and display results
                DisplayResults(basketData, financialParams, optionParams, pricingMethod);
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Error: {ex.Message}");
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
            }
        }

        private static (List<Stock> stocks, double[] weights) GetBasketComposition()
        {
            Console.WriteLine("📊 BASKET COMPOSITION");
            Console.WriteLine("─────────────────────────");
            
            int numStocks = GetIntInput("Number of assets in basket (2-5): ", 2, 5);
            
            var stocks = new List<Stock>();
            var weights = new double[numStocks];
            
            for (int i = 0; i < numStocks; i++)
            {
                Console.WriteLine($"\n• Asset {i + 1}:");
                
                string name = GetStringInput($"  Name: ");
                double spotPrice = GetDoubleInput("  Spot price (€): ", 1.0, 1000.0);
                double volatility = GetDoubleInput("  Volatility (e.g., 0.20 for 20%): ", 0.01, 2.0);
                double dividendRate = GetDoubleInput("  Dividend rate (e.g., 0.02 for 2%): ", 0.0, 0.1);
                double weight = GetDoubleInput("  Weight in basket (e.g., 0.3 for 30%): ", 0.01, 1.0);
                
                stocks.Add(new Stock(name, spotPrice, volatility, dividendRate));
                weights[i] = weight;
            }
            
            // Weight normalization
            double totalWeight = 0;
            for (int i = 0; i < weights.Length; i++) totalWeight += weights[i];
            
            if (Math.Abs(totalWeight - 1.0) > 0.01)
            {
                Console.WriteLine($"\n⚠️  Normalizing weights (sum = {totalWeight:F3})");
                for (int i = 0; i < weights.Length; i++) 
                    weights[i] /= totalWeight;
            }
            
            return (stocks, weights);
        }

        private static (double[,] correlation, double riskFreeRate) GetFinancialParameters(int numStocks)
        {
            Console.WriteLine("\n💰 FINANCIAL PARAMETERS");
            Console.WriteLine("─────────────────────────");
            
            double riskFreeRate = GetDoubleInput("Risk-free rate (e.g., 0.03 for 3%): ", 0.0, 0.2);
            
            var correlation = new double[numStocks, numStocks];
            
            // Diagonal = 1
            for (int i = 0; i < numStocks; i++)
                correlation[i, i] = 1.0;
            
            // Input correlations (symmetric matrix)
            if (numStocks > 1)
            {
                Console.WriteLine("\nCorrelations between assets:");
                for (int i = 0; i < numStocks; i++)
                {
                    for (int j = i + 1; j < numStocks; j++)
                    {
                        double corr = GetDoubleInput($"  Correlation Asset {i+1} - Asset {j+1} (-1 to 1): ", -0.99, 0.99);
                        correlation[i, j] = corr;
                        correlation[j, i] = corr;
                    }
                }
            }
            
            return (correlation, riskFreeRate);
        }

        private static (OptionType type, double strike, double maturity) GetOptionParameters((List<Stock> stocks, double[] weights) basketData)
        {
            Console.WriteLine("\n📋 OPTION PARAMETERS");
            Console.WriteLine("──────────────────────────");
            
            // Calculate basket value
            double basketValue = 0;
            for (int i = 0; i < basketData.stocks.Count; i++)
                basketValue += basketData.weights[i] * basketData.stocks[i].SpotPrice;
            
            Console.WriteLine($"Current basket value: {basketValue:F2} €");
            
            // Option type
            Console.WriteLine("\nOption type:");
            Console.WriteLine("1. Call");
            Console.WriteLine("2. Put");
            int choice = GetIntInput("Choice (1-2): ", 1, 2);
            OptionType optionType = (choice == 1) ? OptionType.Call : OptionType.Put;
            
            // Strike
            double defaultStrike = basketValue;
            double strike = GetDoubleInput($"Strike (default {defaultStrike:F2}€): ", basketValue * 0.5, basketValue * 2.0, defaultStrike);
            
            // Maturity
            double maturity = GetDoubleInput("Maturity in years (e.g., 1.0): ", 0.1, 10.0);
            
            return (optionType, strike, maturity);
        }

        private static string ChoosePricingMethod()
        {
            Console.WriteLine("\n🔧 PRICING METHOD");
            Console.WriteLine("───────────────────────────");
            Console.WriteLine("1. Moment Matching (Brigo et al.)");
            Console.WriteLine("2. Monte Carlo");
            
            int choice = GetIntInput("Choice (1-2): ", 1, 2);
            return (choice == 1) ? "MomentMatching" : "MonteCarlo";
        }

        private static void DisplayResults((List<Stock> stocks, double[] weights) basketData,
            (double[,] correlation, double riskFreeRate) financialParams,
            (OptionType type, double strike, double maturity) optionParams,
            string pricingMethod)
        {
            Console.WriteLine("\n🎯 RESULTS");
            Console.WriteLine("═════════════");
            
            // Create basket
            var basket = new Basket(basketData.stocks, basketData.weights, 
                financialParams.correlation, financialParams.riskFreeRate);
            
            var option = new BasketOption(basket, optionParams.type, optionParams.strike, optionParams.maturity);
            
            // Display summary
            Console.WriteLine($"\nOption summary:");
            Console.WriteLine($"├─ Type: {optionParams.type}");
            Console.WriteLine($"├─ Strike: {optionParams.strike:F2} €");
            Console.WriteLine($"├─ Maturity: {optionParams.maturity:F2} years");
            Console.WriteLine($"├─ Basket value: {basket.GetBasketValue():F2} €");
            Console.WriteLine($"└─ Method: {pricingMethod}");
            Console.WriteLine();
            
            // Calculate price
            Console.WriteLine("Calculating...");
            
            if (pricingMethod == "MomentMatching")
            {
                double price = MomentMatchingPricer.Price(option);
                Console.WriteLine($"\n💰 Option price: {price:F4} €");
            }
            else // Monte Carlo
            {
                int simulations = GetIntInput("Number of simulations (10000-1000000): ", 10000, 1000000, 100000);
                
                var mcPricer = new MonteCarloPricerH2(42);
                
                // Convert to H2 to use improved MC
                var stocksH2 = new List<StockH2>();
                foreach (var stock in basketData.stocks)
                    stocksH2.Add(new StockH2(stock.Name, stock.SpotPrice, stock.Volatility, stock.DividendRate));
                
                var basketH2 = new BasketH2(stocksH2, basketData.weights, financialParams.correlation, financialParams.riskFreeRate);
                var optionH2 = new BasketOptionH2(basketH2, optionParams.type, optionParams.strike, optionParams.maturity);
                
                var result = mcPricer.Price(optionH2, simulations, false);
                
                Console.WriteLine($"\n💰 Option price: {result.Price:F4} €");
                Console.WriteLine($"📊 Standard error: ±{result.StandardError:F4} €");
                Console.WriteLine($"📈 Estimator variance: {result.Variance:F6}");
                Console.WriteLine($"🎯 95% confidence interval: [{result.Price - 1.96*result.StandardError:F4}, {result.Price + 1.96*result.StandardError:F4}] €");
            }
            
            Console.WriteLine("\n" + new string('═', 50));
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        // Input utility methods
        private static string GetStringInput(string prompt)
        {
            Console.Write(prompt);
            string input = Console.ReadLine()?.Trim();
            while (string.IsNullOrEmpty(input))
            {
                Console.Write("Please enter a value: ");
                input = Console.ReadLine()?.Trim();
            }
            return input;
        }

        private static int GetIntInput(string prompt, int min, int max, int defaultValue = 0)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine()?.Trim();
                
                if (string.IsNullOrEmpty(input) && defaultValue != 0)
                    return defaultValue;
                
                if (int.TryParse(input, out int value) && value >= min && value <= max)
                    return value;
                
                Console.WriteLine($"Please enter an integer between {min} and {max}.");
            }
        }

        private static double GetDoubleInput(string prompt, double min, double max, double defaultValue = 0)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine()?.Trim();
                
                if (string.IsNullOrEmpty(input) && defaultValue != 0)
                    return defaultValue;
                
                if (double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out double value) && 
                    value >= min && value <= max)
                    return value;
                
                Console.WriteLine($"Please enter a number between {min:F2} and {max:F2}.");
            }
        }
    }
}