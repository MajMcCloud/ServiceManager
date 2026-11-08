using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceManager.Extensions.Interfaces;
using ServiceManager.Extensions.SmtpNotifier;
using System;
using System.Text.RegularExpressions;
using MailConfig = ServiceManager.Extensions.SmtpNotifier.Config.Config;

namespace ServiceManager.Tests
{
    /// <summary>
    /// What stands in a mail. No server is involved: what matters is that a console line can neither
    /// lengthen a subject past what a mail client shows nor add a second header to the message.
    /// </summary>
    [TestClass]
    public class MailContentTests
    {
        private static MailConfig Settings(String prefix = "ServiceManager")
        {
            return new MailConfig() { SubjectPrefix = prefix };
        }


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


        /// <summary>
        /// A mail rule in the inbox should agree with a rule in the event log and the webhook receiver
        /// of the same installation, so the rating is not up to the mail provider.
        /// </summary>
        [TestMethod]
        public void every_event_gets_the_level_of_the_other_providers()
        {
            Assert.AreEqual("error", MailContent.Severity(NotificationKind.ConsoleError));

            Assert.AreEqual("warning", MailContent.Severity(NotificationKind.ServiceStop));
            Assert.AreEqual("warning", MailContent.Severity(NotificationKind.ServiceRestart));

            Assert.AreEqual("info", MailContent.Severity(NotificationKind.ServerStart));
            Assert.AreEqual("info", MailContent.Severity(NotificationKind.ServerShutdown));
            Assert.AreEqual("info", MailContent.Severity(NotificationKind.ServiceStart));
            Assert.AreEqual("info", MailContent.Severity(NotificationKind.ConsoleOutput));
        }


        [TestMethod]
        public void the_subject_names_severity_prefix_instance_and_service()
        {
            var subject = MailContent.RenderSubject(Settings(), Event(NotificationKind.ServiceStop));

            Assert.AreEqual("[WARNING] ServiceManager - HomeLab / Backup: Service stopped", subject);
        }


        [TestMethod]
        public void an_error_is_spelled_out_in_capitals()
        {
            var subject = MailContent.RenderSubject(Settings("Homelab"), Event(NotificationKind.ConsoleError, "disk full"));

            Assert.AreEqual("[ERROR] Homelab - HomeLab / Backup: disk full", subject);
        }


        [TestMethod]
        public void an_event_without_service_or_instance_still_reads_as_one_line()
        {
            var message = new NotificationMessage()
            {
                Kind = NotificationKind.ServerStart,
                Timestamp = DateTime.Now,
                Text = "host rebooted"
            };

            Assert.AreEqual("[INFO] ServiceManager - ServiceManager: host rebooted", MailContent.RenderSubject(Settings(), message));
        }


        /// <summary>
        /// Mail clients cut a subject far earlier than the protocol does, and a whole console block
        /// would push the interesting part out of the list view.
        /// </summary>
        [TestMethod]
        public void a_long_console_line_is_cut_out_of_the_subject()
        {
            var subject = MailContent.RenderSubject(Settings(), Event(NotificationKind.ConsoleError, new String('x', 500)));

            Assert.AreEqual("[ERROR] ServiceManager - HomeLab / Backup: " + new String('x', 120), subject);
        }


        /// <summary>
        /// A prefix and an instance name of their own can fill the line on their own. What a mail
        /// client shows is the first 200 characters, everything behind them is lost anyway.
        /// </summary>
        [TestMethod]
        public void a_subject_never_grows_past_what_a_mail_client_shows()
        {
            var subject = MailContent.RenderSubject(Settings(new String('p', 150)), Event(NotificationKind.ConsoleError, new String('x', 500)));

            Assert.AreEqual(MailContent.SubjectLimit, subject.Length, "subject: " + subject);
        }


        /// <summary>
        /// The subject is a header. A service name with a line break in it could otherwise append
        /// headers of its own to the mail.
        /// </summary>
        [TestMethod]
        public void a_service_name_cannot_add_a_second_header()
        {
            var message = Event(NotificationKind.ConsoleError, "boom");

            message.ServiceTitle = "Backup\r\nX-Spam: yes";

            var subject = MailContent.RenderSubject(Settings(), message);

            Assert.IsFalse(subject.Contains("\r"), "subject: " + subject);
            Assert.IsFalse(subject.Contains("\n"), "subject: " + subject);
            Assert.IsFalse(subject.Contains("X-Spam"), "subject: " + subject);

            Assert.AreEqual("[ERROR] ServiceManager - HomeLab / Backup: boom", subject);
        }


        [TestMethod]
        public void the_body_carries_the_fields_of_the_event_as_a_table()
        {
            var body = MailContent.RenderBody(Event(NotificationKind.ServiceStop));

            Assert.IsTrue(body.Contains("Event     ServiceStop"), body);
            Assert.IsTrue(body.Contains("Severity  warning"), body);
            Assert.IsTrue(body.Contains("Time      2026-03-04 05:06:07"), body);
            Assert.IsTrue(body.Contains("Instance  HomeLab"), body);
            Assert.IsTrue(body.Contains("Service   Backup"), body);
            Assert.IsTrue(body.Contains("Details   exit code 3"), body);
        }


        /// <summary>
        /// The console line itself belongs below the table, not squeezed into one line of it: a stack
        /// trace has to stay readable and keep its breaks.
        /// </summary>
        [TestMethod]
        public void the_text_of_the_event_is_appended_below_the_table()
        {
            var body = MailContent.RenderBody(Event(NotificationKind.ConsoleError, "line one\nline two"));

            Assert.IsTrue(body.EndsWith("line one\nline two" + Environment.NewLine), "body ends with: " + body);
        }


        [TestMethod]
        public void a_field_that_is_empty_is_left_out_of_the_table()
        {
            var body = MailContent.RenderBody(new NotificationMessage()
            {
                Kind = NotificationKind.ConsoleOutput,
                Timestamp = DateTime.Now,
                Text = "only text"
            });

            Assert.IsFalse(body.Contains("Details"), body);
            Assert.IsFalse(body.Contains("Service "), body);
            Assert.IsTrue(body.Contains("Event     ConsoleOutput"), body);
        }


        /// <summary>
        /// The time is what a recipient compares against the timestamps of other systems, so it names
        /// its own layout rather than a culture specific one.
        /// </summary>
        [TestMethod]
        public void the_time_in_the_body_is_sortable()
        {
            var body = MailContent.RenderBody(Event(NotificationKind.ServiceStart));

            Assert.IsTrue(Regex.IsMatch(body, "Time +\\d{4}-\\d{2}-\\d{2} \\d{2}:\\d{2}:\\d{2}"), body);
        }
    }
}
