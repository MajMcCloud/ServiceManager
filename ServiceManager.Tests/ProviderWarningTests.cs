using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceManager.Extensions;
using ServiceManager.Extensions.Interfaces;
using ServiceManager.UI.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServiceManager.Tests
{
    /// <summary>
    /// The sentence the settings dialog shows about a provider that cannot deliver. What counts as
    /// missing is answered by the provider itself, so these tests only cover the answers a provider
    /// can give and the one line the dialog makes out of them.
    /// </summary>
    [TestClass]
    public class ProviderWarningTests
    {
        private sealed class FakeCheck : INotificationSettingsCheck
        {
            private readonly IList<String> __missing;

            private readonly bool __failing;

            private FakeCheck(IList<String> missing, bool failing)
            {
                __missing = missing;
                __failing = failing;
            }

            public static FakeCheck Reporting(params String[] missing)
            {
                return new FakeCheck(new List<String>(missing), false);
            }

            public static FakeCheck AnsweringNull()
            {
                return new FakeCheck(null, false);
            }

            public static FakeCheck Failing()
            {
                return new FakeCheck(null, true);
            }

            public IList<String> MissingSettings()
            {
                if (__failing)
                    throw new InvalidOperationException("fixture provider failed");

                return __missing;
            }
        }


        private static Extension Loaded(INotificationSettingsCheck check)
        {
            return new Extension() { Id = Guid.NewGuid(), Name = "Test provider", SettingsCheck = check };
        }


        [TestMethod]
        public void missing_names_every_field_the_provider_reports()
        {
            var missing = ProviderWarning.Missing(Loaded(FakeCheck.Reporting("SMTP server", "Recipient(s)")));

            CollectionAssert.AreEqual(new String[] { "SMTP server", "Recipient(s)" }, missing.ToArray());
        }


        [TestMethod]
        public void missing_stays_empty_for_a_provider_that_reports_nothing()
        {
            Assert.AreEqual(0, ProviderWarning.Missing(Loaded(FakeCheck.Reporting())).Count);
        }


        [TestMethod]
        public void missing_stays_empty_for_a_provider_that_answers_with_null()
        {
            Assert.AreEqual(0, ProviderWarning.Missing(Loaded(FakeCheck.AnsweringNull())).Count);
        }


        [TestMethod]
        public void missing_stays_empty_for_a_provider_that_does_not_answer_the_question()
        {
            Assert.AreEqual(0, ProviderWarning.Missing(Loaded(null)).Count, "an extension written before the interface reads as configured");
        }


        [TestMethod]
        public void missing_stays_empty_for_a_provider_that_was_never_loaded()
        {
            Assert.AreEqual(0, ProviderWarning.Missing(null).Count, "a provider of the settings file whose assembly is gone has nothing to report");
        }


        [TestMethod]
        public void missing_stays_empty_for_a_provider_that_throws_while_answering()
        {
            Assert.AreEqual(0, ProviderWarning.Missing(Loaded(FakeCheck.Failing())).Count, "a broken provider must not take the settings dialog with it");
        }


        [TestMethod]
        public void text_names_the_provider_and_every_missing_field()
        {
            var missing = ProviderWarning.Missing(Loaded(FakeCheck.Reporting("SMTP server", "Recipient(s)")));

            Assert.AreEqual("E-Mail (SMTP) is not configured: SMTP server, Recipient(s).", ProviderWarning.Text("E-Mail (SMTP)", missing));
        }


        [TestMethod]
        public void text_stays_empty_when_nothing_is_missing()
        {
            Assert.AreEqual("", ProviderWarning.Text("Script hook", new String[0]));
            Assert.AreEqual("", ProviderWarning.Text("Script hook", null));
            Assert.AreEqual("", ProviderWarning.Text(null, new String[] { "Program" }));
        }
    }
}
