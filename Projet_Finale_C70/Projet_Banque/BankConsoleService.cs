using System;
using System.Collections.Generic;
using System.Linq;
using Projet_Banque.Core.Data;
using Projet_Banque.Core.Models;
using Projet_Banque.Core.Security;

namespace Projet_Banque
{
    public class BankConsoleService
    {
        // Chemins des fichiers JSON
        private readonly string FichierClients;
        private readonly string FichierComptes;
        private readonly string FichierTypesComptes;
        private readonly string FichierOperations;
        private readonly string FichierUtilisateurs;
        private readonly string FichierDemandes;
        private readonly string FichierAlertes;

        // Services de sécurité
        private readonly AuthService _authService;
        private Utilisateur? _adminCourant;

        // =========================
        //  CONSTRUCTEUR
        // =========================
        public BankConsoleService()
        {
            FichierClients = FilePathManager.Clients;
            FichierComptes = FilePathManager.Comptes;
            FichierTypesComptes = FilePathManager.TypesComptes;
            FichierOperations = FilePathManager.Operations;
            FichierUtilisateurs = FilePathManager.Utilisateurs;
            FichierDemandes = FilePathManager.Demandes;
            FichierAlertes = FilePathManager.Alertes;

            _authService = new AuthService(FichierUtilisateurs);
        }

        // =========================
        //  MENU PRINCIPAL BANQUE
        // =========================
        public void MenuBanque()
        {
            Console.Clear();
            Console.WriteLine("===== AUTHENTIFICATION BANQUE =====");

            Console.Write("Login : ");
            string? login = Console.ReadLine();

            Console.Write("Mot de passe : ");
            string? mdp = Console.ReadLine();

            _adminCourant = _authService.Authentifier(login ?? "", mdp ?? "", "Admin")
                             ?? _authService.Authentifier(login ?? "", mdp ?? "", "Employe");

            if (_adminCourant == null)
            {
                Console.WriteLine("Échec d'authentification.");
                return;
            }

            Console.WriteLine($"\nBienvenue {_adminCourant.Login} !");
            AuditLogger.Log(_adminCourant, "Connexion banque", "Connexion au module banque.");

            int choix;
            do
            {
                Console.WriteLine("\n========= MENU BANQUE =========");
                Console.WriteLine("1. Valider les demandes de création de compte");
                Console.WriteLine("2. Gérer les comptes utilisateurs");
                Console.WriteLine("3. Gérer les types de comptes");
                Console.WriteLine("4. Calculer les intérêts mensuels");
                Console.WriteLine("5. Vérifier les soldes minimum et générer des alertes");
                Console.WriteLine("6. Quitter le module banque");

                choix = InputHelper.LireEntier("Votre choix : ", 1, 6);

                switch (choix)
                {
                    case 1:
                        ValiderDemandesCreationCompte();
                        break;
                    case 2:
                        GererUtilisateurs();
                        break;
                    case 3:
                        GererTypesComptes();
                        break;
                    case 4:
                        CalculerInteretsMensuels();
                        break;
                    case 5:
                        GenererAlertesSoldeMinimum();
                        break;
                    case 6:
                        AuditLogger.Log(_adminCourant, "Deconnexion banque", "Déconnexion du module banque.");
                        Console.WriteLine("Déconnexion...");
                        break;
                }

            } while (choix != 6);
        }

