namespace ServiceManager.UI.Columns
{
    /// <summary>
    /// Every column the service list knows. The name is the key a stored choice is kept under, so a column
    /// keeps its key once it is added or taken out again.
    /// </summary>
    public enum eColumn
    {
        Activity,
        Pid,
        Title,
        Status,
        Restarts,
        AutoRestart,
        Uptime,
        LastReply,
        Memory,
        Cpu,
        Threads
    }
}
