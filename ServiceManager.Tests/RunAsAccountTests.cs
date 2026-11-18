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

        [TestMethod]
        public void local_service_is_logged_on_by_the_name_of_the_system()
        {
            ServiceItem item = new ServiceItem();
            item.RunAs = ServiceItem.eRunAs.LocalService;

            RunAsAccount account = RunAsAccount.Of(item);

            Assert.AreEqual("LOCAL SERVICE", account.UserName);
            Assert.AreEqual("NT AUTHORITY", account.Domain);
            Assert.AreEqual("NT AUTHORITY\\LOCAL SERVICE", account.FullName);
        }

        [TestMethod]
        public void local_system_and_network_service_are_their_own_accounts()
        {
            Assert.AreEqual("SYSTEM", builtIn(ServiceItem.eRunAs.LocalSystem).UserName);
            Assert.AreEqual("NETWORK SERVICE", builtIn(ServiceItem.eRunAs.NetworkService).UserName);
        }

        [TestMethod]
        public void a_built_in_account_needs_no_stored_password()
        {
            RunAsAccount account = builtIn(ServiceItem.eRunAs.LocalService);

            Assert.IsNull(account.Error);
            Assert.AreEqual(String.Empty, account.Password);
            Assert.IsTrue(account.UsesLogon);
        }

        [TestMethod]
        public void the_account_of_an_older_settings_file_is_ignored_for_a_built_in_mode()
        {
            ServiceItem item = new ServiceItem();
            item.RunAs = ServiceItem.eRunAs.NetworkService;
            item.RunAsUser = "someone";
            item.RunAsDomain = "CONTOSO";

            RunAsAccount account = RunAsAccount.Of(item);

            Assert.AreEqual("NETWORK SERVICE", account.UserName);
            Assert.AreEqual("NT AUTHORITY", account.Domain);
        }

        [TestMethod]
        public void a_built_in_account_is_not_started_by_the_framework()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo("cmd.exe");

            builtIn(ServiceItem.eRunAs.LocalService).Apply(startInfo);

            Assert.AreEqual(String.Empty, startInfo.UserName);
            Assert.IsNull(startInfo.Password);
            Assert.IsFalse(startInfo.LoadUserProfile);
        }

        [TestMethod]
        public void a_built_in_account_is_usable_while_the_manager_is_localsystem()
        {
            RunAsAccount account = builtIn(ServiceItem.eRunAs.LocalService);
            account.CheckHost(true);

            Assert.IsNull(account.Error);
        }

        [TestMethod]
        public void a_built_in_account_outside_an_installed_service_is_refused()
        {
            RunAsAccount account = builtIn(ServiceItem.eRunAs.LocalService);
            account.CheckHost(false);

            Assert.IsNotNull(account.Error);
            Assert.IsNotNull(account.ErrorLine());
        }

        [TestMethod]
        public void a_named_user_is_usable_outside_an_installed_service()
        {
            ServiceItem item = new ServiceItem();
            item.RunAs = ServiceItem.eRunAs.User;
            item.RunAsUser = "serviceaccount";

            RunAsAccount account = RunAsAccount.Of(item);
            account.CheckHost(false);

            Assert.IsNull(account.Error);
        }

        [TestMethod]
        public void an_inherited_account_never_asks_for_an_installed_service()
        {
            RunAsAccount account = RunAsAccount.Of(new ServiceItem());
            account.CheckHost(false);

            Assert.IsNull(account.Error);
        }

        private static RunAsAccount builtIn(ServiceItem.eRunAs mode)
        {
            ServiceItem item = new ServiceItem();
            item.RunAs = mode;

            return RunAsAccount.Of(item);
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
