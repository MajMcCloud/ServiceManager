using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceManager.Base.Data.Settings;
using ServiceManager.Base.Windows;

namespace ServiceManager.Tests
{
    [TestClass]
    public class WindowsServiceQueryTests
    {
        [TestMethod]
        public void is_startable_is_true_for_an_automatic_service()
        {
            WindowsServiceInfo info = create("svc", "A service", WindowsServiceInfo.eStartType.Automatic);

            Assert.IsTrue(WindowsServiceQuery.IsStartable(info));
        }

        [TestMethod]
        public void is_startable_is_true_for_a_manual_service()
        {
            WindowsServiceInfo info = create("svc", "A service", WindowsServiceInfo.eStartType.Manual);

            Assert.IsTrue(WindowsServiceQuery.IsStartable(info));
        }

        [TestMethod]
        public void is_startable_is_false_for_a_disabled_service()
        {
            WindowsServiceInfo info = create("svc", "A service", WindowsServiceInfo.eStartType.Disabled);

            Assert.IsFalse(WindowsServiceQuery.IsStartable(info));
        }

        [TestMethod]
        public void is_startable_is_false_for_a_driver_loaded_at_boot()
        {
            WindowsServiceInfo info = create("Tcpip", "TCP/IP", WindowsServiceInfo.eStartType.Boot);

            Assert.IsFalse(WindowsServiceQuery.IsStartable(info));
        }

        [TestMethod]
        public void is_startable_is_false_without_a_service()
        {
            Assert.IsFalse(WindowsServiceQuery.IsStartable(null));
        }

        [TestMethod]
        public void empty_filter_keeps_every_service()
        {
            WindowsServiceInfo info = create("svc", "A service", WindowsServiceInfo.eStartType.Automatic);

            Assert.IsTrue(WindowsServiceQuery.Matches(info, "   "));
        }

        [TestMethod]
        public void filter_matches_the_display_name()
        {
            WindowsServiceInfo info = create("Dnscache", "DNS Client", WindowsServiceInfo.eStartType.Automatic);

            Assert.IsTrue(WindowsServiceQuery.Matches(info, "dns cli"));
        }

        [TestMethod]
        public void filter_matches_the_service_name()
        {
            WindowsServiceInfo info = create("Dnscache", "DNS Client", WindowsServiceInfo.eStartType.Automatic);

            Assert.IsTrue(WindowsServiceQuery.Matches(info, "dnscache"));
        }

        [TestMethod]
        public void filter_ignores_case_and_surrounding_spaces()
        {
            WindowsServiceInfo info = create("Dnscache", "DNS Client", WindowsServiceInfo.eStartType.Automatic);

            Assert.IsTrue(WindowsServiceQuery.Matches(info, "  client  "));
        }

        [TestMethod]
        public void filter_without_a_match_returns_false()
        {
            WindowsServiceInfo info = create("Dnscache", "DNS Client", WindowsServiceInfo.eStartType.Automatic);

            Assert.IsFalse(WindowsServiceQuery.Matches(info, "spooler"));
        }

        [TestMethod]
        public void select_drops_the_services_the_manager_cannot_start()
        {
            List<WindowsServiceInfo> all = new List<WindowsServiceInfo>();

            all.Add(create("one", "One", WindowsServiceInfo.eStartType.Automatic));
            all.Add(create("two", "Two", WindowsServiceInfo.eStartType.Disabled));
            all.Add(create("three", "Three", WindowsServiceInfo.eStartType.System));

            List<WindowsServiceInfo> shown = WindowsServiceQuery.Select(all, false);

            Assert.AreEqual(1, shown.Count);
            Assert.AreEqual("one", shown[0].ServiceName);
        }

        [TestMethod]
        public void select_adds_the_network_stack_when_it_is_asked_for()
        {
            List<WindowsServiceInfo> all = new List<WindowsServiceInfo>();

            all.Add(create("one", "One", WindowsServiceInfo.eStartType.Automatic));

            List<WindowsServiceInfo> shown = WindowsServiceQuery.Select(all, true);

            Assert.AreEqual(WindowsService.StandardNetworkDependencies.Length + 1, shown.Count);
            Assert.IsTrue(shown.Any(m => m.ServiceName == "Tcpip"));
        }

        [TestMethod]
        public void select_leaves_out_the_network_stack_when_it_is_not_asked_for()
        {
            List<WindowsServiceInfo> all = new List<WindowsServiceInfo>();

            all.Add(create("one", "One", WindowsServiceInfo.eStartType.Automatic));

            List<WindowsServiceInfo> shown = WindowsServiceQuery.Select(all, false);

            Assert.IsFalse(shown.Any(m => m.ServiceName == "Tcpip"));
        }

