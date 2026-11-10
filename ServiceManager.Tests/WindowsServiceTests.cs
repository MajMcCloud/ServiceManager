using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using ServiceManager.Base.Data;
using ServiceManager.Base.Data.Settings;

namespace ServiceManager.Tests
{
    [TestClass]
    public class WindowsServiceTests
    {
        [TestMethod]
        public void default_start_mode_is_automatic()
        {
            WindowsService settings = new WindowsService();

            Assert.AreEqual(WindowsService.eStartMode.Automatic, settings.StartMode);
        }

        [TestMethod]
        public void default_account_is_local_system()
        {
            WindowsService settings = new WindowsService();

            Assert.AreEqual(WindowsService.eAccount.LocalSystem, settings.Account);
        }

        [TestMethod]
        public void default_dependency_mode_is_standard_network()
        {
            WindowsService settings = new WindowsService();

            Assert.AreEqual(WindowsService.eDependencyMode.StandardNetwork, settings.DependenciesMode);
        }

        [TestMethod]
        public void default_custom_dependency_list_is_empty()
        {
            WindowsService settings = new WindowsService();

            Assert.AreEqual(0, settings.CustomDependencies.Count);
        }

        [TestMethod]
        public void get_dependencies_returns_the_network_stack_by_default()
        {
            WindowsService settings = new WindowsService();

            CollectionAssert.AreEqual(new List<String> { "Tcpip", "Dhcp", "Dnscache" }, settings.GetDependencies());
        }

        [TestMethod]
        public void get_dependencies_returns_no_entry_for_none()
        {
            WindowsService settings = new WindowsService();
            settings.DependenciesMode = WindowsService.eDependencyMode.None;

            Assert.AreEqual(0, settings.GetDependencies().Length);
        }

        [TestMethod]
        public void get_dependencies_returns_the_custom_list_for_custom()
        {
            WindowsService settings = new WindowsService();
            settings.DependenciesMode = WindowsService.eDependencyMode.Custom;
            settings.CustomDependencies = new List<String> { "LanmanServer", "TermService" };

            CollectionAssert.AreEqual(new List<String> { "LanmanServer", "TermService" }, settings.GetDependencies());
        }

        [TestMethod]
        public void get_dependencies_skips_blank_custom_entries()
        {
            WindowsService settings = new WindowsService();
            settings.DependenciesMode = WindowsService.eDependencyMode.Custom;
            settings.CustomDependencies = new List<String> { "LanmanServer", "  ", String.Empty };

            CollectionAssert.AreEqual(new List<String> { "LanmanServer" }, settings.GetDependencies());
        }

        [TestMethod]
        public void get_dependencies_trims_custom_entries()
        {
            WindowsService settings = new WindowsService();
            settings.DependenciesMode = WindowsService.eDependencyMode.Custom;
            settings.CustomDependencies = new List<String> { "  LanmanServer  " };

            CollectionAssert.AreEqual(new List<String> { "LanmanServer" }, settings.GetDependencies());
        }

        [TestMethod]
        public void custom_dependency_names_are_distinct_ignoring_case()
        {
            WindowsService settings = new WindowsService();
            settings.DependenciesMode = WindowsService.eDependencyMode.Custom;
            settings.CustomDependencies = new List<String> { "  Dhcp", "dhcp ", "Dnscache" };

            CollectionAssert.AreEqual(new[] { "Dhcp", "Dnscache" }, settings.GetDependencies());
        }

        [TestMethod]
        public void editing_returned_network_dependencies_preserves_the_defaults()
        {
            WindowsService settings = new WindowsService();
            settings.GetDependencies()[0] = "Changed";

            CollectionAssert.AreEqual(new[] { "Tcpip", "Dhcp", "Dnscache" }, settings.GetDependencies());
        }

        [TestMethod]
        public void null_custom_dependency_list_is_empty()
        {
            WindowsService settings = new WindowsService();
            settings.DependenciesMode = WindowsService.eDependencyMode.Custom;
            settings.CustomDependencies = null;

            Assert.IsEmpty(settings.GetDependencies());
        }

