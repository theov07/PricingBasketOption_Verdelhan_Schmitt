using System;
using System.Collections.Generic;

namespace BasketOptionPricer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("═══════════════════════════════════════════════════════════════════");
            Console.WriteLine("    PRICING D'OPTIONS SUR PANIER - VERDELHAN & SCHMITT - M2 272");
            Console.WriteLine("═══════════════════════════════════════════════════════════════════");
            Console.WriteLine();
            Console.WriteLine("Choisissez le mode d'utilisation:");
            Console.WriteLine("1. Démonstration automatique (H1 vs H2)");
            Console.WriteLine("2. Interface interactive (saisie manuelle)");
            Console.WriteLine("3. Quitter");
            Console.WriteLine();
            
            while (true)
            {
                Console.Write("Votre choix (1-3): ");
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
                        Console.WriteLine("Au revoir !");
                        return;
                    default:
                        Console.WriteLine("Choix invalide. Veuillez saisir 1, 2 ou 3.");
                        continue;
                }
                
                // Retour au menu principal
                Console.WriteLine("\nRetour au menu principal...");
                Console.WriteLine("Appuyez sur une touche pour continuer...");
                Console.ReadKey();
                Console.Clear();
                Console.WriteLine("═══════════════════════════════════════════════════════════════════");
                Console.WriteLine("    PRICING D'OPTIONS SUR PANIER - VERDELHAN & SCHMITT - M2 272");
                Console.WriteLine("═══════════════════════════════════════════════════════════════════");
                Console.WriteLine();
                Console.WriteLine("Choisissez le mode d'utilisation:");
                Console.WriteLine("1. Démonstration automatique (H1 vs H2)");
                Console.WriteLine("2. Interface interactive (saisie manuelle)");
                Console.WriteLine("3. Quitter");
                Console.WriteLine();
            }
        }
        
        static void RunDemonstration()
        {
            Console.Clear();
            Console.WriteLine("════════════════════════════════════════════════════════════════════");
            Console.WriteLine("                        DÉMONSTRATION AUTOMATIQUE");
            Console.WriteLine("════════════════════════════════════════════════════════════════════");
            Console.WriteLine("Implémentation: Moment Matching (Brigo et al. - sections 3.2-3.3)");
            Console.WriteLine();
            
            // Démonstration approche H1 vs H2
            DemonstrateH1Approach();
            Console.WriteLine();
            DemonstrateH2Approach();
            
            Console.WriteLine();
            Console.WriteLine("════════════════════════════════════════════════════════════════════");
            Console.WriteLine("                        FIN DE LA DÉMONSTRATION");
            Console.WriteLine("════════════════════════════════════════════════════════════════════");
        }
        
        static void DemonstrateH1Approach()
        {
            Console.WriteLine("🔶 APPROCHE H1 : Paramètres constants (r, σ fixes)");
            Console.WriteLine("─────────────────────────────────────────────────────");
            
            // Configuration du panier
            var stocks = new List<Stock>
            {
                new Stock("Action A", 100.0, 0.20, 0.02),
                new Stock("Action B", 120.0, 0.25, 0.015),
                new Stock("Action C", 80.0, 0.18, 0.025)
            };
            
            double[] weights = { 0.5, 0.3, 0.2 };
            double[,] correlation = new double[,]
            {
                { 1.0, 0.3, 0.2 },
                { 0.3, 1.0, 0.4 },
                { 0.2, 0.4, 1.0 }
            };
            
            var basket = new Basket(stocks, weights, correlation, 0.03);
            double basketValue = basket.GetBasketValue();
            
            var callOption = new BasketOption(basket, OptionType.Call, basketValue * 1.05, 1.0);
            var putOption = new BasketOption(basket, OptionType.Put, basketValue * 0.95, 1.0);
            
            Console.WriteLine($"• Valeur initiale du panier: {basketValue:F2} €");
            Console.WriteLine($"• Strike Call (105%): {callOption.Strike:F2} €");
            Console.WriteLine($"• Strike Put (95%): {putOption.Strike:F2} €");
            Console.WriteLine($"• Maturité: {callOption.Maturity} an");
            Console.WriteLine($"• Taux sans risque: {basket.RiskFreeRate:P1}");
            Console.WriteLine();
            
            // Pricing
            double callPrice = MomentMatchingPricer.Price(callOption);
            double putPrice = MomentMatchingPricer.Price(putOption);
            
            Console.WriteLine("Résultats H1:");
            Console.WriteLine($"├─ Prix Call: {callPrice:F4} €");
            Console.WriteLine($"└─ Prix Put:  {putPrice:F4} €");
            
            // Validation Black-Scholes pour un seul actif
            ValidateBlackScholes();
        }
        
        static void DemonstrateH2Approach()
        {
            Console.WriteLine("🔷 APPROCHE H2 : Paramètres déterministes (r(t), σ(t))");
            Console.WriteLine("──────────────────────────────────────────────────────");
            
            // Courbe de taux déterministe
            var rateModel = new DeterministicRateModel();
            rateModel.AddRatePoint(0.0, 0.025);  // 2.5% à t=0
            rateModel.AddRatePoint(0.5, 0.030);  // 3.0% à t=0.5
            rateModel.AddRatePoint(1.0, 0.035);  // 3.5% à t=1
            
            Console.WriteLine("Courbe de taux r(t):");
            Console.WriteLine($"├─ r(0) = {rateModel.GetRate(0.0):P2}");
            Console.WriteLine($"├─ r(0.5) = {rateModel.GetRate(0.5):P2}");
            Console.WriteLine($"└─ r(1) = {rateModel.GetRate(1.0):P2}");
            Console.WriteLine();
            
            // Volatilités déterministes
            var vol1 = new DeterministicVolatilityModel();
            vol1.AddVolatilityPoint(0.0, 0.20);
            vol1.AddVolatilityPoint(0.5, 0.25);
            vol1.AddVolatilityPoint(1.0, 0.22);
            
            var vol2 = new DeterministicVolatilityModel();
            vol2.AddVolatilityPoint(0.0, 0.18);
            vol2.AddVolatilityPoint(1.0, 0.28);
            
            Console.WriteLine("Volatilités σ(t):");
            Console.WriteLine($"├─ Action A: σ(0)={vol1.GetVolatility(0.0):P1} → σ(1)={vol1.GetVolatility(1.0):P1}");
            Console.WriteLine($"└─ Action B: σ(0)={vol2.GetVolatility(0.0):P1} → σ(1)={vol2.GetVolatility(1.0):P1}");
            Console.WriteLine();
            
            // Panier H2
            var stocksH2 = new List<StockH2>
            {
                new StockH2("Action A", 100.0, vol1, 0.02),
                new StockH2("Action B", 120.0, vol2, 0.015)
            };
            
            double[] weights = { 0.6, 0.4 };
            double[,] correlation = { { 1.0, 0.3 }, { 0.3, 1.0 } };
            
            var basketH2 = new BasketH2(stocksH2, weights, correlation, rateModel);
            var callH2 = new BasketOptionH2(basketH2, OptionType.Call, 110.0, 1.0);
            var putH2 = new BasketOptionH2(basketH2, OptionType.Put, 105.0, 1.0);
            
            Console.WriteLine($"• Valeur initiale du panier: {basketH2.GetBasketValue():F2} €");
            Console.WriteLine($"• Strike Call: {callH2.Strike:F2} €");
            Console.WriteLine($"• Strike Put: {putH2.Strike:F2} €");
            Console.WriteLine();
            
            // Pricing H2
            double callPriceH2 = MomentMatchingPricerH2.Price(callH2);
            double putPriceH2 = MomentMatchingPricerH2.Price(putH2);
            
            Console.WriteLine("Résultats H2:");
            Console.WriteLine($"├─ Prix Call: {callPriceH2:F4} €");
            Console.WriteLine($"└─ Prix Put:  {putPriceH2:F4} €");
            Console.WriteLine();
            
            // Démonstration convergence H2 → H1
            DemonstrateConvergence();
            
            // Monte Carlo avec réduction de variance
            DemonstrateVarianceReduction(callH2);
        }
        
        static void ValidateBlackScholes()
        {
            Console.WriteLine();
            Console.WriteLine("Validation Black-Scholes (cas limite 1 actif):");
            
            var singleStock = new Stock("Test", 100.0, 0.20, 0.02);
            var singleBasket = new Basket(new List<Stock> { singleStock }, new double[] { 1.0 }, 
                                        new double[,] { { 1.0 } }, 0.03);
            var testCall = new BasketOption(singleBasket, OptionType.Call, 100.0, 1.0);
            
            double mmPrice = MomentMatchingPricer.Price(testCall);
            double bsPrice = MathUtils.BlackScholesPrice(100.0, 100.0, 0.03 - 0.02, 0.20, 1.0, OptionType.Call);
            
            Console.WriteLine($"├─ Moment Matching: {mmPrice:F6} €");
            Console.WriteLine($"├─ Black-Scholes:   {bsPrice:F6} €");
            Console.WriteLine($"└─ Différence: {Math.Abs(mmPrice - bsPrice):E}");
        }
        
        static void DemonstrateConvergence()
        {
            Console.WriteLine("Convergence H2 → H1 (paramètres constants):");
            
            // H1
            var stockH1 = new Stock("Test", 100.0, 0.20, 0.02);
            var basketH1 = new Basket(new List<Stock> { stockH1 }, new double[] { 1.0 }, 
                                    new double[,] { { 1.0 } }, 0.03);
            var optionH1 = new BasketOption(basketH1, OptionType.Call, 105.0, 1.0);
            
            // H2 avec paramètres équivalents constants
            var stockH2 = new StockH2("Test", 100.0, 0.20, 0.02);
            var basketH2 = new BasketH2(new List<StockH2> { stockH2 }, new double[] { 1.0 }, 
                                      new double[,] { { 1.0 } }, 0.03);
            var optionH2 = new BasketOptionH2(basketH2, OptionType.Call, 105.0, 1.0);
            
            double priceH1 = MomentMatchingPricer.Price(optionH1);
            double priceH2 = MomentMatchingPricerH2.Price(optionH2);
            double error = Math.Abs(priceH1 - priceH2) / priceH1 * 100;
            
            Console.WriteLine($"├─ Prix H1: {priceH1:F6} €");
            Console.WriteLine($"├─ Prix H2: {priceH2:F6} €");
            Console.WriteLine($"└─ Erreur relative: {error:F4}% ✓");
            Console.WriteLine();
        }
        
        static void DemonstrateVarianceReduction(BasketOptionH2 option)
        {
            Console.WriteLine("Monte Carlo avec réduction de variance:");
            
            var mcPricer = new MonteCarloPricerH2(42);
            var resultStandard = mcPricer.Price(option, 50000, false);
            var resultControlVariate = mcPricer.Price(option, 50000, true);
            
            Console.WriteLine($"├─ MC standard: {resultStandard.Price:F4} € (σ = {resultStandard.StandardError:F4})");
            Console.WriteLine($"├─ MC avec CV:  {resultControlVariate.Price:F4} € (σ = {resultControlVariate.StandardError:F4})");
            Console.WriteLine($"└─ Réduction variance: {resultControlVariate.VarianceReduction:F1}% ✓");
        }
    }
}
