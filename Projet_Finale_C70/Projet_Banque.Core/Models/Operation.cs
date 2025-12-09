using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projet_Banque.Core.Models
{
    public class Operation
    {
        public int IdOperation { get; set; }
        public DateTime DateOperation { get; set; }
        public string TypeOperation { get; set; } = ""; // Depot, Retrait, Interet…
        public double Montant { get; set; }
        public string IbanCompte { get; set; } = "";
    }
}
