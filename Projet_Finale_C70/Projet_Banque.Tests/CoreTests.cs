using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Projet_Banque.Core.Data;
using Projet_Banque.Core.Models;
using Projet_Banque.Core.Security;

namespace Projet_Banque.Tests
{
    [TestClass]
    public class CoreTests
    {
        // ================================
        // TESTS SUR PasswordHasher
        // ================================
        [TestMethod]
        public void PasswordHasher_SameInput_ProducesSameHash()
        {
            // Arrange
            string password = "monSuperMdp123";

            // Act
            string hash1 = PasswordHasher.Hash(password);
            string hash2 = PasswordHasher.Hash(password);

            // Assert
            Assert.AreEqual(hash1, hash2, "Le même mot de passe doit produire le même hash.");
        }

        [TestMethod]
        public void PasswordHasher_DifferentInput_ProducesDifferentHash()
        {
            // Arrange
            string pwd1 = "mdp1";
            string pwd2 = "mdp2";

            // Act
            string hash1 = PasswordHasher.Hash(pwd1);
            string hash2 = PasswordHasher.Hash(pwd2);

            // Assert
            Assert.AreNotEqual(hash1, hash2, "Deux mots de passe différents ne devraient pas avoir le même hash.");
        }

        // ================================
        // TESTS SUR JsonStorage
        // ================================
        [TestMethod]
        public void JsonStorage_SaveAndLoadList_WorksCorrectly()
        {
            // Arrange : on crée un fichier temporaire
            string tempFile = Path.GetTempFileName();

            var clients = new List<Client>
            {
                new Client { IdClient = 1, Nom = "Test", Prenom = "Client1" },
                new Client { IdClient = 2, Nom = "Autre", Prenom = "Client2" }
            };

            // Act
            JsonStorage.SaveList(tempFile, clients);
            var loaded = JsonStorage.LoadList<Client>(tempFile);

            // Assert
            Assert.AreEqual(2, loaded.Count, "On doit retrouver le même nombre de clients.");
            Assert.AreEqual("Test", loaded[0].Nom);
            Assert.AreEqual("Client2", loaded[1].Prenom);
        }

        // ================================
        // TESTS SUR AuthService
        // ================================
        [TestMethod]
        public void AuthService_Authentifier_ReturnsUser_WhenCredentialsAreValid()
        {
            // Arrange : créer un fichier JSON d'utilisateurs factice
            string tempFile = Path.GetTempFileName();

            var utilisateurs = new List<Utilisateur>
            {
                new Utilisateur
                {
                    IdUtilisateur = 1,
                    Login = "client1",
                    MotDePasseHash = PasswordHasher.Hash("secret123"),
                    Role = "Client",
                    EstActif = true,
                    IdClient = 10
                },
                new Utilisateur
                {
                    IdUtilisateur = 2,
                    Login = "admin",
                    MotDePasseHash = PasswordHasher.Hash("admin123"),
                    Role = "Admin",
                    EstActif = true,
                    IdClient = 0
                }
            };

            JsonStorage.SaveList(tempFile, utilisateurs);

            var authService = new AuthService(tempFile);

            // Act
            var user = authService.Authentifier("client1", "secret123", "Client");

            // Assert
            Assert.IsNotNull(user, "L'utilisateur devrait être authentifié.");
            Assert.AreEqual("client1", user!.Login);
            Assert.AreEqual("Client", user.Role);
        }

        [TestMethod]
        public void AuthService_Authentifier_ReturnsNull_WhenPasswordIsInvalid()
        {
            // Arrange
            string tempFile = Path.GetTempFileName();

            var utilisateurs = new List<Utilisateur>
            {
                new Utilisateur
                {
                    IdUtilisateur = 1,
                    Login = "client1",
                    MotDePasseHash = PasswordHasher.Hash("secret123"),
                    Role = "Client",
                    EstActif = true,
                    IdClient = 10
                }
            };

            JsonStorage.SaveList(tempFile, utilisateurs);

            var authService = new AuthService(tempFile);

            // Act
            var user = authService.Authentifier("client1", "mauvaisMdp", "Client");

            // Assert
            Assert.IsNull(user, "L'authentification doit échouer avec un mauvais mot de passe.");
        }
    }
}
