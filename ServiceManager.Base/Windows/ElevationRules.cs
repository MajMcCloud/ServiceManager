using System;

namespace ServiceManager.Base.Windows
{
    /// <summary>
    /// Which operations of the interface ask for elevated rights of the own process. The rules live here
    /// rather than in the forms, so that they can be read and tested without a window. An operation that is
    /// not listed asks for nothing, which is why every value of <see cref="eOperation"/> has to be classified.
    /// </summary>
    public static class ElevationRules
    {
        /// <summary>An operation the interface offers the user.</summary>
        public enum eOperation
        {
            /// <summary>Install the ServiceManager entry of the Windows service control manager.</summary>
            InstallWindowsService,

            /// <summary>Remove the ServiceManager entry of the Windows service control manager.</summary>
            UninstallWindowsService,

            /// <summary>Read and apply the settings of the ServiceManager Windows service.</summary>
            WindowsServiceSettings,

            /// <summary>Start the ServiceManager Windows service.</summary>
            StartWindowsService,

            /// <summary>Restart the ServiceManager Windows service.</summary>
            RestartWindowsService,

            /// <summary>Stop the ServiceManager Windows service.</summary>
            StopWindowsService,

            /// <summary>Start or stop a managed application through a connected instance.</summary>
            ManagedApplication,

            /// <summary>Connect to another instance.</summary>
            ConnectToInstance,

            /// <summary>Host the manager inside the interface process instead of talking to the Windows service.</summary>
            TestingRun
        }

        /// <summary>
        /// True when the operation asks the own process for elevated rights. Everything that runs through a
        /// connected instance is not in that list: an instance that only accepts administrators cannot be
        /// reached by a process without elevated rights in the first place, so the user never stands in front
        /// of a closed door the shield could have warned him about.
        /// </summary>
        public static bool NeedsElevation(eOperation operation)
        {
            switch (operation)
            {
                case eOperation.InstallWindowsService:
                case eOperation.UninstallWindowsService:
                case eOperation.WindowsServiceSettings:
                case eOperation.StartWindowsService:
                case eOperation.RestartWindowsService:
                case eOperation.StopWindowsService:
                    return true;

                case eOperation.ManagedApplication:
                case eOperation.ConnectToInstance:
                case eOperation.TestingRun:
                    return false;

                default:
                    throw new ArgumentOutOfRangeException(nameof(operation), operation, "the operation is not classified");
            }
        }

        /// <summary>
        /// True when an entry has to carry the shield: it asks for elevation and the own process does not
        /// hold it. An elevated process needs no hint, and the entry stays clickable either way.
        /// </summary>
        public static bool NeedsShield(eOperation operation, bool isElevated)
        {
            return NeedsElevation(operation) && !isElevated;
        }
    }
}
