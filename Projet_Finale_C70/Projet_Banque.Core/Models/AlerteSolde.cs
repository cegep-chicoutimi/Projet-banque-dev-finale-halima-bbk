using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projet_Banque.Core.Models
{
    public class AlerteSolde
    {
        public int IdAlerte { get; set; }
        public string IbanCompte { get; set; } = "";
        public DateTime DateAlerte { get; set; }
        public string Message { get; set; } = "";
    }
}
