using System;
using System.Collections.Generic;

namespace ServiceManager.Base.Windows
{
    /// <summary>
    /// The decisions behind the dependency wait of a managed service: which dependency is still missing,
    /// whether the wait has run out and what the log and the providers are told about it. Nothing here waits
    /// and nothing here talks to the machine, which is what makes these rules testable.
    /// </summary>
    public static class WindowsDependencyWait
    {
        /// <summary>How often the wait looks at the machine again.</summary>
        public const int Poll_Milliseconds = 1000;

        /// <summary>The wait used when the settings do not name one.</summary>
        public const int Default_Timeout_Seconds = 120;

        /// <summary>
        /// The first dependency that is not running, null when every dependency is met. Names without a value
        /// are skipped, a service the machine does not offer counts as missing.
        /// </summary>
        public static String FirstMissing(IEnumerable<String> names, IWindowsServiceStatus status)
        {
            if (status == null || names == null)
            {
                return null;
            }

            foreach (String name in names)
            {
                if (String.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                if (!status.IsRunning(name))
                {
                    return name;
                }
            }

            return null;
        }

        /// <summary>
        /// True once the wait has used up its budget. A budget of zero means do not wait at all, so a missing
        /// dependency fails straight away.
        /// </summary>
        public static bool HasTimedOut(DateTime started, int timeoutSeconds, DateTime now)
        {
            if (timeoutSeconds <= 0)
            {
                return true;
            }

            return now.Subtract(started).TotalSeconds >= timeoutSeconds;
        }

        /// <summary>The line the providers get while the service holds back.</summary>
        public static String WaitText(String instanceName, String title, String missing)
        {
            return instanceName + ": " + title + "\r\nStatus: Waiting for " + missing;
        }

        /// <summary>The line the providers get when the wait ran out.</summary>
        public static String TimeoutText(String instanceName, String title, String missing, int timeoutSeconds)
        {
            return instanceName + ": " + title + "\r\nStatus: Start failed, " + missing + " did not start within " + timeoutSeconds + " seconds";
        }

        /// <summary>The line that lands in the error stream of the service while it waits.</summary>
        public static String WaitError(String missing)
        {
            return "SERVICE MANAGER: WAITING FOR WINDOWS SERVICE " + missing + "\r\n";
        }

        /// <summary>The line that lands in the error stream of the service when the wait ran out.</summary>
        public static String TimeoutError(String missing, int timeoutSeconds)
        {
            return "SERVICE MANAGER: START FAILED, WINDOWS SERVICE " + missing + " DID NOT START WITHIN " + timeoutSeconds + " SECONDS\r\n";
        }
    }
}
