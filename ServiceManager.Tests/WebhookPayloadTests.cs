using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using ServiceManager.Extensions.Interfaces;
using ServiceManager.Extensions.WebhookNotifier;
using System;
using System.Text.RegularExpressions;

namespace ServiceManager.Tests
{
    /// <summary>
    /// The body a webhook receiver gets. Everything here runs without an endpoint: what matters is
    /// that a console line can never break the body and that a template written for another tool
    /// still arrives as it was written.
    /// </summary>
    [TestClass]
    public class WebhookPayloadTests
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
        public void standard_body_carries_every_field_of_the_event()
        {
            var body = WebhookPayload.Render(null, Event(NotificationKind.ServiceStop), true);

            var json = JObject.Parse(body);

            Assert.AreEqual("HomeLab", (String)json["instance"]);
            Assert.AreEqual("ServiceStop", (String)json["event"]);
            Assert.AreEqual("warning", (String)json["level"]);
            Assert.AreEqual("Backup", (String)json["service"]);
            Assert.AreEqual("Service stopped", (String)json["text"]);
            Assert.AreEqual("exit code 3", (String)json["details"]);
        }


        /// <summary>
        /// The body of a webhook is JSON, so it starts with a brace itself. Scanning for the next
        /// closing brace would eat the placeholder of "{ \"text\": \"{text}\" }" and send it empty.
        /// </summary>
        [TestMethod]
        public void a_json_body_keeps_its_own_braces_and_still_fills_the_placeholders()
        {
            var body = WebhookPayload.Render("{\"text\": \"{text}\"}", Event(NotificationKind.ConsoleError, "hello"), true);

            Assert.AreEqual("{\"text\": \"hello\"}", body);
        }


        /// <summary>
        /// A console line is whatever the program printed. With a quote or a backslash in it an
        /// unescaped body stops being JSON and the receiver answers with a parse error.
        /// </summary>
        [TestMethod]
        public void a_console_line_cannot_break_the_body()
        {
            var printed = "quote \" backslash \\ newline\nand a tab\t";

            var body = WebhookPayload.Render("{\"text\": \"{text}\"}", Event(NotificationKind.ConsoleError, printed), true);

            Assert.AreEqual(printed, (String)JObject.Parse(body)["text"]);
        }


        /// <summary>
        /// A receiver that is told the body is plain text wants the line as it is, escaping would
        /// put a literal backslash n into the message.
        /// </summary>
        [TestMethod]
        public void a_plain_text_body_gets_the_values_untouched()
        {
            var body = WebhookPayload.Render("event {kind}: {text}", Event(NotificationKind.ConsoleError, "line1\nline2 \"q\""), false);

            Assert.AreEqual("event ConsoleError: line1\nline2 \"q\"", body);
        }


        /// <summary>
        /// Templates are written for Slack, Discord, n8n or a flow of our own. A variable the
        /// provider does not know belongs to the receiver and stays untouched.
        /// </summary>
        [TestMethod]
        public void a_placeholder_of_another_tool_survives()
        {
            var body = WebhookPayload.Render("{\"flow\": \"{{myVar}}\", \"text\": \"{text}\"}", Event(NotificationKind.ConsoleError, "x"), false);

            Assert.AreEqual("{\"flow\": \"{{myVar}}\", \"text\": \"x\"}", body);
        }


        [TestMethod]
        public void an_event_without_fields_sends_empty_text()
        {
            var message = new NotificationMessage()
            {
                Kind = NotificationKind.ConsoleOutput,
                Timestamp = DateTime.Now,
                Text = "only text"
            };

            var body = WebhookPayload.Render("{text}|{details}|{service}|{instance}", message, false);

            Assert.AreEqual("only text|||", body);
        }


        /// <summary>
        /// Both providers rate an event the same way, so a rule in the receiving system does not have
        /// to know which provider delivered the message.
        /// </summary>
        [TestMethod]
        public void every_event_gets_the_level_of_the_event_log_provider()
        {
            Assert.AreEqual("error", WebhookPayload.Level(NotificationKind.ConsoleError));
            Assert.AreEqual("error", WebhookPayload.Level(NotificationKind.ServiceCrashed));

            Assert.AreEqual("warning", WebhookPayload.Level(NotificationKind.ServiceStop));
            Assert.AreEqual("warning", WebhookPayload.Level(NotificationKind.ServiceRestart));
            Assert.AreEqual("warning", WebhookPayload.Level(NotificationKind.DependencyWait));

            Assert.AreEqual("info", WebhookPayload.Level(NotificationKind.ServerStart));
            Assert.AreEqual("info", WebhookPayload.Level(NotificationKind.ServerShutdown));
            Assert.AreEqual("info", WebhookPayload.Level(NotificationKind.ServiceStart));
            Assert.AreEqual("info", WebhookPayload.Level(NotificationKind.ConsoleOutput));
        }


        /// <summary>
        /// Receivers sort by this field, so it has to sort in the same order the events happened.
        /// </summary>
        [TestMethod]
        public void the_time_is_sortable_and_names_the_zone()
        {
            var body = WebhookPayload.Render("{timestamp}", Event(NotificationKind.ServiceStart), false);

            Assert.IsTrue(Regex.IsMatch(body, "^\\d{4}-\\d{2}-\\d{2}T\\d{2}:\\d{2}:\\d{2}[+-]\\d{2}:\\d{2}$"), "sent: " + body);
        }


        [TestMethod]
        public void placeholder_names_are_read_without_case_or_spaces()
        {
            var body = WebhookPayload.Render("{ Text }/{TEXT}/{Kind}", Event(NotificationKind.ConsoleError, "x"), false);

            Assert.AreEqual("x/x/ConsoleError", body);
        }


        [TestMethod]
        public void control_characters_get_their_json_escape()
        {
            Assert.AreEqual("a \\\" b \\\\ c\\n d\\u0001", WebhookPayload.EscapeJson("a \" b \\ c\n d\u0001"));

            Assert.AreEqual("", WebhookPayload.EscapeJson(null));
        }
    }
}
