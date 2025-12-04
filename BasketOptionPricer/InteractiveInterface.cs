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
            Console.WriteLine("           PRICER INTERACTIF D'OPTIONS SUR PANIER");
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine();

            try
            {
                // 1. Saisie de la composition du panier
                var basketData = GetBasketComposition();
                
                // 2. Saisie des paramètres financiers
                var financialParams = GetFinancialParameters(basketData.stocks.Count);
                
                // 3. Saisie des paramètres de l'option
                var optionParams = GetOptionParameters(basketData);
                
                // 4. Choix de la méthode de valorisation
                var pricingMethod = ChoosePricingMethod();
                
                // 5. Calcul et affichage des résultats
                DisplayResults(basketData, financialParams, optionParams, pricingMethod);
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Erreur: {ex.Message}");
                Console.WriteLine("\nAppuyez sur une touche pour continuer...");
                Console.ReadKey();
            }
        }

        private static (List<Stock> stocks, double[] weights) GetBasketComposition()
        {
            Console.WriteLine("📊 COMPOSITION DU PANIER");
            Console.WriteLine("─────────────────────────");
            
            int numStocks = GetIntInput("Nombre d'actifs dans le panier (2-5): ", 2, 5);
            
            var stocks = new List<Stock>();
            var weights = new double[numStocks];
            
            for (int i = 0; i < numStocks; i++)
            {
                Console.WriteLine($"\n• Actif {i + 1}:");
                
                string name = GetStringInput($"  Nom: ");
                double spotPrice = GetDoubleInput("  Prix spot (€): ", 1.0, 1000.0);
                double volatility = GetDoubleInput("  Volatilité (ex: 0.20 pour 20%): ", 0.01, 2.0);
                double dividendRate = GetDoubleInput("  Taux dividende (ex: 0.02 pour 2%): ", 0.0, 0.1);
                double weight = GetDoubleInput("  Poids dans le panier (ex: 0.3 pour 30%): ", 0.01, 1.0);
                
                stocks.Add(new Stock(name, spotPrice, volatility, dividendRate));
                weights[i] = weight;
            }
            
            // Normalisation des poids
            double totalWeight = 0;
            for (int i = 0; i < weights.Length; i++) totalWeight += weights[i];
            
            if (Math.Abs(totalWeight - 1.0) > 0.01)
            {
                Console.WriteLine($"\n⚠️  Normalisation des poids (somme = {totalWeight:F3})");
                for (int i = 0; i < weights.Length; i++) 
                    weights[i] /= totalWeight;
            }
            
            return (stocks, weights);
        }

        private static (double[,] correlation, double riskFreeRate) GetFinancialParameters(int numStocks)
        {
            Console.WriteLine("\n💰 PARAMÈTRES FINANCIERS");
            Console.WriteLine("─────────────────────────");
            
            double riskFreeRate = GetDoubleInput("Taux sans risque (ex: 0.03 pour 3%): ", 0.0, 0.2);
            
            var correlation = new double[numStocks, numStocks];
            
            // Diagonale = 1
            for (int i = 0; i < numStocks; i++)
                correlation[i, i] = 1.0;
            
            // Saisie des corrélations (matrice symétrique)
            if (numStocks > 1)
            {
                Console.WriteLine("\nCorrélations entre actifs:");
                for (int i = 0; i < numStocks; i++)
                {
                    for (int j = i + 1; j < numStocks; j++)
                    {
                        double corr = GetDoubleInput($"  Corrélation Actif {i+1} - Actif {j+1} (-1 à 1): ", -0.99, 0.99);
                        correlation[i, j] = corr;
                        correlation[j, i] = corr;
                    }
                }
            }
            
            return (correlation, riskFreeRate);
        }

        private static (OptionType type, double strike, double maturity) GetOptionParameters((List<Stock> stocks, double[] weights) basketData)
        {
            Console.WriteLine("\n📋 PARAMÈTRES DE L'OPTION");
            Console.WriteLine("──────────────────────────");
            
            // Calcul de la valeur du panier
            double basketValue = 0;
            for (int i = 0; i < basketData.stocks.Count; i++)
                basketValue += basketData.weights[i] * basketData.stocks[i].SpotPrice;
            
            Console.WriteLine($"Valeur actuelle du panier: {basketValue:F2} €");
            
            // Type d'option
            Console.WriteLine("\nType d'option:");
            Console.WriteLine("1. Call");
            Console.WriteLine("2. Put");
            int choice = GetIntInput("Choix (1-2): ", 1, 2);
            OptionType optionType = (choice == 1) ? OptionType.Call : OptionType.Put;
            
            // Strike
            double defaultStrike = basketValue;
            double strike = GetDoubleInput($"Strike (défaut {defaultStrike:F2}€): ", basketValue * 0.5, basketValue * 2.0, defaultStrike);
            
            // Maturité
            double maturity = GetDoubleInput("Maturité en années (ex: 1.0): ", 0.1, 10.0);
            
            return (optionType, strike, maturity);
        }

        private static string ChoosePricingMethod()
        {
            Console.WriteLine("\n🔧 MÉTHODE DE VALORISATION");
            Console.WriteLine("───────────────────────────");
            Console.WriteLine("1. Moment Matching (Brigo et al.)");
            Console.WriteLine("2. Monte Carlo");
            
            int choice = GetIntInput("Choix (1-2): ", 1, 2);
            return (choice == 1) ? "MomentMatching" : "MonteCarlo";
        }

        private static void DisplayResults((List<Stock> stocks, double[] weights) basketData,
            (double[,] correlation, double riskFreeRate) financialParams,
            (OptionType type, double strike, double maturity) optionParams,
            string pricingMethod)
        {
            Console.WriteLine("\n🎯 RÉSULTATS");
            Console.WriteLine("═════════════");
            
            // Création du panier
            var basket = new Basket(basketData.stocks, basketData.weights, 
                financialParams.correlation, financialParams.riskFreeRate);
            
            var option = new BasketOption(basket, optionParams.type, optionParams.strike, optionParams.maturity);
            
            // Affichage du résumé
            Console.WriteLine($"\nRésumé de l'option:");
            Console.WriteLine($"├─ Type: {optionParams.type}");
            Console.WriteLine($"├─ Strike: {optionParams.strike:F2} €");
            Console.WriteLine($"├─ Maturité: {optionParams.maturity:F2} ans");
            Console.WriteLine($"├─ Valeur panier: {basket.GetBasketValue():F2} €");
            Console.WriteLine($"└─ Méthode: {pricingMethod}");
            Console.WriteLine();
            
            // Calcul du prix
            Console.WriteLine("Calcul en cours...");
            
            if (pricingMethod == "MomentMatching")
            {
                double price = MomentMatchingPricer.Price(option);
                Console.WriteLine($"\n💰 Prix de l'option: {price:F4} €");
            }
            else // Monte Carlo
            {
                int simulations = GetIntInput("Nombre de simulations (10000-1000000): ", 10000, 1000000, 100000);
                
                var mcPricer = new MonteCarloPricerH2(42);
                
                // Conversion vers H2 pour utiliser le MC amélioré
                var stocksH2 = new List<StockH2>();
                foreach (var stock in basketData.stocks)
                    stocksH2.Add(new StockH2(stock.Name, stock.SpotPrice, stock.Volatility, stock.DividendRate));
                
                var basketH2 = new BasketH2(stocksH2, basketData.weights, financialParams.correlation, financialParams.riskFreeRate);
                var optionH2 = new BasketOptionH2(basketH2, optionParams.type, optionParams.strike, optionParams.maturity);
                
                var result = mcPricer.Price(optionH2, simulations, false);
                
                Console.WriteLine($"\n💰 Prix de l'option: {result.Price:F4} €");
                Console.WriteLine($"📊 Écart-type: ±{result.StandardError:F4} €");
                Console.WriteLine($"📈 Variance estimateur: {result.Variance:F6}");
                Console.WriteLine($"🎯 Intervalle confiance 95%: [{result.Price - 1.96*result.StandardError:F4}, {result.Price + 1.96*result.StandardError:F4}] €");
            }
            
            Console.WriteLine("\n" + new string('═', 50));
            Console.WriteLine("Appuyez sur une touche pour continuer...");
            Console.ReadKey();
        }

        // Méthodes utilitaires de saisie
        private static string GetStringInput(string prompt)
        {
            Console.Write(prompt);
            string input = Console.ReadLine()?.Trim();
            while (string.IsNullOrEmpty(input))
            {
                Console.Write("Veuillez saisir une valeur: ");
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
                
                Console.WriteLine($"Veuillez saisir un entier entre {min} et {max}.");
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
                
                Console.WriteLine($"Veuillez saisir un nombre entre {min:F2} et {max:F2}.");
            }
        }
    }
}