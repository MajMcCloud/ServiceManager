using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceManager.Base.Data;

namespace ServiceManager.Tests
{
    [TestClass]
    public class RunAsAccountTests
    {
        [TestMethod]
        public void inherit_needs_no_logon()
        {
            ServiceItem item = new ServiceItem();

            RunAsAccount account = RunAsAccount.Of(item);

            Assert.IsFalse(account.UsesLogon);
            Assert.IsNull(account.Error);
        }

        [TestMethod]
        public void inherit_leaves_the_start_information_alone()
        {
            ServiceItem item = new ServiceItem();
            item.RunAs = ServiceItem.eRunAs.Inherit;
            item.RunAsUser = "someone";

            ProcessStartInfo startInfo = new ProcessStartInfo("cmd.exe");

            RunAsAccount.Of(item).Apply(startInfo);

            Assert.AreEqual(String.Empty, startInfo.UserName);
            Assert.IsNull(startInfo.Password);
            Assert.IsFalse(startInfo.LoadUserProfile);
        }

        [TestMethod]
        public void a_user_account_names_the_account_on_the_start_information()
        {
            ServiceItem item = new ServiceItem();
            item.RunAs = ServiceItem.eRunAs.User;
            item.RunAsUser = "serviceaccount";
            item.RunAsPassword = SecretStorage.Protect("s3cret");

            ProcessStartInfo startInfo = new ProcessStartInfo("cmd.exe");

            RunAsAccount account = RunAsAccount.Of(item);
            account.Apply(startInfo);

            Assert.AreEqual("serviceaccount", startInfo.UserName);
            Assert.AreEqual("s3cret", secureText(startInfo.Password));
        }

        [TestMethod]
        public void a_local_account_keeps_the_domain_empty()
        {
            ServiceItem item = new ServiceItem();
            item.RunAs = ServiceItem.eRunAs.User;
            item.RunAsUser = "  serviceaccount  ";

            RunAsAccount account = RunAsAccount.Of(item);

            Assert.AreEqual("serviceaccount", account.UserName);
            Assert.AreEqual(String.Empty, account.Domain);
            Assert.AreEqual("serviceaccount", account.FullName);
        }

        [TestMethod]
        public void a_separate_domain_stays_on_its_own()
        {
            ServiceItem item = new ServiceItem();
            item.RunAs = ServiceItem.eRunAs.User;
            item.RunAsUser = "serviceaccount";
            item.RunAsDomain = "CONTOSO";

            ProcessStartInfo startInfo = new ProcessStartInfo("cmd.exe");

            RunAsAccount.Of(item).Apply(startInfo);

            Assert.AreEqual("serviceaccount", startInfo.UserName);
            Assert.AreEqual("CONTOSO", startInfo.Domain);
        }

        [TestMethod]
        public void a_domain_written_in_front_of_the_name_is_taken_apart()
        {
            ServiceItem item = new ServiceItem();
            item.RunAs = ServiceItem.eRunAs.User;
            item.RunAsUser = "CONTOSO\\serviceaccount";

            RunAsAccount account = RunAsAccount.Of(item);

            Assert.AreEqual("CONTOSO", account.Domain);
            Assert.AreEqual("serviceaccount", account.UserName);
            Assert.AreEqual("CONTOSO\\serviceaccount", account.FullName);
        }

        [TestMethod]
        public void the_profile_of_the_account_is_loaded()
        {
            ServiceItem item = new ServiceItem();
            item.RunAs = ServiceItem.eRunAs.User;
            item.RunAsUser = "serviceaccount";

            ProcessStartInfo startInfo = new ProcessStartInfo("cmd.exe");

            RunAsAccount.Of(item).Apply(startInfo);

            Assert.IsTrue(startInfo.LoadUserProfile);
        }

        [TestMethod]
        public void the_stored_password_is_read_back_for_the_logon()
        {
            ServiceItem item = new ServiceItem();
            item.RunAs = ServiceItem.eRunAs.User;
            item.RunAsUser = "serviceaccount";
            item.RunAsPassword = SecretStorage.Protect("s3cret Pässwörd!");

            RunAsAccount account = RunAsAccount.Of(item);

            Assert.AreEqual("s3cret Pässwörd!", account.Password);
        }

        [TestMethod]
        public void a_missing_account_name_is_reported()
        {
            ServiceItem item = new ServiceItem();
            item.RunAs = ServiceItem.eRunAs.User;
            item.RunAsUser = "   ";

            RunAsAccount account = RunAsAccount.Of(item);

            Assert.IsNotNull(account.Error);
            Assert.IsNotNull(account.ErrorLine());
        }

        [TestMethod]
        public void a_password_from_another_machine_is_reported()
        {
            ServiceItem item = new ServiceItem();
            item.RunAs = ServiceItem.eRunAs.User;
            item.RunAsUser = "serviceaccount";
            item.RunAsPassword = "dpapi:not base64!";

            RunAsAccount account = RunAsAccount.Of(item);

            Assert.IsNotNull(account.Error);
            Assert.AreEqual(String.Empty, account.Password);
        }

        [TestMethod]
        public void a_broken_account_leaves_the_start_information_alone()
        {
            ServiceItem item = new ServiceItem();
            item.RunAs = ServiceItem.eRunAs.User;

            ProcessStartInfo startInfo = new ProcessStartInfo("cmd.exe");

            RunAsAccount.Of(item).Apply(startInfo);

            Assert.AreEqual(String.Empty, startInfo.UserName);
            Assert.IsNull(startInfo.Password);
        }

        [TestMethod]
        public void an_account_without_a_stored_password_starts_with_an_empty_one()
        {
            ServiceItem item = new ServiceItem();
            item.RunAs = ServiceItem.eRunAs.User;
            item.RunAsUser = "serviceaccount";

            ProcessStartInfo startInfo = new ProcessStartInfo("cmd.exe");

            RunAsAccount account = RunAsAccount.Of(item);
            account.Apply(startInfo);

            Assert.IsNull(account.Error);
            Assert.AreEqual(String.Empty, secureText(startInfo.Password));
        }

        private static String secureText(SecureString value)
        {
            if (value == null)
            {
                return null;
            }

            IntPtr pointer = Marshal.SecureStringToGlobalAllocUnicode(value);

            try
            {
                return Marshal.PtrToStringUni(pointer);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(pointer);
            }
        }
    }
}
