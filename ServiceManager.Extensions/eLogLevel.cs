namespace ServiceManager.Extensions
{
    /// <summary>
    /// How serious a line is. Warning and Error are the lines that additionally reach the Windows event
    /// log, so the level decides whether an operator has to look at the machine or only at the log file.
    /// </summary>
    public enum eLogLevel
    {
        Info = 0,
        Warning = 1,
        Error = 2
    }
}
