using System;
using System.Globalization;

namespace Projet_Banque
{
    internal static class InputHelper
    {
        public static int LireEntier(string message, int? min = null, int? max = null)
        {
            while (true)
            {
                Console.Write(message);
                var saisie = Console.ReadLine();

                if (!int.TryParse(saisie, out int valeur))
                {
                    Console.WriteLine("⚠ Veuillez entrer un nombre entier valide.");
                    continue;
                }

                if (min.HasValue && valeur < min.Value)
                {
                    Console.WriteLine($"⚠ La valeur doit être ≥ {min.Value}.");
                    continue;
                }

                if (max.HasValue && valeur > max.Value)
                {
                    Console.WriteLine($"⚠ La valeur doit être ≤ {max.Value}.");
                    continue;
                }

                return valeur;
            }
        }

        public static double LireDouble(string message, double? min = null, double? max = null)
        {
            while (true)
            {
                Console.Write(message);
                var saisie = Console.ReadLine();

                // Pour gérer les virgules et points
                if (!double.TryParse(saisie, NumberStyles.Any, CultureInfo.InvariantCulture, out double valeur))
                {
                    Console.WriteLine("⚠ Veuillez entrer un nombre valide (ex: 100 ou 100.50).");
                    continue;
                }

                if (min.HasValue && valeur < min.Value)
                {
                    Console.WriteLine($"⚠ La valeur doit être ≥ {min.Value}.");
                    continue;
                }

                if (max.HasValue && valeur > max.Value)
                {
                    Console.WriteLine($"⚠ La valeur doit être ≤ {max.Value}.");
                    continue;
                }

                return valeur;
            }
        }

        public static string LireTexteObligatoire(string message)
        {
            while (true)
            {
                Console.Write(message);
                var saisie = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(saisie))
                {
                    Console.WriteLine("⚠ Ce champ est obligatoire.");
                    continue;
                }

                return saisie.Trim();
            }
        }
    }
}
