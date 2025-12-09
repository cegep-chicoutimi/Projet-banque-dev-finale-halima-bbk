using System;
using System.Collections.Generic;
using System.Linq;
using Projet_Banque.Core.Data;
using Projet_Banque.Core.Models;
using Projet_Banque.Core.Security;

namespace Projet_Banque
{
    public class ClientConsoleService
    {
        // Chemins des fichiers JSON
        private readonly string FichierClients;
        private readonly string FichierComptes;
        private readonly string FichierTypesComptes;
        private readonly string FichierOperations;
        private readonly string FichierUtilisateurs;
        private readonly string FichierDemandes;

        private readonly AuthService _authService;

        public ClientConsoleService()
        {
            FichierClients = FilePathManager.Clients;
            FichierComptes = FilePathManager.Comptes;
            FichierTypesComptes = FilePathManager.TypesComptes;
            FichierOperations = FilePathManager.Operations;
            FichierUtilisateurs = FilePathManager.Utilisateurs;
            FichierDemandes = FilePathManager.Demandes;

            _authService = new AuthService(FichierUtilisateurs);
        }

        // =========================
        //  MENU PRINCIPAL CLIENT
        // =========================
        public void MenuPrincipalClient()
        {
            int choix;
            do
            {
                Console.Clear();
                Console.WriteLine("=========== MENU CLIENT ===========");
                Console.WriteLine("1. Remplir une demande de compte (sans connexion)");
                Console.WriteLine("2. Se connecter pour gérer ses comptes");
                Console.WriteLine("3. Retour au menu principal");

                choix = InputHelper.LireEntier("Votre choix : ", 1, 3);

                switch (choix)
                {
                    case 1:
                        SoumettreDemandeCompte();
                        break;
                    case 2:
                        MenuClientAuthentifie();
                        break;
                    case 3:
                        Console.WriteLine("Retour au menu principal...");
                        break;
                }

                if (choix != 3)
                {
                    Console.WriteLine("\nAppuyez sur une touche pour continuer...");
                    Console.ReadKey();
                }

            } while (choix != 3);
        }

        // =======================================
        // 1) FORMULAIRE DE DEMANDE (sans login)
        // =======================================
        private void SoumettreDemandeCompte()
        {
            Console.Clear();
            Console.WriteLine("=== Demande de création de compte bancaire ===");

            Client nouveauClient = new Client
            {
                Nom = InputHelper.LireTexteObligatoire("Nom : "),
                Prenom = InputHelper.LireTexteObligatoire("Prénom : "),
                Adresse = InputHelper.LireTexteObligatoire("Adresse : "),
                Telephone = InputHelper.LireEntier("Téléphone : ")
            };

            // Choix du type de compte
            var types = JsonStorage.LoadList<TypeCompte>(FichierTypesComptes);
            if (!types.Any())
            {
                Console.WriteLine("Aucun type de compte n'est défini. Impossible de soumettre la demande.");
                return;
            }

            Console.WriteLine("\nTypes de comptes disponibles :");
            foreach (var t in types)
            {
                Console.WriteLine($"{t.IdTypeDeCompte}. {t.Nom} (taux : {t.TauxInteret} %)");
            }

            int idType = InputHelper.LireEntier("Choisissez l'id du type de compte : ");
            var typeChoisi = types.FirstOrDefault(t => t.IdTypeDeCompte == idType);
            if (typeChoisi == null)
            {
                Console.WriteLine("Type de compte invalide, demande annulée.");
                return;
            }

            double soldeMin = InputHelper.LireDouble("Solde minimum souhaité : ", min: 0);

            var demandes = JsonStorage.LoadList<DemandeCompte>(FichierDemandes);
            int nouvelId = demandes.Any() ? demandes.Max(d => d.IdDemande) + 1 : 1;

            DemandeCompte demande = new DemandeCompte
            {
                IdDemande = nouvelId,
                Client = nouveauClient,
                IdTypeDeCompte = typeChoisi.IdTypeDeCompte,
                SoldeMinimum = soldeMin,
                DateDemande = DateTime.Now,
                Statut = "EnAttente"
            };

            demandes.Add(demande);
            JsonStorage.SaveList(FichierDemandes, demandes);

            Console.WriteLine("\nVotre demande a été enregistrée avec succès.");
            Console.WriteLine($"Numéro de suivi de la demande : {demande.IdDemande}");
            Console.WriteLine("La banque devra valider votre demande.");
        }

        // =======================================
        // 2) AUTHENTIFICATION + MENU TRANSACTIONS
        // =======================================
        private void MenuClientAuthentifie()
        {
            Utilisateur? utilisateur = AuthentifierClient();
            if (utilisateur == null)
            {
                Console.WriteLine("Authentification échouée.");
                return;
            }

            Console.WriteLine($"\nBienvenue {utilisateur.Login} !");

            int choix;
            do
            {
                Console.WriteLine("\n====== MENU DES TRANSACTIONS ======");
                Console.WriteLine("1. Définir / modifier le solde minimum");
                Console.WriteLine("2. Effectuer un dépôt");
                Console.WriteLine("3. Effectuer un retrait");
                Console.WriteLine("4. Consulter l'état du compte");
                Console.WriteLine("5. Déconnexion");

                choix = InputHelper.LireEntier("Votre choix : ", 1, 5);

                switch (choix)
                {
                    case 1:
                        DefinirSoldeMinimum(utilisateur.IdClient);
                        break;
                    case 2:
                        EffectuerOperation(utilisateur.IdClient, "Depot");
                        break;
                    case 3:
                        EffectuerOperation(utilisateur.IdClient, "Retrait");
                        break;
                    case 4:
                        ConsulterEtatCompte(utilisateur.IdClient);
                        break;
                    case 5:
                        Console.WriteLine("Déconnexion...");
                        break;
                }

            } while (choix != 5);
        }

