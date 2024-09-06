using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using CefSharp.DevTools.Target;
using System.Windows.Controls;
using System.Security.Policy;

namespace TBChestTracker
{
    public class MD5Helper
    {
        public static string Create(string value)
        {
            MD5 md5Hasher = MD5.Create();
            
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(value));
            
            StringBuilder sBuilder = new StringBuilder();
            
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            
            return sBuilder.ToString();
        }
        public static bool Verify(string input, string hash)
        {
            // Hash the input.
            string hashOfInput = Create(input);
            // Create a StringComparer—an instance of string comparison in .NET; the class is abstract.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;
            // Compare the hashes.
            if (0 == comparer.Compare(hashOfInput, hash))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
