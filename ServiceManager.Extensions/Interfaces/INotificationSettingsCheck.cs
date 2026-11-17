using System;
using System.Collections.Generic;

namespace ServiceManager.Extensions.Interfaces
{
    /// <summary>
    /// The optional answer of a provider to the question of what its settings still need. It sits on its
    /// own, the way INotificationMessageExtension does: an extension that does not offer it is read as
    /// configured, so an extension written before this interface keeps working and stays silent.
    /// </summary>
    public interface INotificationSettingsCheck
    {
        /// <summary>
        /// The settings a person has to fill in before the provider can deliver, named the way the
        /// settings dialog of the provider names them. An empty list means the provider is ready.
        /// </summary>
        IList<String> MissingSettings();
    }
}
