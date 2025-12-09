using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projet_Banque.Core.Security
{
    public class AuditEntry
    {
        public int Id { get; set; }
        public int? IdUtilisateur { get; set; }
        public string Login { get; set; } = "";
        public DateTime DateAction { get; set; }
        public string Action { get; set; } = "";
        public string Details { get; set; } = "";
    }
}
