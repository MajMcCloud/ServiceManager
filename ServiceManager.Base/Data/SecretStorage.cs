using System;
using System.Security.Cryptography;
using System.Text;

namespace ServiceManager.Base.Data
{
    /// <summary>Protects settings secrets with this machine's DPAPI key.</summary>
    public static class SecretStorage
    {
        private const String Prefix = "dpapi:";

        /// <summary>Protects plaintext, including passwords that start with the storage marker.</summary>
        public static String Protect(String value)
        {
            if (String.IsNullOrEmpty(value))
            {
                return value;
            }

            byte[] sealedBytes = ProtectedData.Protect(Encoding.UTF8.GetBytes(value), null, DataProtectionScope.LocalMachine);

            return Prefix + Convert.ToBase64String(sealedBytes);
        }

        /// <summary>Decrypts a stored value; unreadable protected values raise an error.</summary>
        public static String Unprotect(String value)
        {
            if (String.IsNullOrEmpty(value) || !IsProtected(value))
            {
                return value;
            }

            byte[] sealedBytes = Convert.FromBase64String(value.Substring(Prefix.Length));

            return Encoding.UTF8.GetString(ProtectedData.Unprotect(sealedBytes, null, DataProtectionScope.LocalMachine));
        }

        /// <summary>True when the value carries the marker of a sealed secret.</summary>
        public static bool IsProtected(String value)
        {
            return value != null && value.StartsWith(Prefix, StringComparison.Ordinal);
        }
    }
}
