using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace saturdayclub.Messages
{
    public static class MessageValidator
    {
        public static string ComputeChecksum(string token, string timestamp, string nonce)
        {
            if (string.IsNullOrEmpty(token) ||
                string.IsNullOrEmpty(timestamp) ||
                string.IsNullOrEmpty(nonce))
            {
                return string.Empty;
            }

            string[] param = new[] { token, timestamp, nonce };
            Array.Sort<string>(param);
            string hashParam = string.Join(string.Empty, param);
            byte[] buf = Encoding.UTF8.GetBytes(hashParam);
            using (var sha1 = SHA1.Create())
            {
                var hash = sha1.ComputeHash(buf);
                StringBuilder sb = new StringBuilder();
                foreach (var b in hash)
                {
                    if (b < 0x10)
                        sb.Append('0');
                    sb.AppendFormat("{0:x}", b);
                }
                string hashStr = sb.ToString();
                return hashStr;
            }
        }

        public static bool ValidateChecksum(string signature, string token, string timestamp, string nonce)
        {
            string hash = ComputeChecksum(token, timestamp, nonce);
            if (string.IsNullOrEmpty(hash))
                return false;
            return (string.Compare(hash, signature, false) == 0);
        }
    }
}