        [TestMethod]
        public void invalid_dependency_mode_reports_an_error()
        {
            WindowsService settings = new WindowsService();
            settings.DependenciesMode = (WindowsService.eDependencyMode)99;

            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => settings.GetDependencies());
        }

        [TestMethod]
        public void invalid_account_reports_an_error()
        {
            WindowsService settings = new WindowsService();
            settings.Account = (WindowsService.eAccount)99;

            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => settings.GetAccountName());
        }

        [TestMethod]
        public void get_account_name_returns_local_system()
        {
            WindowsService settings = new WindowsService();
            settings.Account = WindowsService.eAccount.LocalSystem;

            Assert.AreEqual("LocalSystem", settings.GetAccountName());
        }

        [TestMethod]
        public void get_account_name_returns_local_service()
        {
            WindowsService settings = new WindowsService();
            settings.Account = WindowsService.eAccount.LocalService;

            Assert.AreEqual(@"NT AUTHORITY\LocalService", settings.GetAccountName());
        }

        [TestMethod]
        public void get_account_name_returns_network_service()
        {
            WindowsService settings = new WindowsService();
            settings.Account = WindowsService.eAccount.NetworkService;

            Assert.AreEqual(@"NT AUTHORITY\NetworkService", settings.GetAccountName());
        }

        [TestMethod]
        public void get_account_name_returns_the_user_name_for_a_user_account()
        {
            WindowsService settings = new WindowsService();
            settings.Account = WindowsService.eAccount.User;
            settings.UserName = "  CORP\\ServiceAccount  ";

            Assert.AreEqual(@"CORP\ServiceAccount", settings.GetAccountName());
        }

        [TestMethod]
        public void get_account_name_is_null_for_a_user_account_without_a_name()
        {
            WindowsService settings = new WindowsService();
            settings.Account = WindowsService.eAccount.User;
            settings.UserName = "   ";

            Assert.IsNull(settings.GetAccountName());
        }

        [TestMethod]
        public void uses_password_is_only_true_for_a_user_account()
        {
            WindowsService settings = new WindowsService();
            settings.Account = WindowsService.eAccount.NetworkService;

            Assert.IsFalse(settings.UsesPassword());

            settings.Account = WindowsService.eAccount.User;

            Assert.IsTrue(settings.UsesPassword());
        }

        [TestMethod]
        public void json_round_trip_keeps_every_value()
        {
            WindowsService settings = new WindowsService();
            settings.StartMode = WindowsService.eStartMode.AutomaticDelayed;
            settings.Account = WindowsService.eAccount.User;
            settings.UserName = @"CORP\ServiceAccount";
            settings.Password = "s3cret";
            settings.DependenciesMode = WindowsService.eDependencyMode.Custom;
            settings.CustomDependencies = new List<String> { "LanmanServer" };

            WindowsService restored = JsonConvert.DeserializeObject<WindowsService>(JsonConvert.SerializeObject(settings));

            Assert.AreEqual(WindowsService.eStartMode.AutomaticDelayed, restored.StartMode);
            Assert.AreEqual(WindowsService.eAccount.User, restored.Account);
            Assert.AreEqual(@"CORP\ServiceAccount", restored.UserName);
            Assert.AreEqual("s3cret", restored.Password);
            Assert.AreEqual(WindowsService.eDependencyMode.Custom, restored.DependenciesMode);
            CollectionAssert.AreEqual(settings.CustomDependencies, restored.CustomDependencies);
        }

        [TestMethod]
        public void a_config_without_the_service_section_keeps_the_defaults()
        {
            ServiceConfig config = JsonConvert.DeserializeObject<ServiceConfig>("{\"ServiceList\":[]}");

            Assert.IsNotNull(config.WindowsSettings);
            Assert.AreEqual(WindowsService.eStartMode.Automatic, config.WindowsSettings.StartMode);
            Assert.AreEqual(WindowsService.eAccount.LocalSystem, config.WindowsSettings.Account);
            Assert.AreEqual(WindowsService.eDependencyMode.StandardNetwork, config.WindowsSettings.DependenciesMode);
        }
    }
}
