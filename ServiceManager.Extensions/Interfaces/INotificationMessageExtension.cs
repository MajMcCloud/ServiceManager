using System.Threading.Tasks;

namespace ServiceManager.Extensions.Interfaces
{
    /// <summary>
    /// The structured counterpart of INotificationExtension. It is a separate interface on purpose:
    /// an extension compiled against the old contract still loads and still works, the host prefers
    /// this one when the assembly offers it and falls back to Send(String) otherwise.
    /// </summary>
    public interface INotificationMessageExtension : INotificationExtension
    {
        Task Send(NotificationMessage message);
    }
}
