using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using ServiceManager.Base.Data;
using System;
using System.Collections.Generic;

namespace ServiceManager.Tests
{
    /// <summary>
    /// A service carries its Windows dependencies inside the settings file, so a fresh item has to be
    /// complete and an older file has to end up with the defaults of the new properties.
    /// </summary>
    [TestClass]
    public class ServiceItemTests
    {
        [TestMethod]
        public void serviceItem_carries_an_empty_dependency_list()
        {
            var item = new ServiceItem();

            Assert.IsNotNull(item.WindowsDependencies);
            Assert.AreEqual(0, item.WindowsDependencies.Count);
        }

        [TestMethod]
        public void serviceItem_waits_for_its_dependencies_after_a_restart_by_default()
        {
            var item = new ServiceItem();

            Assert.IsTrue(item.Wait_On_Restart);
        }

        [TestMethod]
        public void dependencies_survive_the_settings_file()
        {
            var item = new ServiceItem();

            item.Title = "Telegram bot";
            item.WindowsDependencies = new List<String> { "Tcpip", "Dnscache" };
            item.Wait_On_Restart = false;

            var again = JsonConvert.DeserializeObject<ServiceItem>(JsonConvert.SerializeObject(item));

            CollectionAssert.AreEqual(new List<String> { "Tcpip", "Dnscache" }, again.WindowsDependencies);
            Assert.IsFalse(again.Wait_On_Restart);
        }

        [TestMethod]
        public void older_settings_without_dependencies_keep_the_defaults()
        {
            var again = JsonConvert.DeserializeObject<ServiceItem>("{\"Title\":\"Telegram bot\",\"ForceRestart\":true}");

            Assert.IsNotNull(again.WindowsDependencies, "the start path reads the list without checking it");
            Assert.AreEqual(0, again.WindowsDependencies.Count);
            Assert.IsTrue(again.Wait_On_Restart);
        }
    }
}
