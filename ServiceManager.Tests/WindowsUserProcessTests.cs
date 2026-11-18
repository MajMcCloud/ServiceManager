using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceManager.Base.Data;
using ServiceManager.Base.Windows;
using System;

namespace ServiceManager.Tests
{
    /// <summary>
    /// A process of another account is created from a token when the manager itself runs as LocalSystem, and it
    /// is started the way the framework does in every other case. Which of the two paths a service takes is a
    /// decision that can be checked without logging anyone on.
    /// </summary>
    [TestClass]
    public class WindowsUserProcessTests
    {
        [TestMethod]
        public void the_token_path_is_wanted_for_an_account_while_the_manager_is_localsystem()
        {
            Assert.IsTrue(WindowsUserProcess.WantsTokenPath(account(ServiceItem.eRunAs.User, "MajMcCloud"), true));
        }

        [TestMethod]
        public void the_token_path_is_not_wanted_while_the_manager_runs_as_a_person()
        {
            Assert.IsFalse(WindowsUserProcess.WantsTokenPath(account(ServiceItem.eRunAs.User, "MajMcCloud"), false));
        }

        [TestMethod]
        public void the_token_path_is_not_wanted_for_a_service_that_inherits_the_account()
        {
            Assert.IsFalse(WindowsUserProcess.WantsTokenPath(account(ServiceItem.eRunAs.Inherit, null), true));
        }

        [TestMethod]
        public void the_token_path_is_not_wanted_without_an_account()
        {
            Assert.IsFalse(WindowsUserProcess.WantsTokenPath(null, true));
        }

        [TestMethod]
        public void the_token_path_is_wanted_for_a_built_in_account_while_the_manager_is_localsystem()
        {
            Assert.IsTrue(WindowsUserProcess.WantsTokenPath(builtIn(ServiceItem.eRunAs.LocalService), true));
        }

        [TestMethod]
        public void a_built_in_account_is_logged_on_as_a_service()
        {
            Assert.AreEqual(WindowsUserProcess.Logon_Type_Service, WindowsUserProcess.LogonTypeOf(builtIn(ServiceItem.eRunAs.NetworkService)));
        }

        [TestMethod]
        public void a_named_user_is_logged_on_interactively()
        {
            Assert.AreEqual(WindowsUserProcess.Logon_Type_Interactive, WindowsUserProcess.LogonTypeOf(account(ServiceItem.eRunAs.User, "MajMcCloud")));
        }

        [TestMethod]
        public void the_command_line_quotes_the_path_of_the_program()
        {
            Assert.AreEqual("\"C:\\Program Files\\ServiceManager\\Test.exe\"", WindowsUserProcess.CommandLine("C:\\Program Files\\ServiceManager\\Test.exe"));
        }

        [TestMethod]
        public void the_command_line_of_no_path_is_empty()
        {
            Assert.AreEqual(String.Empty, WindowsUserProcess.CommandLine(null));
        }

        private static RunAsAccount account(ServiceItem.eRunAs mode, String userName)
        {
            var item = new ServiceItem();

            item.RunAs = mode;
            item.RunAsUser = userName;
            item.RunAsPassword = SecretStorage.Protect("s3cret");

            return RunAsAccount.Of(item);
        }

        private static RunAsAccount builtIn(ServiceItem.eRunAs mode)
        {
            ServiceItem item = new ServiceItem();
            item.RunAs = mode;

            return RunAsAccount.Of(item);
        }
    }
}
