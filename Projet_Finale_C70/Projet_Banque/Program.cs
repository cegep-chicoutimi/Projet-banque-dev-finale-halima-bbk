using System;
using System.Collections.Generic;
using System.Linq;
using Projet_Banque.Core.Data;
using Projet_Banque.Core.Models;
using Projet_Banque.Core.Security;

namespace Projet_Banque
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Initialiser des données de base si nécessaire
            InitialiserDonnees();

            var clientService = new ClientConsoleService();
            var banqueService = new BankConsoleService();

            int choix;
            do
            {
                Console.Clear();
                Console.WriteLine("================================");
                Console.WriteLine("      APPLICATION BANCAIRE      ");
                Console.WriteLine("================================");
                Console.WriteLine("1. Menu Client");
                Console.WriteLine("2. Menu Banque (administration)");
                Console.WriteLine("3. Quitter");

                choix = InputHelper.LireEntier("\nVotre choix : ", 1, 3);

                switch (choix)
                {
                    case 1:
                        clientService.MenuPrincipalClient();
                        break;

                    case 2:
                        banqueService.MenuBanque();
                        break;

                    case 3:
                        Console.WriteLine("Fermeture de l'application...");
                        break;
                }

                if (choix != 3)
                {
                    Console.WriteLine("\nAppuyez sur une touche pour revenir au menu principal...");
                    Console.ReadKey();
                }

            } while (choix != 3);
        }

        /// <summary>
        /// Crée des données de base si les fichiers JSON sont vides :
        /// - quelques types de comptes
        /// - un utilisateur admin (login: admin / mdp: admin123)
        /// </summary>
        private static void InitialiserDonnees()
        {
            InitialiserTypesComptes();
            InitialiserAdmin();
        }

        private static void InitialiserTypesComptes()
        {
            var types = JsonStorage.LoadList<TypeCompte>(FilePathManager.TypesComptes);
            if (types.Any())
                return; // déjà initialisé

            types = new List<TypeCompte>
            {
                new TypeCompte { IdTypeDeCompte = 1, Nom = "Epargne", TauxInteret = 2.5 },
                new TypeCompte { IdTypeDeCompte = 2, Nom = "Debit",   TauxInteret = 0.0 },
                new TypeCompte { IdTypeDeCompte = 3, Nom = "Credit",  TauxInteret = 12.0 }
            };

            JsonStorage.SaveList(FilePathManager.TypesComptes, types);

            Console.WriteLine("Types de comptes par défaut créés.");
        }

        private static void InitialiserAdmin()
        {
            var utilisateurs = JsonStorage.LoadList<Utilisateur>(FilePathManager.Utilisateurs);
            if (utilisateurs.Any(u => u.Role == "Admin"))
                return; // il y a déjà un admin

            int newId = utilisateurs.Any() ? utilisateurs.Max(u => u.IdUtilisateur) + 1 : 1;

            var admin = new Utilisateur
            {
                IdUtilisateur = newId,
                Login = "admin",
                MotDePasseHash = PasswordHasher.Hash("admin123"),
                Role = "Admin",
                EstActif = true,
                IdClient = 0
            };

            utilisateurs.Add(admin);
            JsonStorage.SaveList(FilePathManager.Utilisateurs, utilisateurs);

            Console.WriteLine("Utilisateur admin par défaut créé (login: admin / mdp: admin123).");
            Console.WriteLine("Pensez à changer le mot de passe ensuite 😉");
        }
    }
}
