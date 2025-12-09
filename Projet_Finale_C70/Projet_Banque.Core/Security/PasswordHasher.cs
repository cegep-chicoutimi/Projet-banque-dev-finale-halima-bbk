using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
namespace Projet_Banque.Core.Security
{
   

    public static class PasswordHasher
    {
        public static string Hash(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return "";

            using var sha = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(plainText);
            byte[] hash = sha.ComputeHash(bytes);
            return Convert.ToHexString(hash); // format HEXA
        }

        public static bool Verify(string plainText, string hash)
        {
            return Hash(plainText) == hash;
        }
    }

}