        [TestMethod]
        public void select_does_not_add_a_network_service_twice()
        {
            List<WindowsServiceInfo> all = new List<WindowsServiceInfo>();

            all.Add(create("Dhcp", "DHCP Client", WindowsServiceInfo.eStartType.Automatic));

            List<WindowsServiceInfo> shown = WindowsServiceQuery.Select(all, true);

            Assert.AreEqual(WindowsService.StandardNetworkDependencies.Length, shown.Count);
            Assert.AreEqual(1, shown.Count(m => String.Equals(m.ServiceName, "Dhcp", StringComparison.OrdinalIgnoreCase)));
        }

        [TestMethod]
        public void select_keeps_the_title_of_a_network_driver_it_cannot_start()
        {
            List<WindowsServiceInfo> all = new List<WindowsServiceInfo>();

            all.Add(create("Tcpip", "TCP/IP Protocol Driver", WindowsServiceInfo.eStartType.Boot));

            List<WindowsServiceInfo> shown = WindowsServiceQuery.Select(all, true);

            Assert.AreEqual("TCP/IP Protocol Driver", shown.First(m => m.ServiceName == "Tcpip").DisplayName);
        }

        [TestMethod]
        public void select_orders_the_list_by_display_name()
        {
            List<WindowsServiceInfo> all = new List<WindowsServiceInfo>();

            all.Add(create("c", "Charlie", WindowsServiceInfo.eStartType.Automatic));
            all.Add(create("a", "alpha", WindowsServiceInfo.eStartType.Manual));
            all.Add(create("b", "Bravo", WindowsServiceInfo.eStartType.Automatic));

            List<WindowsServiceInfo> shown = WindowsServiceQuery.Select(all, false);

            CollectionAssert.AreEqual(new[] { "a", "b", "c" }, shown.Select(m => m.ServiceName).ToArray());
        }

        [TestMethod]
        public void select_without_a_list_returns_an_empty_list()
        {
            Assert.AreEqual(0, WindowsServiceQuery.Select(null, false).Count);
        }

        [TestMethod]
        public void registry_start_value_maps_onto_the_start_types()
        {
            Assert.AreEqual(WindowsServiceInfo.eStartType.Boot, WindowsServiceQuery.ToStartType(0));
            Assert.AreEqual(WindowsServiceInfo.eStartType.System, WindowsServiceQuery.ToStartType(1));
            Assert.AreEqual(WindowsServiceInfo.eStartType.Automatic, WindowsServiceQuery.ToStartType(2));
            Assert.AreEqual(WindowsServiceInfo.eStartType.Manual, WindowsServiceQuery.ToStartType(3));
            Assert.AreEqual(WindowsServiceInfo.eStartType.Disabled, WindowsServiceQuery.ToStartType(4));
        }

        [TestMethod]
        public void unknown_registry_start_value_stays_unknown()
        {
            Assert.AreEqual(WindowsServiceInfo.eStartType.Unknown, WindowsServiceQuery.ToStartType(-1));
        }

        [TestMethod]
        public void service_controller_start_mode_maps_onto_the_start_types()
        {
            Assert.AreEqual(WindowsServiceInfo.eStartType.Automatic, WindowsServiceQuery.ToStartType(ServiceStartMode.Automatic));
            Assert.AreEqual(WindowsServiceInfo.eStartType.Manual, WindowsServiceQuery.ToStartType(ServiceStartMode.Manual));
            Assert.AreEqual(WindowsServiceInfo.eStartType.Boot, WindowsServiceQuery.ToStartType(ServiceStartMode.Boot));
        }

        [TestMethod]
        public void status_text_names_the_state_in_english()
        {
            Assert.AreEqual("Running", WindowsServiceQuery.StatusText(ServiceControllerStatus.Running));
            Assert.AreEqual("Stopped", WindowsServiceQuery.StatusText(ServiceControllerStatus.Stopped));
        }

        [TestMethod]
        public void start_type_text_names_the_type_in_english()
        {
            Assert.AreEqual("Automatic", WindowsServiceQuery.StartTypeText(WindowsServiceInfo.eStartType.Automatic));
            Assert.AreEqual("Unknown", WindowsServiceQuery.StartTypeText(WindowsServiceInfo.eStartType.Unknown));
        }

        [TestMethod]
        public void a_title_reference_is_recognised_as_indirect_string()
        {
            Assert.IsTrue(WindowsServiceQuery.IsIndirectString(@"@%SystemRoot%\system32\drivers\tcpip.sys,-10001"));
        }

        [TestMethod]
        public void a_plain_title_is_not_an_indirect_string()
        {
            Assert.IsFalse(WindowsServiceQuery.IsIndirectString("DHCP Client"));
        }

        [TestMethod]
        public void indirect_string_check_survives_a_missing_value()
        {
            Assert.IsFalse(WindowsServiceQuery.IsIndirectString(null));
        }

        static WindowsServiceInfo create(String serviceName, String displayName, WindowsServiceInfo.eStartType startType)
        {
            WindowsServiceInfo info = new WindowsServiceInfo();

            info.ServiceName = serviceName;
            info.DisplayName = displayName;
            info.StartType = startType;

            return info;
        }
    }
}
