using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projet_Banque.Core.Data
{
    public static class FilePathManager
    {
        public static readonly string BasePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

        static FilePathManager()
        {
            if (!Directory.Exists(BasePath))
                Directory.CreateDirectory(BasePath);
        }

        public static string Clients => Path.Combine(BasePath, "clients.json");
        public static string Comptes => Path.Combine(BasePath, "comptes.json");
        public static string TypesComptes => Path.Combine(BasePath, "typesComptes.json");
        public static string Utilisateurs => Path.Combine(BasePath, "utilisateurs.json");
        public static string Operations => Path.Combine(BasePath, "operations.json");
        public static string Demandes => Path.Combine(BasePath, "demandesComptes.json");
        public static string Alertes => Path.Combine(BasePath, "alertes.json");
        public static string Audit => Path.Combine(BasePath, "audit.json");
    }
}
