using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceManager.Extensions.Interfaces;
using ServiceManager.Extensions.SyslogNotifier;
using System;
using System.Globalization;
using System.Text;
using SyslogConfig = ServiceManager.Extensions.SyslogNotifier.Config.Config;

namespace ServiceManager.Tests
{
    /// <summary>
    /// How one event looks on the wire. No collector is involved: what matters is that the line keeps
    /// the brackets a collector parses and that a console block never turns into several entries.
    /// </summary>
    [TestClass]
    public class SyslogMessageTests
    {
        private static readonly DateTime Stamp = new DateTime(2026, 3, 4, 5, 6, 7);


        private static NotificationMessage Event(NotificationKind kind, String text = "Service stopped")
        {
            return new NotificationMessage()
            {
                Kind = kind,
                Timestamp = Stamp,
                InstanceName = "HomeLab",
                ServiceTitle = "Backup",
                Text = text,
                Details = "exit code 3"
            };
        }


        private static SyslogConfig Settings(Boolean rfc5424 = false, Int32 facility = 16, Int32 maxLength = 1024)
        {
            return new SyslogConfig()
            {
                Hostname = "host1",
                AppName = "ServiceManager",
                Facility = facility,
                MaxLength = maxLength,
                UseRfc5424 = rfc5424
            };
        }


        /// <summary>
        /// The numbers of RFC 5424 are what a collector filters on, and they have to mean the same as
        /// the error level of the event log and the level of the webhook provider.
        /// </summary>
        [TestMethod]
        public void the_severity_of_an_event_is_the_one_of_the_other_providers()
        {
            Assert.AreEqual(3, SyslogMessage.SeverityOf(NotificationKind.ConsoleError));
            Assert.AreEqual(3, SyslogMessage.SeverityOf(NotificationKind.ServiceCrashed));

            Assert.AreEqual(4, SyslogMessage.SeverityOf(NotificationKind.ServiceStop));
            Assert.AreEqual(4, SyslogMessage.SeverityOf(NotificationKind.ServiceRestart));
            Assert.AreEqual(4, SyslogMessage.SeverityOf(NotificationKind.DependencyWait));

            Assert.AreEqual(6, SyslogMessage.SeverityOf(NotificationKind.ServerStart));
            Assert.AreEqual(6, SyslogMessage.SeverityOf(NotificationKind.ServerShutdown));
            Assert.AreEqual(6, SyslogMessage.SeverityOf(NotificationKind.ServiceStart));
            Assert.AreEqual(6, SyslogMessage.SeverityOf(NotificationKind.ConsoleOutput));
        }


        [TestMethod]
        public void the_priority_is_the_facility_times_eight_plus_the_severity()
        {
            Assert.AreEqual(132, SyslogMessage.PriorityOf(16, NotificationKind.ServiceStop));

            Assert.AreEqual(11, SyslogMessage.PriorityOf(1, NotificationKind.ConsoleError));

            Assert.AreEqual(190, SyslogMessage.PriorityOf(23, NotificationKind.ServerStart));
        }


        /// <summary>
        /// A facility outside the range of the standard would produce a number no collector knows, so
        /// the provider falls back to local0 instead of sending something unusable.
        /// </summary>
        [TestMethod]
        public void a_facility_out_of_range_falls_back_to_local0()
        {
            Assert.AreEqual(132, SyslogMessage.PriorityOf(-1, NotificationKind.ServiceStop));

            Assert.AreEqual(134, SyslogMessage.PriorityOf(99, NotificationKind.ConsoleOutput));
        }


        [TestMethod]
        public void the_message_names_the_event_the_instance_and_the_service()
        {
            Assert.AreEqual("[ServiceStop] HomeLab / Backup: Service stopped | exit code 3", SyslogMessage.Text(Event(NotificationKind.ServiceStop)));
        }


        [TestMethod]
        public void a_field_that_is_empty_is_left_out_of_the_message()
        {
            var message = new NotificationMessage()
            {
                Kind = NotificationKind.ServerStart,
                Timestamp = Stamp,
                Text = "host rebooted"
            };

            Assert.AreEqual("[ServerStart] host rebooted", SyslogMessage.Text(message));
        }


        /// <summary>
        /// A line break is the end of an entry for every collector, so a console block of twenty lines
        /// has to arrive as one entry rather than twenty.
        /// </summary>
        [TestMethod]
        public void a_console_block_becomes_one_line()
        {
            Assert.AreEqual("a b c", SyslogMessage.SingleLine("a\r\nb\nc"));

            Assert.AreEqual("a b", SyslogMessage.SingleLine("a\n\nb"), "an empty line of a stack trace does not add a second space");

            Assert.AreEqual("abc", SyslogMessage.SingleLine("a\tb\u0000c"), "a tab and a null byte have nothing to search for in a log line");
        }


