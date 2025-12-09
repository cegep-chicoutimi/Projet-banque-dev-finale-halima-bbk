using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Projet_Banque.Core.Data
{
    public class JsonStorage
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        public static List<T> LoadList<T>(string fileName)
        {
            if (!File.Exists(fileName))
                return new List<T>();

            string json = File.ReadAllText(fileName);
            if (string.IsNullOrWhiteSpace(json))
                return new List<T>();

            return JsonSerializer.Deserialize<List<T>>(json, Options) ?? new List<T>();
        }

        public static void SaveList<T>(string fileName, List<T> data)
        {
            string json = JsonSerializer.Serialize(data, Options);
            File.WriteAllText(fileName, json);
        }
    }
}
