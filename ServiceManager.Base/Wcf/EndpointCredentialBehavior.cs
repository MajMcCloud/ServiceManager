using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace ServiceManager.Base.Wcf
{
    /// <summary>
    /// Sends the credential of the caller with every request. The header goes with each call rather
    /// than with the first one only, because a service that restarts answers a call at any time.
    /// </summary>
    public class EndpointCredentialBehavior : IEndpointBehavior
    {
        /// <summary>The account named in the header.</summary>
        public String User { get; private set; }

        /// <summary>The password named in the header.</summary>
        public String Password { get; private set; }

        public EndpointCredentialBehavior(String User, String Password)
        {
            this.User = User;
            this.Password = Password;
        }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            // A caller without a user has nothing to send and stays on its Windows identity alone.
            if (String.IsNullOrEmpty(this.User))
            {
                return;
            }

            clientRuntime.ClientMessageInspectors.Add(new CredentialInspector(this.User, this.Password));
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
        }

        public void Validate(ServiceEndpoint endpoint)
        {
        }

        // The inspector of one client channel, named by the account that opened it.
        private class CredentialInspector : IClientMessageInspector
        {
            private String User { get; set; }

            private String Password { get; set; }

            public CredentialInspector(String User, String Password)
            {
                this.User = User;
                this.Password = Password;
            }

            public object BeforeSendRequest(ref Message Request, IClientChannel Channel)
            {
                EndpointCredential.AddTo(Request, this.User, this.Password);

                return null;
            }

            public void AfterReceiveReply(ref Message Reply, object Correlation)
            {
            }
        }
    }
}