        // ==========================================
        // 1) CRÉER DES COMPTES : VALIDER DEMANDES
        // ==========================================
        private void ValiderDemandesCreationCompte()
        {
            var demandes = JsonStorage.LoadList<DemandeCompte>(FichierDemandes);
            var demandesEnAttente = demandes.Where(d => d.Statut == "EnAttente").ToList();

            if (!demandesEnAttente.Any())
            {
                Console.WriteLine("Aucune demande en attente.");
                return;
            }

            Console.WriteLine("\n=== Demandes en attente ===");
            for (int i = 0; i < demandesEnAttente.Count; i++)
            {
                var d = demandesEnAttente[i];
                Console.WriteLine($"{i + 1}. Demande #{d.IdDemande} - {d.Client.Nom} {d.Client.Prenom} - TypeCompteId: {d.IdTypeDeCompte}");
            }

            int choixDemande = InputHelper.LireEntier("Sélectionnez une demande : ", 1, demandesEnAttente.Count);
            var demande = demandesEnAttente[choixDemande - 1];

            Console.WriteLine("1 = Approuver, 2 = Refuser");
            int decision = InputHelper.LireEntier("Votre choix : ", 1, 2);

            if (decision == 2)
            {
                demande.Statut = "Refusee";
                JsonStorage.SaveList(FichierDemandes, demandes);
                AuditLogger.Log(_adminCourant, "Demande refusée", $"Demande #{demande.IdDemande} refusée.");
                Console.WriteLine("Demande refusée.");
                return;
            }

            // APPROBATION : création Client + Compte
            var clients = JsonStorage.LoadList<Client>(FichierClients);
            var comptes = JsonStorage.LoadList<Compte>(FichierComptes);

            int nouvelIdClient = clients.Any() ? clients.Max(c => c.IdClient) + 1 : 1;
            demande.Client.IdClient = nouvelIdClient;
            clients.Add(demande.Client);

            string nouvelIban = "CPT-" + (comptes.Count + 1).ToString("D5");

            var nouveauCompte = new Compte
            {
                Iban = nouvelIban,
                Solde = 0,
                SoldeMinimum = demande.SoldeMinimum,
                DateOuverture = DateTime.Now,
                IdClient = nouvelIdClient,
                IdTypeDeCompte = demande.IdTypeDeCompte
            };
            comptes.Add(nouveauCompte);

            demande.Statut = "Approuvee";

            JsonStorage.SaveList(FichierClients, clients);
            JsonStorage.SaveList(FichierComptes, comptes);
            JsonStorage.SaveList(FichierDemandes, demandes);

            AuditLogger.Log(_adminCourant, "Demande approuvée",
                $"Demande #{demande.IdDemande} approuvée. Compte {nouveauCompte.Iban} créé pour client {nouvelIdClient}.");

            Console.WriteLine($"Demande approuvée. Compte créé avec IBAN : {nouveauCompte.Iban}");
        }

        // ======================================
        // 2) GÉRER LES COMPTES UTILISATEURS
        // ======================================
        private void GererUtilisateurs()
        {
            int choix;
            do
            {
                Console.WriteLine("\n=== Gestion des utilisateurs ===");
                Console.WriteLine("1. Créer un utilisateur");
                Console.WriteLine("2. Désactiver un utilisateur");
                Console.WriteLine("3. Réinitialiser le mot de passe");
                Console.WriteLine("4. Retour");

                choix = InputHelper.LireEntier("Votre choix : ", 1, 4);

                switch (choix)
                {
                    case 1:
                        CreerUtilisateur();
                        break;
                    case 2:
                        DesactiverUtilisateur();
                        break;
                    case 3:
                        ReinitialiserMotDePasse();
                        break;
                }

            } while (choix != 4);
        }

