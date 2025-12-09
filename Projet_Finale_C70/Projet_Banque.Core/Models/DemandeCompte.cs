using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projet_Banque.Core.Models
{
    public class DemandeCompte
    {
        public int IdDemande { get; set; }
        public Client Client { get; set; } = new Client();
        public int IdTypeDeCompte { get; set; }
        public double SoldeMinimum { get; set; }
        public DateTime DateDemande { get; set; }
        public string Statut { get; set; } = "EnAttente";
    }
}
