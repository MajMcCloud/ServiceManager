using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceManager.Extensions.WebhookNotifier;
using System;
using System.Linq;
using WebhookConfig = ServiceManager.Extensions.WebhookNotifier.Config.Config;

namespace ServiceManager.Tests
{
    /// <summary>
    /// The provider class itself. The body of a message is the subject of WebhookPayloadTests, this
    /// class only carries the question of what the settings dialog still shows as empty.
    /// </summary>
    [TestClass]
    public class WebhookNotifierTests
    {
        [TestMethod]
        public void missing_settings_names_the_url_of_the_settings_dialog()
        {
            var missing = WebhookNotifier.MissingSettings(new WebhookConfig());

            CollectionAssert.AreEqual(new String[] { "URL" }, missing.ToArray());
        }


        [TestMethod]
        public void missing_settings_stays_empty_once_the_url_is_filled_in()
        {
            var missing = WebhookNotifier.MissingSettings(new WebhookConfig() { Url = "https://hook.internal/events" });

            Assert.AreEqual(0, missing.Count, "the body template stays optional");
        }


        [TestMethod]
        public void missing_settings_names_the_url_when_no_settings_file_was_read()
        {
            CollectionAssert.AreEqual(new String[] { "URL" }, WebhookNotifier.MissingSettings(null).ToArray());
        }
    }
}
