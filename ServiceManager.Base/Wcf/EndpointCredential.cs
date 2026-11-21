using System;
using System.Runtime.Serialization;
using System.ServiceModel.Channels;

namespace ServiceManager.Base.Wcf
{
    /// <summary>
    /// The user and password a client sends with every request. Both transports sign in with
    /// Windows over the connection itself, so the credential rides along as a header instead of
    /// replacing that sign in.
    /// </summary>
    [DataContract(Name = "Credential", Namespace = "http://servicemanager/endpoint")]
    public class EndpointCredential
    {
        public const String HeaderName = "EndpointCredential";

        public const String HeaderNamespace = "http://servicemanager/endpoint";

        [DataMember(Order = 0)]
        public String User { get; set; }

        [DataMember(Order = 1)]
        public String Password { get; set; }

        /// <summary>
        /// Adds the credential to an outgoing request.
        /// </summary>
        public static void AddTo(Message Message, String User, String Password)
        {
            if (Message == null)
            {
                return;
            }

            EndpointCredential credential = new EndpointCredential();
            credential.User = User;
            credential.Password = Password;

            Message.Headers.Add(MessageHeader.CreateHeader(HeaderName, HeaderNamespace, credential));
        }

        /// <summary>
        /// Reads the credential of an incoming request; a caller that sends none returns null.
        /// </summary>
        public static EndpointCredential ReadFrom(Message Message)
        {
            if (Message == null)
            {
                return null;
            }

            // GetHeader over the names throws over a missing header, which is the normal case for a
            // caller that sends no credential at all.
            int index = Message.Headers.FindHeader(HeaderName, HeaderNamespace);

            if (index < 0)
            {
                return null;
            }

            return Message.Headers.GetHeader<EndpointCredential>(index);
        }
    }
}
