using System;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceManager.Base.Data;

namespace ServiceManager.Tests
{
    [TestClass]
    public class SecretStorageTests
    {
        [TestMethod]
        public void protect_unprotect_round_trip()
        {
            String secret = "s3cret Pässwörd!";

            Assert.AreEqual(secret, SecretStorage.Unprotect(SecretStorage.Protect(secret)));
        }

        [TestMethod]
        public void protected_value_does_not_hold_the_plaintext()
        {
            String secret = "s3cret";

            Assert.IsFalse(SecretStorage.Protect(secret).Contains(secret));
        }

        [TestMethod]
        public void a_password_starting_with_the_marker_round_trips()
        {
            String secret = "dpapi:plaintext-password";

            String protectedValue = SecretStorage.Protect(secret);

            Assert.AreNotEqual(secret, protectedValue);
            Assert.AreEqual(secret, SecretStorage.Unprotect(protectedValue));
        }

        [TestMethod]
        public void unprotect_returns_an_untouched_value_unchanged()
        {
            Assert.AreEqual("plain", SecretStorage.Unprotect("plain"));
        }

        [TestMethod]
        public void empty_secret_stays_empty()
        {
            Assert.AreEqual(String.Empty, SecretStorage.Protect(String.Empty));
            Assert.AreEqual(String.Empty, SecretStorage.Unprotect(String.Empty));
        }

        [TestMethod]
        public void is_protected_only_reports_our_own_format()
        {
            Assert.IsTrue(SecretStorage.IsProtected(SecretStorage.Protect("s3cret")));
            Assert.IsFalse(SecretStorage.IsProtected("plain"));
        }

        [TestMethod]
        public void unprotect_of_a_foreign_blob_reports_an_error()
        {
            Random random = new Random(12345);
            byte[] garbage = new byte[64];
            random.NextBytes(garbage);

            Assert.ThrowsExactly<CryptographicException>(() => SecretStorage.Unprotect("dpapi:" + Convert.ToBase64String(garbage)));
        }

        [TestMethod]
        public void malformed_protected_value_reports_an_error()
        {
            Assert.ThrowsExactly<FormatException>(() => SecretStorage.Unprotect("dpapi:not base64!"));
        }
    }
}
