using Projet_Banque.Core.Data;
using Projet_Banque.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projet_Banque.Core.Security
{
    public static class AuditLogger
    {
        private const string FichierAudit = "audit.json";

        public static void Log(Utilisateur? user, string action, string details)
        {
            var logs = JsonStorage.LoadList<AuditEntry>(FichierAudit);
            int newId = logs.Any() ? logs.Max(l => l.Id) + 1 : 1;

            logs.Add(new AuditEntry
            {
                Id = newId,
                IdUtilisateur = user?.IdUtilisateur,
                Login = user?.Login ?? "SYSTEM",
                DateAction = DateTime.Now,
                Action = action,
                Details = details
            });

            JsonStorage.SaveList(FichierAudit, logs);
        }
    }
}