        private Utilisateur? AuthentifierClient()
        {
            Console.Write("\nLogin : ");
            string? login = Console.ReadLine();

            Console.Write("Mot de passe : ");
            string? mdp = Console.ReadLine();

            return _authService.Authentifier(login ?? "", mdp ?? "", "Client");
        }

        // =========================
        // 3) DÉFINIR SOLDE MINIMUM
        // =========================
        private void DefinirSoldeMinimum(int idClient)
        {
            var comptes = JsonStorage.LoadList<Compte>(FichierComptes);
            var compte = ChoisirCompteDuClient(idClient, comptes);
            if (compte == null) return;

            Console.WriteLine($"\nSolde minimum actuel : {compte.SoldeMinimum}");
            double nouveauMin = InputHelper.LireDouble("Nouveau solde minimum : ", min: 0);

            compte.SoldeMinimum = nouveauMin;
            JsonStorage.SaveList(FichierComptes, comptes);

            Console.WriteLine("Solde minimum mis à jour avec succès.");
        }

        // =========================
        // 4) DÉPÔT / RETRAIT
        // =========================
        private void EffectuerOperation(int idClient, string typeOperation)
        {
            var comptes = JsonStorage.LoadList<Compte>(FichierComptes);
            var compte = ChoisirCompteDuClient(idClient, comptes);
            if (compte == null) return;

            double montant = InputHelper.LireDouble(
                $"\nMontant du {typeOperation.ToLower()} : ",
                min: 0.01
            );

            if (typeOperation == "Retrait")
            {
                if (compte.Solde - montant < 0)
                {
                    Console.WriteLine("Retrait impossible : solde insuffisant.");
                    return;
                }

                compte.Solde -= montant;

                if (compte.Solde < compte.SoldeMinimum)
                {
                    Console.WriteLine("Attention : votre solde est inférieur au solde minimum défini.");
                }
            }
            else if (typeOperation == "Depot")
            {
                compte.Solde += montant;
            }

            JsonStorage.SaveList(FichierComptes, comptes);

            var operations = JsonStorage.LoadList<Operation>(FichierOperations);
            int newId = operations.Any() ? operations.Max(o => o.IdOperation) + 1 : 1;

            operations.Add(new Operation
            {
                IdOperation = newId,
                DateOperation = DateTime.Now,
                TypeOperation = typeOperation,
                Montant = montant,
                IbanCompte = compte.Iban
            });

            JsonStorage.SaveList(FichierOperations, operations);

            Console.WriteLine($"{typeOperation} effectué avec succès. Nouveau solde : {compte.Solde}");
        }

        // =========================
        // 5) CONSULTER ÉTAT COMPTE
        // =========================
        private void ConsulterEtatCompte(int idClient)
        {
            var comptes = JsonStorage.LoadList<Compte>(FichierComptes);
            var types = JsonStorage.LoadList<TypeCompte>(FichierTypesComptes);

            var comptesClient = comptes.Where(c => c.IdClient == idClient).ToList();
            if (!comptesClient.Any())
            {
                Console.WriteLine("Vous n'avez aucun compte.");
                return;
            }

            Console.WriteLine("\n=== Vos comptes ===");
            foreach (var c in comptesClient)
            {
                var type = types.FirstOrDefault(t => t.IdTypeDeCompte == c.IdTypeDeCompte);
                Console.WriteLine("----------------------------------------");
                Console.WriteLine($"IBAN           : {c.Iban}");
                Console.WriteLine($"Type de compte : {type?.Nom ?? "Inconnu"}");
                Console.WriteLine($"Solde          : {c.Solde}");
                Console.WriteLine($"Solde minimum  : {c.SoldeMinimum}");
                Console.WriteLine($"Date ouverture : {c.DateOuverture:d}");
            }
        }

        // =========================
        //  UTILITAIRE : CHOIX COMPTE
        // =========================
        private Compte? ChoisirCompteDuClient(int idClient, List<Compte> comptes)
        {
            var comptesClient = comptes.Where(c => c.IdClient == idClient).ToList();
            if (!comptesClient.Any())
            {
                Console.WriteLine("Aucun compte trouvé pour ce client.");
                return null;
            }

            Console.WriteLine("\nVos comptes :");
            for (int i = 0; i < comptesClient.Count; i++)
            {
                Console.WriteLine($"{i + 1}. IBAN : {comptesClient[i].Iban}   Solde : {comptesClient[i].Solde}");
            }

            int choix = InputHelper.LireEntier("Sélectionnez un compte : ", 1, comptesClient.Count);
            return comptesClient[choix - 1];
        }
    }
}
