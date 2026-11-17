using System;
using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceManager.Base.Data;
using ServiceManager.Base.Windows;

namespace ServiceManager.Tests
{
    [TestClass]
    public class WindowsStartFailureTests
    {
        [TestMethod]
        public void reason_names_the_win32_message_with_its_error_code()
        {
            Exception ex = new Win32Exception(1326, "Logon failure: unknown user name or bad password");

            String reason = WindowsStartFailure.Reason(ex);

            Assert.AreEqual("Logon failure: unknown user name or bad password (1326)", reason);
        }

        [TestMethod]
        public void reason_uses_the_message_of_any_other_exception()
        {
            Exception ex = new InvalidOperationException("The system cannot find the file specified");

            String reason = WindowsStartFailure.Reason(ex);

            Assert.AreEqual("The system cannot find the file specified", reason);
        }

        [TestMethod]
        public void reason_says_so_when_nothing_was_caught()
        {
            String reason = WindowsStartFailure.Reason(null);

            Assert.AreEqual("the operating system reported no reason", reason);
        }

        [TestMethod]
        public void error_line_names_the_account_of_a_failed_logon()
        {
            Exception ex = new Win32Exception(1326, "Logon failure: unknown user name or bad password");

            String line = WindowsStartFailure.ErrorLine(userAccount("CONTOSO", "serviceaccount"), ex);

            Assert.AreEqual("SERVICE MANAGER: START FAILED, ACCOUNT CONTOSO\\serviceaccount: Logon failure: unknown user name or bad password (1326)\r\n", line);
        }

        [TestMethod]
        public void error_line_stays_plain_for_an_inherited_account()
        {
            Exception ex = new Win32Exception(2, "The system cannot find the file specified");

            String line = WindowsStartFailure.ErrorLine(RunAsAccount.Of(new ServiceItem()), ex);

            Assert.AreEqual("SERVICE MANAGER: START FAILED: The system cannot find the file specified (2)\r\n", line);
        }

        [TestMethod]
        public void error_line_survives_a_missing_account_and_exception()
        {
            String line = WindowsStartFailure.ErrorLine(null, null);

            Assert.AreEqual("SERVICE MANAGER: START FAILED: the operating system reported no reason\r\n", line);
        }

        [TestMethod]
        public void console_line_names_the_account_the_logon_was_made_with()
        {
            Exception ex = new Win32Exception(1385, "The logon attempt failed");

            String line = WindowsStartFailure.ConsoleLine("Test application", userAccount("CONTOSO", "serviceaccount"), ex);

            Assert.AreEqual("ServiceManager: Test application could not be started as CONTOSO\\serviceaccount: The logon attempt failed (1385)", line);
        }

        [TestMethod]
        public void console_line_leaves_out_an_inherited_account()
        {
            Exception ex = new Win32Exception(3, "The system cannot find the path specified");

            String line = WindowsStartFailure.ConsoleLine("Test application", RunAsAccount.Of(new ServiceItem()), ex);

            Assert.AreEqual("ServiceManager: Test application could not be started: The system cannot find the path specified (3)", line);
        }

        private static RunAsAccount userAccount(String domain, String user)
        {
            ServiceItem item = new ServiceItem();
            item.RunAs = ServiceItem.eRunAs.User;
            item.RunAsDomain = domain;
            item.RunAsUser = user;

            return RunAsAccount.Of(item);
        }
    }
}
