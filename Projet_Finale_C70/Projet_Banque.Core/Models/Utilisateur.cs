using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projet_Banque.Core.Models
{
    public class Utilisateur
    {
        public int IdUtilisateur { get; set; }
        public string Login { get; set; } = "";
        public string MotDePasseHash { get; set; } = ""; // pour le projet on peut stocker le mot de passe en clair
        public string Role { get; set; } = "Client";      // Client, Admin, etc.
        public bool EstActif { get; set; } = true;
        public int IdClient { get; set; }
    }
}
