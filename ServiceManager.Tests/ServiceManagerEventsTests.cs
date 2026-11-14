using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceManager.Base;
using ServiceManager.Base.Data;
using ServiceManager.Base.Events;
using ServiceManager.Base.Windows;
using ServiceManager.Extensions.Interfaces;
using System;
using System.Collections.Generic;

namespace ServiceManager.Tests
{
    /// <summary>
    /// The bus is how the rest of the application learns what the manager did to a service, so the events
    /// of a start and of a stop have to be right whether or not a client is connected.
    /// </summary>
    [TestClass]
    public class ServiceManagerEventsTests
    {
        private static readonly Guid Service = new Guid("33333333-3333-3333-3333-333333333333");


        [TestMethod]
        public void a_service_that_holds_back_for_a_dependency_raises_the_wait_once()
        {
            var item = new ServiceItem()
            {
                ID = Service,
                Title = "TelegramApi",
                WindowsDependencies = new List<String> { "Dhcp" }
            };

            var bus = new RecordingBus();

            var manager = new ServiceManager.Base.ServiceManager();

            manager.Config = new ServiceConfig();
            manager.Analytics = new Analytics();
            manager.WindowsStatus = new FakeStatus();
            manager.Events = bus;

            manager.StartService(item);

            Assert.AreEqual(1, bus.Events.Count, "the client push and the provider message travel in one event");

            var e = bus.Events[0];

            Assert.AreEqual(NotificationKind.DependencyWait, e.Kind);
            Assert.AreEqual(eCallback.updated, e.Callback);
            Assert.AreEqual(Service, e.ServiceId);
            Assert.AreEqual("TelegramApi", e.Title);
            Assert.AreEqual("Dhcp", e.Details);
        }


        [TestMethod]
        public void taking_a_waiting_service_offline_raises_an_update_for_the_clients_only()
        {
            var item = new ServiceItem() { ID = Service, Title = "TelegramApi" };

            var bus = new RecordingBus();

            var manager = new ServiceManager.Base.ServiceManager();

            manager.Config = new ServiceConfig();
            manager.Analytics = new Analytics();
            manager.Events = bus;

            manager.PrepareService(item);

            manager.Analytics.Services[0].Status = ServiceAnalytics.eStatus.waiting_for_dependency;

            manager.ServiceEnd(item);

            Assert.AreEqual(1, bus.Events.Count);

            Assert.IsNull(bus.Events[0].Kind, "a status change is no event a provider was asked for");
            Assert.AreEqual(eCallback.updated, bus.Events[0].Callback);
            Assert.AreEqual(Service, bus.Events[0].ServiceId);
        }


        [TestMethod]
        public void stopping_a_service_that_never_ran_raises_nothing()
        {
            var item = new ServiceItem() { ID = Service, Title = "TelegramApi" };

            var bus = new RecordingBus();

            var manager = new ServiceManager.Base.ServiceManager();

            manager.Config = new ServiceConfig();
            manager.Analytics = new Analytics();
            manager.Events = bus;

            manager.PrepareService(item);

            manager.ServiceEnd(item);

            Assert.AreEqual(0, bus.Events.Count, "a stop nobody performed must not appear as an event");
        }


        /// <summary>Takes the place of the bus, so a test reads back what the manager raised.</summary>
        private class RecordingBus : IServiceEventBus
        {
            public List<ServiceEvent> Events { get; } = new List<ServiceEvent>();

            public int Dropped
            {
                get { return 0; }
            }


            public void Publish(ServiceEvent e)
            {
                Events.Add(e);
            }


            public IDisposable Subscribe(eGroup group, Action<ServiceEvent> handler)
            {
                return new Nothing();
            }


            public bool Flush()
            {
                return true;
            }


            public void Complete()
            {
            }


            /// <summary>A manager without subscribers has nothing to unsubscribe from.</summary>
            private class Nothing : IDisposable
            {
                public void Dispose()
                {
                }
            }
        }


        /// <summary>A machine on which no Windows service runs.</summary>
        private class FakeStatus : IWindowsServiceStatus
        {
            public bool IsRunning(String name)
            {
                return false;
            }
        }
    }
}
