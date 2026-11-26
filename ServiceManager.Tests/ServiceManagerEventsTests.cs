using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceManager.Base;
using ServiceManager.Base.Data;
using ServiceManager.Base.Events;
using ServiceManager.Base.Windows;
using ServiceManager.Extensions;
using ServiceManager.Extensions.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        private static readonly Guid Child = new Guid("44444444-4444-4444-4444-444444444444");

        private static readonly Guid Grandchild = new Guid("55555555-5555-5555-5555-555555555555");

        private String logFolder;

        private String storedPath;

        private bool storedConsole;

        private bool storedEventLog;


        [TestInitialize]
        public void Setup()
        {
            storedPath = SMLog.LogPath;
            storedConsole = SMLog.UseConsole;
            storedEventLog = SMLog.UseEventLog;

            logFolder = Path.Combine(Path.GetTempPath(), "servicemanager-tests", Guid.NewGuid().ToString("N"));

            SMLog.LogPath = logFolder;
            SMLog.UseConsole = false;
            SMLog.UseEventLog = false;
        }


        [TestCleanup]
        public void Teardown()
        {
            SMLog.LogPath = storedPath;
            SMLog.UseConsole = storedConsole;
            SMLog.UseEventLog = storedEventLog;

            if (Directory.Exists(logFolder))
            {
                Directory.Delete(logFolder, true);
            }
        }


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


        [TestMethod]
        public void killing_a_waiting_service_ends_the_wait_without_a_process()
        {
            var item = new ServiceItem() { ID = Service, Title = "TelegramApi" };

            var bus = new RecordingBus();

            var manager = new ServiceManager.Base.ServiceManager();

            manager.Config = new ServiceConfig();
            manager.Analytics = new Analytics();
            manager.Events = bus;

            manager.PrepareService(item);

            manager.Analytics.Services[0].Status = ServiceAnalytics.eStatus.waiting_for_dependency;

            manager.KillService(item);

            Assert.AreEqual(ServiceAnalytics.eStatus.offline, manager.Analytics.Services[0].Status);

            Assert.AreEqual(1, bus.Events.Count);
            Assert.AreEqual(eCallback.updated, bus.Events[0].Callback);
        }


        [TestMethod]
        public void killing_a_service_follows_the_stop_rules_of_the_dependents()
        {
            var parent = new ServiceItem() { ID = Service, Title = "TelegramApi" };

            var child = new ServiceItem()
            {
                ID = Child,
                Title = "TelegramBot",
                Dependencies = new List<Guid> { Service },
                DependencyStopRule = ServiceItem.eDependencyStopRule.Stop
            };

            var bus = new RecordingBus();

            var manager = new ServiceManager.Base.ServiceManager();

            manager.Config = new ServiceConfig();
            manager.Config.ServiceList.Add(parent);
            manager.Config.ServiceList.Add(child);
            manager.Analytics = new Analytics();
            manager.Events = bus;

            manager.KillService(parent);

            Assert.AreEqual(1, bus.Events.Count, "a kill is a stop, so the dependents that go with it follow");

            var e = bus.Events[0];

            Assert.AreEqual(NotificationKind.ServiceStop, e.Kind);
            Assert.AreEqual(Child, e.ServiceId);
        }


        [TestMethod]
        public void stopping_a_service_stops_the_dependents_that_go_with_it()
        {
            var parent = new ServiceItem() { ID = Service, Title = "TelegramApi" };

            var child = new ServiceItem()
            {
                ID = Child,
                Title = "TelegramBot",
                Dependencies = new List<Guid> { Service },
                DependencyStopRule = ServiceItem.eDependencyStopRule.Stop
            };

            var bus = new RecordingBus();

            var manager = new ServiceManager.Base.ServiceManager();

            manager.Config = new ServiceConfig();
            manager.Config.ServiceList.Add(parent);
            manager.Config.ServiceList.Add(child);
            manager.Analytics = new Analytics();
            manager.Events = bus;

            manager.StopService(parent);

            Assert.AreEqual(1, bus.Events.Count, "the stopped service itself raises nothing, it has no process");

            var e = bus.Events[0];

            Assert.AreEqual(NotificationKind.ServiceStop, e.Kind);
            Assert.AreEqual(Child, e.ServiceId);
            Assert.AreEqual("TelegramBot", e.Title);
            Assert.AreEqual(false, child.ForceRestart, "the service would otherwise start itself straight again");
        }


        [TestMethod]
        public void stopping_a_service_leaves_a_dependent_that_stays_running_alone()
        {
            var parent = new ServiceItem() { ID = Service, Title = "TelegramApi" };

            var child = new ServiceItem()
            {
                ID = Child,
                Title = "TelegramBot",
                Dependencies = new List<Guid> { Service },
                DependencyStopRule = ServiceItem.eDependencyStopRule.StayRunning
            };

            var bus = new RecordingBus();

            var manager = new ServiceManager.Base.ServiceManager();

            manager.Config = new ServiceConfig();
            manager.Config.ServiceList.Add(parent);
            manager.Config.ServiceList.Add(child);
            manager.Analytics = new Analytics();
            manager.Events = bus;

            manager.StopService(parent);

            Assert.AreEqual(0, bus.Events.Count, "a dependent that is meant to stay up is not touched");
        }


        [TestMethod]
        public void the_stop_rule_carries_down_a_chain_of_dependents()
        {
            var parent = new ServiceItem() { ID = Service, Title = "TelegramApi" };

            var child = new ServiceItem()
            {
                ID = Child,
                Title = "TelegramBot",
                Dependencies = new List<Guid> { Service },
                DependencyStopRule = ServiceItem.eDependencyStopRule.Stop
            };

            var grandchild = new ServiceItem()
            {
                ID = Grandchild,
                Title = "TelegramSite",
                Dependencies = new List<Guid> { Child },
                DependencyStopRule = ServiceItem.eDependencyStopRule.Stop
            };

            var bus = new RecordingBus();

            var manager = new ServiceManager.Base.ServiceManager();

            manager.Config = new ServiceConfig();
            manager.Config.ServiceList.Add(parent);
            manager.Config.ServiceList.Add(child);
            manager.Config.ServiceList.Add(grandchild);
            manager.Analytics = new Analytics();
            manager.Events = bus;

            manager.StopService(parent);

            Assert.AreEqual(2, bus.Events.Count, "a service below a stopped service goes the same way");
            Assert.AreEqual(Child, bus.Events[0].ServiceId);
            Assert.AreEqual(Grandchild, bus.Events[1].ServiceId);
        }


        [TestMethod]
        public void a_circle_of_stop_rules_stops_every_service_once()
        {
            var parent = new ServiceItem()
            {
                ID = Service,
                Title = "TelegramApi",
                Dependencies = new List<Guid> { Child },
                DependencyStopRule = ServiceItem.eDependencyStopRule.Stop
            };

            var child = new ServiceItem()
            {
                ID = Child,
                Title = "TelegramBot",
                Dependencies = new List<Guid> { Service },
                DependencyStopRule = ServiceItem.eDependencyStopRule.Stop
            };

            var bus = new RecordingBus();

            var manager = new ServiceManager.Base.ServiceManager();

            manager.Config = new ServiceConfig();
            manager.Config.ServiceList.Add(parent);
            manager.Config.ServiceList.Add(child);
            manager.Analytics = new Analytics();
            manager.Events = bus;

            manager.StopService(parent);

            Assert.AreEqual(1, bus.Events.Count, "the service that was stopped is not stopped again through the circle");
            Assert.AreEqual(Child, bus.Events[0].ServiceId);
        }


        [TestMethod]
        public void a_service_that_ends_on_its_own_stops_the_dependents_that_go_with_it()
        {
            var parent = new ServiceItem() { ID = Service, Title = "TelegramApi" };

            var child = new ServiceItem()
            {
                ID = Child,
                Title = "TelegramBot",
                Dependencies = new List<Guid> { Service },
                DependencyStopRule = ServiceItem.eDependencyStopRule.Stop
            };

            var bus = new RecordingBus();

            var manager = new ServiceManager.Base.ServiceManager();

            manager.Config = new ServiceConfig();
            manager.Config.ServiceList.Add(parent);
            manager.Config.ServiceList.Add(child);
            manager.Analytics = new Analytics();
            manager.Events = bus;
            manager.IsRunning = true;

            manager.PrepareService(parent);

            manager.Process_Exited(parent, null);

            Assert.AreEqual(2, bus.Events.Count, "the end of the service and the stop of the service waiting for it");

            Assert.AreEqual(NotificationKind.ServiceCrashed, bus.Events[0].Kind);
            Assert.AreEqual(Service, bus.Events[0].ServiceId);

            Assert.AreEqual(NotificationKind.ServiceStop, bus.Events[1].Kind);
            Assert.AreEqual(Child, bus.Events[1].ServiceId);
        }


        [TestMethod]
        public void a_service_the_manager_has_no_process_for_goes_offline_and_stops_its_dependents()
        {
            var parent = new ServiceItem() { ID = Service, Title = "TelegramApi" };

            var child = new ServiceItem()
            {
                ID = Child,
                Title = "TelegramBot",
                Dependencies = new List<Guid> { Service },
                DependencyStopRule = ServiceItem.eDependencyStopRule.Stop
            };

            var bus = new RecordingBus();

            var manager = new ServiceManager.Base.ServiceManager();

            manager.Config = new ServiceConfig();
            manager.Config.ServiceList.Add(parent);
            manager.Config.ServiceList.Add(child);
            manager.Analytics = new Analytics();
            manager.Events = bus;
            manager.IsRunning = true;

            manager.PrepareService(parent);
            manager.PrepareService(child);

            var parentAnalytics = manager.Analytics.Services.First(a => a.ServiceID == Service);
            var childAnalytics = manager.Analytics.Services.First(a => a.ServiceID == Child);

            parentAnalytics.ProcessID = 4242;
            parentAnalytics.Status = ServiceAnalytics.eStatus.running;

            childAnalytics.ProcessID = 4243;
            childAnalytics.Status = ServiceAnalytics.eStatus.running;
            childAnalytics.WorkingSetBytes = 2048;

            manager.StopService(parent);

            Assert.AreEqual(ServiceAnalytics.eStatus.offline, parentAnalytics.Status, "a service with no process left to end is not running anymore");
            Assert.AreEqual(ServiceAnalytics.eStatus.offline, childAnalytics.Status, "the service waiting for it goes the same way");
            Assert.IsNull(childAnalytics.WorkingSetBytes, "what was measured on a process that is gone is handed back");

            Assert.AreEqual(1, bus.Events.Count, "the dependent is stopped through the rules of its own");
            Assert.AreEqual(NotificationKind.ServiceStop, bus.Events[0].Kind);
            Assert.AreEqual(Child, bus.Events[0].ServiceId);
        }


        [TestMethod]
        public void a_stop_the_manager_asked_for_does_not_run_the_rules_a_second_time()
        {
            var parent = new ServiceItem() { ID = Service, Title = "TelegramApi" };

            var child = new ServiceItem()
            {
                ID = Child,
                Title = "TelegramBot",
                Dependencies = new List<Guid> { Service },
                DependencyStopRule = ServiceItem.eDependencyStopRule.Stop
            };

            var bus = new RecordingBus();

            var manager = new ServiceManager.Base.ServiceManager();

            manager.Config = new ServiceConfig();
            manager.Config.ServiceList.Add(parent);
            manager.Config.ServiceList.Add(child);
            manager.Analytics = new Analytics();
            manager.Events = bus;
            manager.IsRunning = true;

            manager.PrepareService(parent);

            //The stop is asked for, so its rules travel with StopService; the process only reports in afterwards
            manager.ServiceEnd(parent);
            manager.Process_Exited(parent, null);

            Assert.AreEqual(1, bus.Events.Count, "the dependent is stopped once, not once more by the report of the process");
            Assert.AreEqual(Service, bus.Events[0].ServiceId);
        }


        [TestMethod]
        public void the_shutdown_of_the_instance_does_not_run_the_stop_rules()
        {
            var parent = new ServiceItem() { ID = Service, Title = "TelegramApi" };

            var child = new ServiceItem()
            {
                ID = Child,
                Title = "TelegramBot",
                Dependencies = new List<Guid> { Service },
                DependencyStopRule = ServiceItem.eDependencyStopRule.Stop
            };

            var bus = new RecordingBus();

            var manager = new ServiceManager.Base.ServiceManager();

            manager.Config = new ServiceConfig();
            manager.Config.ServiceList.Add(parent);
            manager.Config.ServiceList.Add(child);
            manager.Analytics = new Analytics();
            manager.Events = bus;

            manager.PrepareService(parent);

            manager.Process_Exited(parent, null);

            Assert.AreEqual(1, bus.Events.Count, "the instance goes down on its own, the stop levels of the shutdown do the work");
            Assert.AreEqual(Service, bus.Events[0].ServiceId);
        }


        [TestMethod]
        public void a_service_held_back_for_a_dependency_leaves_a_warning_in_the_log()
        {
            var item = new ServiceItem()
            {
                ID = Service,
                Title = "TelegramApi",
                WindowsDependencies = new List<String>() { "Dhcp" }
            };

            var manager = new ServiceManager.Base.ServiceManager();

            manager.Config = new ServiceConfig();
            manager.Analytics = new Analytics();
            manager.WindowsStatus = new FakeStatus();
            manager.Events = new RecordingBus();

            manager.StartService(item);

            var lines = File.ReadAllLines(SMLog.GetFiles()[0]);

            Assert.AreEqual(1, lines.Length, "an event is logged once, not once per provider");
            Assert.IsTrue(lines[0].Contains("] [WARN] Service Manager: TelegramApi | Status: Waiting for Dhcp"), lines[0]);
        }


        [TestMethod]
        public void a_service_that_ended_on_its_own_leaves_the_crash_in_the_log()
        {
            var item = new ServiceItem()
            {
                ID = Service,
                Title = "TelegramApi"
            };

            var manager = new ServiceManager.Base.ServiceManager();

            manager.Config = new ServiceConfig();
            manager.Config.ServiceList.Add(item);
            manager.Analytics = new Analytics();
            manager.Events = new RecordingBus();

            manager.PrepareService(item);

            manager.Process_Exited(item, null);

            var lines = File.ReadAllLines(SMLog.GetFiles()[0]);

            Assert.AreEqual(1, lines.Length, "the same line is not written for every provider");
            Assert.IsTrue(lines[0].Contains("] [ERROR] Service Manager: TelegramApi | Status: Crashed"), lines[0]);
        }


        [TestMethod]
        public void a_service_the_manager_stopped_leaves_an_information_in_the_log()
        {
            var item = new ServiceItem()
            {
                ID = Service,
                Title = "TelegramApi"
            };

            var manager = new ServiceManager.Base.ServiceManager();

            manager.Config = new ServiceConfig();
            manager.Config.ServiceList.Add(item);
            manager.Analytics = new Analytics();
            manager.Events = new RecordingBus();

            manager.PrepareService(item);

            manager.StopRequests.Mark(item.ID);

            manager.Process_Exited(item, null);

            var lines = File.ReadAllLines(SMLog.GetFiles()[0]);

            Assert.AreEqual(1, lines.Length);
            Assert.IsTrue(lines[0].Contains("] [INFO] Service Manager: TelegramApi | Status: Stopped"), lines[0]);
        }


        [TestMethod]
        public void a_console_line_stays_out_of_the_service_log()
        {
            ServiceManager.Base.ServiceManager.LogEvent(NotificationKind.ConsoleOutput, "Service Manager: TelegramApi | a line of console output");
            ServiceManager.Base.ServiceManager.LogEvent(NotificationKind.ConsoleError, "Service Manager: TelegramApi | a line of console errors");

            Assert.AreEqual(0, SMLog.GetFiles().Count, "every service has its own console capture, the log has to stay readable");
        }


        [TestMethod]
        public void the_manager_itself_leaves_an_information_when_it_starts()
        {
            ServiceManager.Base.ServiceManager.LogEvent(NotificationKind.ServerStart, "Service Manager: starting");

            var lines = File.ReadAllLines(SMLog.GetFiles()[0]);

            Assert.AreEqual(1, lines.Length);
            Assert.IsTrue(lines[0].Contains("] [INFO] Service Manager: starting"), lines[0]);
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
