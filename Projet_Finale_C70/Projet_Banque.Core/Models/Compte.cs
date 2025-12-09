using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projet_Banque.Core.Models
{
    public class Compte
    {
        public string Iban { get; set; } = "";
        public double Solde { get; set; }
        public double SoldeMinimum { get; set; }
        public DateTime DateOuverture { get; set; }
        public int IdClient { get; set; }
        public int IdTypeDeCompte { get; set; }

    }
}