        private void CreerUtilisateur()
        {
            var utilisateurs = JsonStorage.LoadList<Utilisateur>(FichierUtilisateurs);
            var clients = JsonStorage.LoadList<Client>(FichierClients);

            Console.WriteLine("\nAssocier l'utilisateur à un client :");
            Console.WriteLine("0. Aucun (employé / admin sans client)");
            for (int i = 0; i < clients.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {clients[i].IdClient} - {clients[i].Nom} {clients[i].Prenom}");
            }

            int choixClient = InputHelper.LireEntier("Choisissez un client : ", 0, clients.Count);

            int idClient = 0;
            if (choixClient > 0)
                idClient = clients[choixClient - 1].IdClient;

            Console.Write("Login : ");
            string login = Console.ReadLine() ?? "";

            login = login.Trim();
            if (string.IsNullOrWhiteSpace(login))
            {
                Console.WriteLine("Login obligatoire, création annulée.");
                return;
            }

            Console.Write("Mot de passe : ");
            string mdp = Console.ReadLine() ?? "";

            if (string.IsNullOrWhiteSpace(mdp))
            {
                Console.WriteLine("Mot de passe obligatoire, création annulée.");
                return;
            }

            Console.Write("Rôle (Client / Admin / Employe) : ");
            string role = (Console.ReadLine() ?? "Client").Trim();

            if (string.IsNullOrWhiteSpace(role))
                role = "Client";

            int newId = utilisateurs.Any() ? utilisateurs.Max(u => u.IdUtilisateur) + 1 : 1;

            utilisateurs.Add(new Utilisateur
            {
                IdUtilisateur = newId,
                Login = login,
                MotDePasseHash = PasswordHasher.Hash(mdp),
                Role = role,
                EstActif = true,
                IdClient = idClient
            });

            JsonStorage.SaveList(FichierUtilisateurs, utilisateurs);

            AuditLogger.Log(_adminCourant, "Création utilisateur",
                $"Utilisateur {login} (rôle {role}) créé.");

            Console.WriteLine("Utilisateur créé avec succès.");
        }

        private void DesactiverUtilisateur()
        {
            var utilisateurs = JsonStorage.LoadList<Utilisateur>(FichierUtilisateurs);

            if (!utilisateurs.Any())
            {
                Console.WriteLine("Aucun utilisateur trouvé.");
                return;
            }

            Console.WriteLine("\nListe des utilisateurs :");
            for (int i = 0; i < utilisateurs.Count; i++)
            {
                var u = utilisateurs[i];
                Console.WriteLine($"{i + 1}. {u.Login} - Rôle: {u.Role} - Actif: {u.EstActif}");
            }

            int choix = InputHelper.LireEntier("Sélectionnez l'utilisateur à désactiver : ", 1, utilisateurs.Count);
            var user = utilisateurs[choix - 1];

            user.EstActif = false;
            JsonStorage.SaveList(FichierUtilisateurs, utilisateurs);

            AuditLogger.Log(_adminCourant, "Désactivation utilisateur",
                $"Utilisateur {user.Login} désactivé.");

            Console.WriteLine("Utilisateur désactivé.");
        }

        private void ReinitialiserMotDePasse()
        {
            var utilisateurs = JsonStorage.LoadList<Utilisateur>(FichierUtilisateurs);

            if (!utilisateurs.Any())
            {
                Console.WriteLine("Aucun utilisateur trouvé.");
                return;
            }

            Console.WriteLine("\n=== Réinitialisation du mot de passe ===");
            for (int i = 0; i < utilisateurs.Count; i++)
            {
                var u = utilisateurs[i];
                Console.WriteLine($"{i + 1}. {u.Login} - Rôle: {u.Role} - Actif: {u.EstActif}");
            }

            int choix = InputHelper.LireEntier("Sélectionnez l'utilisateur : ", 1, utilisateurs.Count);
            var user = utilisateurs[choix - 1];

            Console.WriteLine($"\nUtilisateur sélectionné : {user.Login} (Rôle: {user.Role})");

            Console.Write("Nouveau mot de passe : ");
            string? nouveauMdp = Console.ReadLine();

            Console.Write("Confirmez le nouveau mot de passe : ");
            string? confirmation = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(nouveauMdp) || nouveauMdp != confirmation)
            {
                Console.WriteLine("Les mots de passe ne correspondent pas ou sont vides. Opération annulée.");
                return;
            }

            user.MotDePasseHash = PasswordHasher.Hash(nouveauMdp);
            JsonStorage.SaveList(FichierUtilisateurs, utilisateurs);

            AuditLogger.Log(_adminCourant, "Réinitialisation mot de passe",
                $"Mot de passe réinitialisé pour l'utilisateur {user.Login}.");

            Console.WriteLine("Mot de passe réinitialisé avec succès.");
            Console.WriteLine($"Nouveau mot de passe (à communiquer au client) : {nouveauMdp}");
        }

