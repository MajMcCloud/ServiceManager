using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceManager.Extensions;
using System;
using System.IO;
using System.Linq;

namespace ServiceManager.Tests
{
    /// <summary>
    /// The log channel every part of the solution writes into. A service has no console, so the lines
    /// only count when they reach a file: which file they land in, how a line is marked, what happens
    /// when the folder cannot be written and what a reader gets back out of the files.
    /// </summary>
    [TestClass]
    public class SMLogTests
    {
        private String directory;

        private String storedPath;

        private int storedRetention;

        private bool storedConsole;

        private bool storedEventLog;

        [TestInitialize]
        public void Setup()
        {
            storedPath = SMLog.LogPath;
            storedRetention = SMLog.RetentionDays;
            storedConsole = SMLog.UseConsole;
            storedEventLog = SMLog.UseEventLog;

            directory = Path.Combine(Path.GetTempPath(), "servicemanager-tests", Guid.NewGuid().ToString("N"));

            SMLog.LogPath = directory;
            SMLog.RetentionDays = 14;
            SMLog.UseConsole = false;
            SMLog.UseEventLog = false;
        }

        [TestCleanup]
        public void Teardown()
        {
            SMLog.LogPath = storedPath;
            SMLog.RetentionDays = storedRetention;
            SMLog.UseConsole = storedConsole;
            SMLog.UseEventLog = storedEventLog;

            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }

        [TestMethod]
        public void the_default_folder_is_a_logs_folder_next_to_the_executable()
        {
            Assert.AreEqual(Path.Combine(AppContext.BaseDirectory, "logs"), storedPath);
        }

        [TestMethod]
        public void write_creates_the_file_of_the_current_day()
        {
            SMLog.Info("the endpoint is open");

            var files = SMLog.GetFiles();

            Assert.AreEqual(1, files.Count);
            Assert.AreEqual(SMLog.FileName(DateTime.Now), Path.GetFileName(files[0]));
        }

        [TestMethod]
        public void write_keeps_every_level_of_its_own_name_in_the_line()
        {
            SMLog.Info("a plain line");
            SMLog.Warning("a recovered problem");
            SMLog.Error("a failure");

            var lines = File.ReadAllLines(SMLog.GetFiles()[0]);

            Assert.IsTrue(lines[0].Contains("] [INFO] a plain line"), lines[0]);
            Assert.IsTrue(lines[1].Contains("] [WARN] a recovered problem"), lines[1]);
            Assert.IsTrue(lines[2].Contains("] [ERROR] a failure"), lines[2]);
        }

        [TestMethod]
        public void write_places_an_exception_under_its_message()
        {
            SMLog.Warning("the provider failed", new InvalidOperationException("the server did not answer"));

            var lines = File.ReadAllLines(SMLog.GetFiles()[0]);

            Assert.IsTrue(lines.Length > 1, "the exception has to follow the message instead of replacing it");
            Assert.IsTrue(lines[1].StartsWith("    "), lines[1]);
            Assert.IsTrue(lines[1].Contains("System.InvalidOperationException: the server did not answer"), lines[1]);
        }

        [TestMethod]
        public void write_ignores_an_empty_line_without_a_failure()
        {
            SMLog.Write(eLogLevel.Info, "", null);

            Assert.AreEqual(0, SMLog.GetFiles().Count);
        }

        [TestMethod]
        public void write_does_not_throw_for_a_folder_it_cannot_create()
        {
            File.WriteAllText(Path.Combine(Path.GetTempPath(), "servicemanager-tests-blocked.log"), "");

            SMLog.LogPath = Path.Combine(Path.GetTempPath(), "servicemanager-tests-blocked.log", "logs");

            try
            {
                SMLog.Info("a line nobody can store");
            }
            finally
            {
                File.Delete(Path.Combine(Path.GetTempPath(), "servicemanager-tests-blocked.log"));
            }
        }

        [TestMethod]
        public void write_reports_the_failure_of_a_broken_folder()
        {
            SMLog.LogPath = Path.Combine(Path.GetTempPath(), "servicemanager-tests-blocked.log", "logs");

            File.WriteAllText(Path.Combine(Path.GetTempPath(), "servicemanager-tests-blocked.log"), "");

            try
            {
                SMLog.Info("a line nobody can store");

                Assert.IsNotNull(SMLog.WriteFailure);
            }
            finally
            {
                File.Delete(Path.Combine(Path.GetTempPath(), "servicemanager-tests-blocked.log"));
            }
        }

        [TestMethod]
        public void get_files_lists_the_newest_day_first()
        {
            WriteDay(DateTime.Now.Date);
            WriteDay(DateTime.Now.Date.AddDays(-1));
            WriteDay(DateTime.Now.Date.AddDays(-2));

            var files = SMLog.GetFiles();

            Assert.AreEqual(3, files.Count);
            Assert.AreEqual(SMLog.FileName(DateTime.Now.Date), Path.GetFileName(files[0]));
            Assert.AreEqual(SMLog.FileName(DateTime.Now.Date.AddDays(-2)), Path.GetFileName(files[2]));
        }

        [TestMethod]
        public void get_files_leaves_out_the_files_of_other_tools()
        {
            Directory.CreateDirectory(directory);

            File.WriteAllText(Path.Combine(directory, "readme.txt"), "not a log");

            Assert.AreEqual(0, SMLog.GetFiles().Count);
        }

        [TestMethod]
        public void first_line_of_a_new_day_drops_what_is_past_the_retention()
        {
            SMLog.RetentionDays = 3;

            var old = WriteDay(DateTime.Now.Date.AddDays(-9));

            SMLog.Info("a new day started");

            Assert.IsFalse(File.Exists(old));
        }

        [TestMethod]
        public void first_line_of_a_new_day_keeps_what_is_inside_the_retention()
        {
            SMLog.RetentionDays = 3;

            var recent = WriteDay(DateTime.Now.Date.AddDays(-2));

            SMLog.Info("a new day started");

            Assert.IsTrue(File.Exists(recent));
        }

        [TestMethod]
        public void read_tail_answers_the_newest_lines_in_the_order_they_came()
        {
            for (var line = 1; line <= 20; line++)
            {
                SMLog.Info("line " + line);
            }

            var lines = SMLog.ReadTail(SMLog.GetFiles()[0], 5);

            CollectionAssert.AreEqual(
                new[] { "line 16", "line 17", "line 18", "line 19", "line 20" },
                lines.Select(a => a.Split(']')[2].Trim()).ToArray());
        }

        [TestMethod]
        public void read_tail_returns_nothing_for_a_missing_file()
        {
            Assert.AreEqual(0, SMLog.ReadTail(Path.Combine(directory, "ServiceManager-2000-01-01.log"), 100).Count);
        }

        [TestMethod]
        public void file_day_reads_the_day_a_file_name_stands_for()
        {
            var day = new DateTime(2026, 3, 7);

            Assert.AreEqual(day, SMLog.FileDay(SMLog.FileName(day)));
        }

        [TestMethod]
        public void file_day_refuses_a_name_that_is_not_a_log()
        {
            Assert.AreEqual(DateTime.MinValue, SMLog.FileDay("notes.txt"));
        }

        private String WriteDay(DateTime day)
        {
            Directory.CreateDirectory(directory);

            var file = Path.Combine(directory, SMLog.FileName(day));

            File.WriteAllText(file, "[2026-01-01 00:00:00.000] [INFO] written earlier" + Environment.NewLine);

            return file;
        }
    }
}
