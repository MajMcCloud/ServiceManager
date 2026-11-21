using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceManager.Base.Wcf;
using System.ServiceModel.Channels;

namespace ServiceManager.Tests
{
    /// <summary>
    /// The credential travels as a header, so both transports carry it without giving up the Windows
    /// sign in of the connection. The header names are part of the contract between UI and service.
    /// </summary>
    [TestClass]
    public class EndpointCredentialTests
    {
        [TestMethod]
        public void the_header_round_trips_user_and_password()
        {
            var request = Message.CreateMessage(MessageVersion.Soap12, "urn:servicemanager:Init");

            EndpointCredential.AddTo(request, "service.watcher", "s3cret");

            var read = EndpointCredential.ReadFrom(request);

            Assert.IsNotNull(read, "the service reads what the client sent");
            Assert.AreEqual("service.watcher", read.User);
            Assert.AreEqual("s3cret", read.Password);
        }

        [TestMethod]
        public void a_request_without_the_header_reads_as_no_credential()
        {
            var request = Message.CreateMessage(MessageVersion.Soap12, "urn:servicemanager:Init");

            Assert.IsNull(EndpointCredential.ReadFrom(request), "a caller that sends nothing is not guessed at");
        }

        [TestMethod]
        public void a_missing_message_reads_as_no_credential()
        {
            Assert.IsNull(EndpointCredential.ReadFrom(null));
        }

        [TestMethod]
        public void adding_to_a_missing_message_changes_nothing()
        {
            EndpointCredential.AddTo(null, "service.watcher", "s3cret");
        }

        [TestMethod]
        public void the_header_name_and_namespace_are_stable()
        {
            // An installed user interface talks to an installed service, so renaming either name
            // would leave the credential of every older caller unread.
            Assert.AreEqual("EndpointCredential", EndpointCredential.HeaderName);
            Assert.AreEqual("http://servicemanager/endpoint", EndpointCredential.HeaderNamespace);
        }
    }
}
