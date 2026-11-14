namespace ServiceManager.Base.Events
{
    /// <summary>
    /// What the connected clients are told about an event. The names follow the operations of
    /// <see cref="Wcf.ICallbacks"/>, so an event says which push it stands for. None means the event
    /// is for the notification providers only.
    /// </summary>
    public enum eCallback
    {
        none = 0,
        updated = 1,
        started = 2,
        exited = 3,
        exiting = 4,
        logs = 5
    }
}
