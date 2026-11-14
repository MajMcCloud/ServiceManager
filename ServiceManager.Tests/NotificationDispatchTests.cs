using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceManager.Extensions.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ExtensionLoader = ServiceManager.Extensions.Extensions;

namespace ServiceManager.Tests
{
    /// <summary>
    /// Delivery to several providers at once. The dispatcher runs on the thread of its caller, so the
    /// call has handed the event to every provider by the time it returns and the assertions read the
    /// fixture log straight away.
    /// </summary>
    [TestClass]
    public class NotificationDispatchTests
    {
        private static NotificationMessage Event(NotificationKind kind, String text, String service = null, String details = null)
        {
            return new NotificationMessage()
            {
                Kind = kind,
                Text = text,
                InstanceName = "Test instance",
                ServiceTitle = service,
                Details = details
            };
        }


        [TestMethod]
        public void structuredProvider_receivesEveryFieldOfTheEvent()
        {
            var fixture = ExtensionFixture.Compile("structured", (id, name) => ExtensionFixture.StructuredSource("StructuredOne", id, name));

            ExtensionLoader.SendNotification(
                new List<Guid> { fixture.Id },
                Event(NotificationKind.ConsoleError, "Test instance: error in the service", "Service A", "exit code 1"));

            var lines = fixture.SentLines;

            Assert.AreEqual(1, lines.Length, "the provider has to be reached");
            Assert.AreEqual(
                String.Join("|", "message", "Test instance: error in the service", "ConsoleError", "Service A", "exit code 1"),
                lines[0], "the structured overload has to be preferred and every field has to arrive");
        }

        [TestMethod]
        public void providerWrittenAgainstTheOldContract_receivesThePlainText()
        {
            var fixture = ExtensionFixture.Compile("legacy", (id, name) => ExtensionFixture.LegacyRecordSource("LegacyOne", id, name));

            var found = ExtensionLoader.GetNotificationExtensions().FirstOrDefault(a => a.Id == fixture.Id);

            Assert.IsNotNull(found);
            Assert.IsNull(found.MessageNotification, "an old extension must not be treated as a structured one");

            ExtensionLoader.SendNotification(
                new List<Guid> { fixture.Id },
                Event(NotificationKind.ServiceStart, "Test instance: Service A started", "Service A"));

            var lines = fixture.SentLines;

            Assert.AreEqual(1, lines.Length);
            Assert.AreEqual("Test instance: Service A started", lines[0], "an old provider sees exactly the text it saw before");
        }

        [TestMethod]
        public void sameProviderListedTwice_isDeliveredToOnlyOnce()
        {
            var fixture = ExtensionFixture.Compile("once", (id, name) => ExtensionFixture.StructuredSource("OnceOne", id, name));

            ExtensionLoader.SendNotification(
                new List<Guid> { fixture.Id, fixture.Id },
                Event(NotificationKind.ConsoleOutput, "Test instance: output"));

            var lines = fixture.SentLines;

            Assert.AreEqual(1, lines.Length, "a provider listed twice must not answer twice");
        }

        [TestMethod]
        public void unknownProviderId_isSkippedWithoutStoppingTheDelivery()
        {
            var fixture = ExtensionFixture.Compile("unknown", (id, name) => ExtensionFixture.StructuredSource("UnknownOne", id, name));

            ExtensionLoader.SendNotification(
                new List<Guid> { Guid.NewGuid(), fixture.Id },
                Event(NotificationKind.ServerStart, "Test instance: Service Manager started"));

            var lines = fixture.SentLines;

            Assert.AreEqual(1, lines.Length, "an id without an extension is dropped, the rest still goes out");
            Assert.AreEqual("message|Test instance: Service Manager started|ServerStart||", lines[0]);
        }

        [TestMethod]
        public void failingProvider_doesNotStopTheProviderBehindIt()
        {
            var broken = ExtensionFixture.Compile("bfailing", (id, name) => ExtensionFixture.FailingStructuredSource("FailingOne", id, name));
            var healthy = ExtensionFixture.Compile("ahealthy", (id, name) => ExtensionFixture.StructuredSource("HealthyOne", id, name));

            ExtensionLoader.SendNotification(
                new List<Guid> { broken.Id, healthy.Id },
                Event(NotificationKind.ConsoleError, "Test instance: error"));

            var lines = healthy.SentLines;

            Assert.AreEqual(1, lines.Length, "a provider that throws only costs itself");
            Assert.AreEqual(0, broken.SentLines.Length);
        }

        [TestMethod]
        public void noProviderIds_neverReachAProvider()
        {
            var fixture = ExtensionFixture.Compile("none", (id, name) => ExtensionFixture.StructuredSource("NoneOne", id, name));

            ExtensionLoader.SendNotification(new List<Guid>(), Event(NotificationKind.ConsoleOutput, "Test instance: output"));
            ExtensionLoader.SendNotification(null, Event(NotificationKind.ConsoleOutput, "Test instance: output"));
            ExtensionLoader.SendNotification(new List<Guid> { fixture.Id }, null);

            Assert.AreEqual(0, fixture.SentLines.Length, "nothing configured means no delivery and no exception");
        }


        [TestMethod]
        public void a_provider_is_handed_the_event_on_the_thread_of_the_caller()
        {
            var fixture = ExtensionFixture.Compile("threaded", (id, name) => ExtensionFixture.ThreadRecordSource("ThreadedOne", id, name));

            ExtensionLoader.SendNotification(
                new List<Guid> { fixture.Id },
                Event(NotificationKind.ServiceStart, "Test instance: Service A started"));

            var lines = fixture.SentLines;

            Assert.AreEqual(1, lines.Length);
            Assert.AreEqual(Thread.CurrentThread.ManagedThreadId.ToString(), lines[0], "the dispatcher starts no thread of its own");
        }
    }
}
