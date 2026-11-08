using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using ServiceManager.Extensions.Interfaces;
using ServiceManager.Extensions.ScriptNotifier;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace ServiceManager.Tests
{
    /// <summary>
    /// The json object a script gets on its input. Everything here runs without a program: what matters
    /// is that the field names are the ones of the webhook provider and that a console line of any kind
    /// still leaves the object parseable.
    /// </summary>
    [TestClass]
    public class ScriptPayloadTests
    {
        private static NotificationMessage Event(NotificationKind kind, String text = "Service stopped")
        {
            return new NotificationMessage()
            {
                Kind = kind,
                Timestamp = new DateTime(2026, 3, 4, 5, 6, 7),
                InstanceName = "HomeLab",
                ServiceTitle = "Backup",
                Text = text,
                Details = "exit code 3"
            };
        }


        [TestMethod]
        public void the_object_carries_every_field_of_the_event()
        {
            var json = JObject.Parse(ScriptPayload.ToJson(Event(NotificationKind.ServiceStop)));

            Assert.AreEqual("HomeLab", (String)json["instance"]);
            Assert.AreEqual("ServiceStop", (String)json["event"]);
            Assert.AreEqual("warning", (String)json["level"]);
            Assert.AreEqual("Backup", (String)json["service"]);
            Assert.AreEqual("Service stopped", (String)json["text"]);
            Assert.AreEqual("exit code 3", (String)json["details"]);
        }


        /// <summary>
        /// A script that reads the fields of the webhook provider should not need a second mapping for
        /// this one, so both providers hand out the same names.
        /// </summary>
        [TestMethod]
        public void the_field_names_are_the_names_of_the_webhook_provider()
        {
            var json = JObject.Parse(ScriptPayload.ToJson(Event(NotificationKind.ConsoleError)));

            String[] expected = { "instance", "event", "level", "service", "time", "text", "details" };

            foreach (var name in expected)
                Assert.IsNotNull(json[name], "no field named " + name);

            Assert.AreEqual(expected.Length, json.Properties().Count(), "the script provider invented a field of its own");
        }


        /// <summary>
        /// Both providers rate an event the same way, so a script does not have to know which one of
        /// them handed it the event.
        /// </summary>
        [TestMethod]
        public void every_event_gets_the_level_of_the_other_providers()
        {
            Assert.AreEqual("error", ScriptPayload.Level(NotificationKind.ConsoleError));

            Assert.AreEqual("warning", ScriptPayload.Level(NotificationKind.ServiceStop));
            Assert.AreEqual("warning", ScriptPayload.Level(NotificationKind.ServiceRestart));

            Assert.AreEqual("info", ScriptPayload.Level(NotificationKind.ServerStart));
            Assert.AreEqual("info", ScriptPayload.Level(NotificationKind.ServerShutdown));
            Assert.AreEqual("info", ScriptPayload.Level(NotificationKind.ServiceStart));
            Assert.AreEqual("info", ScriptPayload.Level(NotificationKind.ConsoleOutput));
        }


        [TestMethod]
        public void the_time_is_sortable_and_names_the_zone()
        {
            // The text as it leaves the provider: a parsed token would already be a date by the time
            // the assertion runs, which says nothing about the form on the wire.
            var sent = ScriptPayload.ToJson(Event(NotificationKind.ServiceStart));

            Assert.IsTrue(Regex.IsMatch(sent, "\"time\":\"\\d{4}-\\d{2}-\\d{2}T\\d{2}:\\d{2}:\\d{2}[+-]\\d{2}:\\d{2}\""), "sent: " + sent);
        }


        /// <summary>
        /// A script that pipes the object into jq does not want to check every field for null first.
        /// </summary>
        [TestMethod]
        public void an_event_without_fields_sends_empty_text()
        {
            var json = JObject.Parse(ScriptPayload.ToJson(new NotificationMessage()
            {
                Kind = NotificationKind.ConsoleOutput,
                Timestamp = DateTime.Now,
                Text = "only text"
            }));

            Assert.AreEqual("", (String)json["instance"]);
            Assert.AreEqual("", (String)json["service"]);
            Assert.AreEqual("", (String)json["details"]);
            Assert.AreEqual("only text", (String)json["text"]);
        }


        /// <summary>
        /// The object travels as one line, either on the input of the program or as one argument. A
        /// pretty printed object would be read as several arguments by a batch file.
        /// </summary>
        [TestMethod]
        public void the_object_is_one_line()
        {
            var json = ScriptPayload.ToJson(Event(NotificationKind.ConsoleError, "line one\nline two \"quoted\""));

            Assert.IsFalse(json.Contains("\r"));
            Assert.IsFalse(json.Contains("\n"));

            Assert.AreEqual("line one\nline two \"quoted\"", (String)JObject.Parse(json)["text"]);
        }
    }
}