        [TestMethod]
        public void a_message_without_text_is_empty_rather_than_null()
        {
            Assert.AreEqual("", SyslogMessage.SingleLine(null));
            Assert.AreEqual("", SyslogMessage.SingleLine("   "));
        }


        /// <summary>
        /// The old form is what every collector reads. Its timestamp carries no year, which is the rule
        /// of RFC 3164 and not a mistake of the provider.
        /// </summary>
        [TestMethod]
        public void the_rfc3164_line_is_one_line_with_the_priority_in_front()
        {
            var line = SyslogMessage.Format3164(132, Stamp, "host1", "ServiceManager", "[ServiceStop] x", 1024);

            Assert.AreEqual("<132>Mar  4 05:06:07 host1 ServiceManager: [ServiceStop] x", line);
        }


        /// <summary>
        /// The day of the month is space padded in RFC 3164, a single digit day has to keep that space
        /// or the collector stops parsing the stamp.
        /// </summary>
        [TestMethod]
        public void a_single_digit_day_keeps_the_space_of_the_stamp()
        {
            var line = SyslogMessage.Format3164(132, new DateTime(2026, 1, 9, 7, 8, 9), "host1", "app", "x", 1024);

            Assert.AreEqual("<132>Jan  9 07:08:09 host1 app: x", line);
        }


        /// <summary>
        /// The new form names the year and the zone of the sender, which is what a collector in another
        /// timezone needs to sort the entries of two machines.
        /// </summary>
        [TestMethod]
        public void the_rfc5424_line_carries_the_version_and_the_zone_of_the_sender()
        {
            var line = SyslogMessage.Format5424(132, Stamp, "host1", "ServiceManager", "[ServiceStop] x", 1024);

            var stamp = Stamp.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);

            Assert.AreEqual("<132>1 " + stamp + " host1 ServiceManager - - [ServiceStop] x", line);
        }


        [TestMethod]
        public void the_format_follows_the_settings_of_the_provider()
        {
            var plain = SyslogMessage.Format(Settings(), Event(NotificationKind.ServiceStop));

            var structured = SyslogMessage.Format(Settings(rfc5424: true), Event(NotificationKind.ServiceStop));

            Assert.IsTrue(plain.StartsWith("<132>Mar  4 05:06:07 host1 ServiceManager: ", StringComparison.Ordinal), plain);

            Assert.IsTrue(structured.StartsWith("<132>1 2026-03-04T05:06:07", StringComparison.Ordinal), structured);
        }


        /// <summary>
        /// A settings file written by hand has the fields empty more often than not. The line still has
        /// to name the machine it came from.
        /// </summary>
        [TestMethod]
        public void empty_settings_fall_back_to_the_machine_and_local0()
        {
            var line = SyslogMessage.Format(new SyslogConfig(), Event(NotificationKind.ConsoleError));

            Assert.IsTrue(line.StartsWith("<131>", StringComparison.Ordinal), line);

            Assert.IsTrue(line.Contains(" " + Environment.MachineName + " ServiceManager: "), line);

            Assert.IsTrue(line.EndsWith("[ConsoleError] HomeLab / Backup: Service stopped | exit code 3"), line);
        }


        /// <summary>
        /// Collectors cut long entries around a kilobyte. Cutting the message part is fine, cutting the
        /// header means the entry is dropped without a word about it.
        /// </summary>
        [TestMethod]
        public void a_long_message_is_cut_but_the_header_stays()
        {
            const String header = "<132>Mar  4 05:06:07 h a: ";

            var room = Encoding.UTF8.GetByteCount(header) + 5;

            var line = SyslogMessage.Format3164(132, Stamp, "h", "a", "hello world", room);

            Assert.AreEqual(header + "hello", line);
        }


        /// <summary>
        /// The limit of a collector counts bytes, not characters, so a cut may not split a character
        /// that utf8 writes as two.
        /// </summary>
        [TestMethod]
        public void the_limit_counts_bytes_and_never_splits_a_character()
        {
            const String header = "<132>Mar  4 05:06:07 h a: ";

            var room = Encoding.UTF8.GetByteCount(header) + 4;

            var line = SyslogMessage.Format3164(132, Stamp, "h", "a", "ab\u00e4cd", room);

            Assert.AreEqual(header + "ab\u00e4", line);

            Assert.AreEqual(room, Encoding.UTF8.GetByteCount(line));
        }


        [TestMethod]
        public void a_limit_of_zero_sends_the_whole_line()
        {
            var line = SyslogMessage.Format3164(132, Stamp, "h", "a", new String('x', 5000), 0);

            Assert.AreEqual(5000, Encoding.UTF8.GetByteCount(line) - Encoding.UTF8.GetByteCount("<132>Mar  4 05:06:07 h a: "));
        }
    }
}
