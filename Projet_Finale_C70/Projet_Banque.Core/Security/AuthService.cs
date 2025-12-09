using System.Linq;
using Projet_Banque.Core.Data;
using Projet_Banque.Core.Models;

namespace Projet_Banque.Core.Security
{
    public class AuthService
    {
        private readonly string _fichierUtilisateurs;

        public AuthService(string fichierUtilisateurs)
        {
            _fichierUtilisateurs = fichierUtilisateurs;
        }

        public Utilisateur? Authentifier(string login, string motDePasse, string roleRequis)
        {
            var utilisateurs = JsonStorage.LoadList<Utilisateur>(_fichierUtilisateurs);

            string hash = PasswordHasher.Hash(motDePasse);

            return utilisateurs.FirstOrDefault(u =>
                u.Login == login &&
                u.MotDePasseHash == hash &&
                u.EstActif &&
                u.Role == roleRequis);
        }
    }
}
