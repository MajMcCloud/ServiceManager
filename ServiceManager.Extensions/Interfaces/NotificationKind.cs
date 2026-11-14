namespace ServiceManager.Extensions.Interfaces
{
    /// <summary>
    /// What happened. The values are part of the settings file (a target stores one flag per kind),
    /// so reordering them would silently move the flags of every installed configuration.
    /// </summary>
    public enum NotificationKind
    {
        ServerStart = 0,
        ServerShutdown = 1,
        ServiceStart = 2,
        ServiceStop = 3,
        ServiceRestart = 4,
        ConsoleOutput = 5,
        ConsoleError = 6,
        DependencyWait = 7,
        ServiceCrashed = 8
    }
}
