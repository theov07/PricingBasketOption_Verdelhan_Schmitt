using System;

namespace BasketOptionPricer
{
    // Fonctions mathematiques utiles
    public static class MathUtils
    {
        // Fonction de répartition de la loi normale
        public static double NormalCdf(double x)
        {
            return 0.5 * (1.0 + Erf(x / Math.Sqrt(2.0)));
        }
        
        // Fonction d'erreur avec approximation d'Abramowitz et Stegun
        private static double Erf(double x)
        {
            // constantes de l'approximation
            double a1 = 0.254829592;
            double a2 = -0.284496736;
            double a3 = 1.421413741;
            double a4 = -1.453152027;
            double a5 = 1.061405429;
            double p = 0.3275911;
            
            int sign = x < 0 ? -1 : 1;
            x = Math.Abs(x);
            
            double t = 1.0 / (1.0 + p * x);
            double y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-x * x);
            
            return sign * y;
        }
        
        // Formule de Black-Scholes classique
        public static double BlackScholesPrice(double s, double k, double r, double sigma, double t, OptionType optionType)
        {
            // calcul de d1 et d2
            double d1 = (Math.Log(s / k) + (r + 0.5 * sigma * sigma) * t) / (sigma * Math.Sqrt(t));
            double d2 = d1 - sigma * Math.Sqrt(t);
            
            switch (optionType)
            {
                case OptionType.Call:
                    return s * NormalCdf(d1) - k * Math.Exp(-r * t) * NormalCdf(d2);
                case OptionType.Put:
                    return k * Math.Exp(-r * t) * NormalCdf(-d2) - s * NormalCdf(-d1);
                default:
                    throw new ArgumentException("Invalid option type");
            }
        }
        
        // Générateur de nombres aléatoires normaux avec Box-Muller
        public static double GenerateNormalRandom(Random random)
        {
            // Version simplifiée de Box-Muller sans variables static
            double u1 = random.NextDouble();
            double u2 = random.NextDouble();
            
            double z0 = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
            return z0;
        }
        
        /// Décomposition de Cholesky d'une matrice de corrélation
        public static double[,] CholeskyDecomposition(double[,] matrix)
        {
            int n = matrix.GetLength(0);
            double[,] result = new double[n, n];
            
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    if (i == j)
                    {
                        double sum = 0;
                        for (int k = 0; k < j; k++)
                        {
                            sum += result[j, k] * result[j, k];
                        }
                        result[j, j] = Math.Sqrt(matrix[j, j] - sum);
                    }
                    else
                    {
                        double sum = 0;
                        for (int k = 0; k < j; k++)
                        {
                            sum += result[i, k] * result[j, k];
                        }
                        result[i, j] = (matrix[i, j] - sum) / result[j, j];
                    }
                }
            }
            
            return result;
        }
    }
}