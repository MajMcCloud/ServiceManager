using ServiceManager.Extensions;
using System;
using System.Collections.Generic;

namespace ServiceManager.UI.Notifications
{
    /// <summary>
    /// Turns the answer of a provider into the one line the settings dialog shows. What counts as
    /// missing is decided by the provider itself, this class only asks and phrases the result.
    /// </summary>
    public static class ProviderWarning
    {
        /// <summary>
        /// What a provider reports as missing. An extension that does not answer the question is read as
        /// configured, which is all an extension written before the interface can be expected to do.
        /// </summary>
        public static IList<String> Missing(Extension extension)
        {
            if (extension == null || extension.SettingsCheck == null)
                return new String[0];

            try
            {
                var missing = extension.SettingsCheck.MissingSettings();

                return missing ?? new String[0];
            }
            catch (Exception ex)
            {
                // A provider that cannot answer its own settings is not a reason to lose the dialog.
                Console.WriteLine("ProviderWarning: " + ex.Message);

                return new String[0];
            }
        }


        /// <summary>
        /// The line shown beside the provider list. It stays empty when nothing is missing, so the
        /// dialog says nothing about a provider that is ready.
        /// </summary>
        public static String Text(String name, IList<String> missing)
        {
            if (name == null || missing == null || missing.Count == 0)
                return "";

            return name + " is not configured: " + String.Join(", ", missing) + ".";
        }
    }
}
