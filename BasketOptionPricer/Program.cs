using System;
using System.Collections.Generic;

namespace BasketOptionPricer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Pricer d'Options sur Panier - Approche H1");
            Console.WriteLine("=========================================");
            Console.WriteLine("Implémentation: Moment Matching (Brigo et al.)");
            Console.WriteLine();
            
            // Exemple 1: Panier simple 2 actifs
            RunExample1();
            
            // Exemple 2: Panier diversifié 3 actifs
            RunExample2();
            
            // Validation: Convergence vers Black-Scholes (1 actif)
            RunValidationTest();
        }
        
        static void RunExample1()
        {
            Console.WriteLine("=== Exemple 1: Panier 2 actifs ===");
            
            var stocks = new List<Stock>
            {
                new Stock("Action A", 100.0, 0.20, 0.02),
                new Stock("Action B", 120.0, 0.25, 0.015)
            };
            
            double[] weights = { 0.6, 0.4 };
            double[,] correlation = { { 1.0, 0.3 }, { 0.3, 1.0 } };
            double riskFreeRate = 0.03;
            double maturity = 1.0;
            
            var basket = new Basket(stocks, weights, correlation, riskFreeRate);
            double strike = basket.GetBasketValue();
            
            var callOption = new BasketOption(basket, OptionType.Call, strike, maturity);
            var putOption = new BasketOption(basket, OptionType.Put, strike, maturity);
            
            double callPrice = MomentMatchingPricer.Price(callOption);
            double putPrice = MomentMatchingPricer.Price(putOption);
            
            Console.WriteLine($"Valeur initiale panier: {strike:F2}");
            Console.WriteLine($"Prix Call ATM: {callPrice:F4}");
            Console.WriteLine($"Prix Put ATM: {putPrice:F4}");
            
            // Vérification parité Put-Call
            double M1 = 0;
            for (int i = 0; i < stocks.Count; i++)
            {
                M1 += weights[i] * stocks[i].SpotPrice * Math.Exp((riskFreeRate - stocks[i].DividendRate) * maturity);
            }
            double putCallParity = callPrice - putPrice - M1 * Math.Exp(-riskFreeRate * maturity) + strike * Math.Exp(-riskFreeRate * maturity);
            Console.WriteLine($"Vérification parité Put-Call: {Math.Abs(putCallParity):E}");
            Console.WriteLine();
        }
        
        static void RunExample2()
        {
            Console.WriteLine("=== Exemple 2: Panier diversifié 3 actifs ===");
            
            var stocks = new List<Stock>
            {
                new Stock("Tech", 150.0, 0.30, 0.01),
                new Stock("Finance", 80.0, 0.20, 0.03),
                new Stock("Energie", 120.0, 0.35, 0.02)
            };
            
            double[] weights = { 0.5, 0.3, 0.2 };
            double[,] correlation = new double[,]
            {
                { 1.0, 0.4, 0.2 },
                { 0.4, 1.0, 0.3 },
                { 0.2, 0.3, 1.0 }
            };
            
            var basket = new Basket(stocks, weights, correlation, 0.04);
            var callOption = new BasketOption(basket, OptionType.Call, basket.GetBasketValue(), 1.5);
            
            double mmPrice = MomentMatchingPricer.Price(callOption);
            
            // Comparaison Monte Carlo
            var mcPricer = new MonteCarloPricer(12345);
            var mcResult = mcPricer.Price(callOption, 100000);
            
            Console.WriteLine($"Prix Moment Matching: {mmPrice:F4}");
            Console.WriteLine($"Prix Monte Carlo: {mcResult.Price:F4} (±{mcResult.StandardError:F4})");
            Console.WriteLine($"Écart relatif: {Math.Abs(mmPrice - mcResult.Price) / mmPrice * 100:F2}%");
            Console.WriteLine();
        }
        
        static void RunValidationTest()
        {
            Console.WriteLine("=== Validation: Convergence vers Black-Scholes ===");
            
            // Test avec un seul actif (doit donner exactement Black-Scholes)
            var singleStock = new Stock("Single", 100.0, 0.20, 0.02);
            var singleBasket = new Basket(new List<Stock> { singleStock }, new double[] { 1.0 }, 
                                        new double[,] { { 1.0 } }, 0.03);
            var singleCall = new BasketOption(singleBasket, OptionType.Call, 100.0, 1.0);
            
            double mmPrice = MomentMatchingPricer.Price(singleCall);
            double bsPrice = CalculateBlackScholesPrice(100.0, 100.0, 0.03, 0.02, 0.20, 1.0, true);
            
            Console.WriteLine($"Prix Moment Matching: {mmPrice:F8}");
            Console.WriteLine($"Prix Black-Scholes: {bsPrice:F8}");
            Console.WriteLine($"Différence absolue: {Math.Abs(mmPrice - bsPrice):E}");
            
            if (Math.Abs(mmPrice - bsPrice) < 1e-10)
            {
                Console.WriteLine("✓ Validation réussie: Convergence parfaite");
            }
            else
            {
                Console.WriteLine("✗ Problème de convergence détecté");
            }
        }
        
        static double CalculateBlackScholesPrice(double s, double k, double r, double q, double vol, double t, bool isCall)
        {
            double d1 = (Math.Log(s / k) + (r - q + 0.5 * vol * vol) * t) / (vol * Math.Sqrt(t));
            double d2 = d1 - vol * Math.Sqrt(t);
            
            if (isCall)
                return s * Math.Exp(-q * t) * MathUtils.NormalCdf(d1) - k * Math.Exp(-r * t) * MathUtils.NormalCdf(d2);
            else
                return k * Math.Exp(-r * t) * MathUtils.NormalCdf(-d2) - s * Math.Exp(-q * t) * MathUtils.NormalCdf(-d1);
        }
    }
}
