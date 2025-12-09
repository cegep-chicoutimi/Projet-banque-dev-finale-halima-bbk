using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projet_Banque.Core.Models
{
    public class TypeCompte
    {
        public int IdTypeDeCompte { get; set; }
        public string Nom { get; set; } = "";  // Epargne, Debit, Credit
        public double TauxInteret { get; set; }
    }
}
