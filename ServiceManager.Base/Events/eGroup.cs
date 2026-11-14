namespace ServiceManager.Base.Events
{
    /// <summary>
    /// The groups the bus delivers to. Each group has its own queue and its own thread, so a provider that
    /// waits for its webhook cannot hold up the push to the client.
    /// </summary>
    public enum eGroup
    {
        clients = 0,
        providers = 1
    }
}
