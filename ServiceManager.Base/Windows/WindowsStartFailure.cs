using System;
using System.ComponentModel;
using ServiceManager.Base.Data;

namespace ServiceManager.Base.Windows
{
    /// <summary>
    /// What is said about a process that did not start. The operating system names the reason, and without it a
    /// start failure only tells that something went wrong: a wrong password, a missing profile and a path that
    /// leads nowhere all look the same. Nothing here starts a process, which is what makes the wording testable.
    /// </summary>
    public static class WindowsStartFailure
    {
        /// <summary>
        /// The reason the operating system gave. A logon failure carries its error code as well, because the
        /// message alone does not tell apart a wrong password from an account that may not log on locally.
        /// </summary>
        public static String Reason(Exception ex)
        {
            if (ex == null)
            {
                return "the operating system reported no reason";
            }

            var win32 = ex as Win32Exception;

            if (win32 != null)
            {
                return win32.Message + " (" + win32.NativeErrorCode + ")";
            }

            return ex.Message;
        }

        /// <summary>The line that lands in the error stream of the service.</summary>
        public static String ErrorLine(RunAsAccount account, Exception ex)
        {
            var line = "SERVICE MANAGER: START FAILED";

            if (account != null && account.UsesLogon)
            {
                line += ", ACCOUNT " + account.FullName;
            }

            return line + ": " + Reason(ex) + "\r\n";
        }

        /// <summary>The line the console of the ServiceManager shows.</summary>
        public static String ConsoleLine(String title, RunAsAccount account, Exception ex)
        {
            var line = "ServiceManager: " + title + " could not be started";

            if (account != null && account.UsesLogon)
            {
                line += " as " + account.FullName;
            }

            return line + ": " + Reason(ex);
        }
    }
}