        // ======================================
        // 3) DÉFINIR LES TYPES DE COMPTES
        // ======================================
        private void GererTypesComptes()
        {
            var types = JsonStorage.LoadList<TypeCompte>(FichierTypesComptes);

            Console.WriteLine("\n=== Types de comptes existants ===");
            foreach (var t in types)
            {
                Console.WriteLine($"{t.IdTypeDeCompte}. {t.Nom} - Taux: {t.TauxInteret}%");
            }

            Console.WriteLine("\nCréation d'un nouveau type de compte :");
            string nom = InputHelper.LireTexteObligatoire("Nom du type (Epargne, Débit, Crédit, etc.) : ");
            double taux = InputHelper.LireDouble("Taux d'intérêt (%) : ", min: 0);

            int newId = types.Any() ? types.Max(t => t.IdTypeDeCompte) + 1 : 1;

            types.Add(new TypeCompte
            {
                IdTypeDeCompte = newId,
                Nom = nom,
                TauxInteret = taux
            });

            JsonStorage.SaveList(FichierTypesComptes, types);

            AuditLogger.Log(_adminCourant, "Création type compte",
                $"TypeCompte {nom} (taux {taux}) créé.");

            Console.WriteLine("Type de compte créé avec succès.");
        }

        // ======================================
        // 4) CALCUL DES INTÉRÊTS MENSUELS
        // ======================================
        private void CalculerInteretsMensuels()
        {
            var comptes = JsonStorage.LoadList<Compte>(FichierComptes);
            var types = JsonStorage.LoadList<TypeCompte>(FichierTypesComptes);
            var operations = JsonStorage.LoadList<Operation>(FichierOperations);

            int nbComptesAjustes = 0;

            foreach (var c in comptes)
            {
                var type = types.FirstOrDefault(t => t.IdTypeDeCompte == c.IdTypeDeCompte);
                if (type == null || type.TauxInteret <= 0 || c.Solde <= 0)
                    continue;

                double interet = c.Solde * (type.TauxInteret / 100.0);
                if (interet <= 0) continue;

                c.Solde += interet;
                nbComptesAjustes++;

                int newIdOp = operations.Any() ? operations.Max(o => o.IdOperation) + 1 : 1;
                operations.Add(new Operation
                {
                    IdOperation = newIdOp,
                    DateOperation = DateTime.Now,
                    TypeOperation = "Interet",
                    Montant = interet,
                    IbanCompte = c.Iban
                });
            }

            JsonStorage.SaveList(FichierComptes, comptes);
            JsonStorage.SaveList(FichierOperations, operations);

            AuditLogger.Log(_adminCourant, "Calcul intérêts",
                $"Intérêts mensuels appliqués sur {nbComptesAjustes} compte(s).");

            Console.WriteLine($"Intérêts appliqués sur {nbComptesAjustes} compte(s).");
        }

        // ======================================
        // 5) ALERTES SUR SOLDE MINIMUM
        // ======================================
        private void GenererAlertesSoldeMinimum()
        {
            var comptes = JsonStorage.LoadList<Compte>(FichierComptes);
            var alertes = JsonStorage.LoadList<AlerteSolde>(FichierAlertes);

            var comptesEnAlerte = comptes.Where(c => c.Solde < c.SoldeMinimum).ToList();
            if (!comptesEnAlerte.Any())
            {
                Console.WriteLine("Aucun compte en-dessous du solde minimum.");
                return;
            }

            int newId = alertes.Any() ? alertes.Max(a => a.IdAlerte) + 1 : 1;

            foreach (var c in comptesEnAlerte)
            {
                string msg = $"Solde {c.Solde} inférieur au minimum {c.SoldeMinimum} pour le compte {c.Iban}";
                alertes.Add(new AlerteSolde
                {
                    IdAlerte = newId++,
                    IbanCompte = c.Iban,
                    DateAlerte = DateTime.Now,
                    Message = msg
                });

                Console.WriteLine(msg);
            }

            JsonStorage.SaveList(FichierAlertes, alertes);

            AuditLogger.Log(_adminCourant, "Alertes solde minimum",
                $"{comptesEnAlerte.Count} compte(s) en-dessous du solde minimum.");
        }
    }
}
