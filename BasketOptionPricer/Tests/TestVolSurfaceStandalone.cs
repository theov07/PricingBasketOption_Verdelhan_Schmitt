using System;
using BasketOptionPricer.Data;

namespace BasketOptionPricer
{
    class TestVolSurfaceStandalone
    {
        public static void Run()
        {
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
            }

            Console.WriteLine("\n════════════════════════════════════════════════════════════════════");
        }
    }
}
