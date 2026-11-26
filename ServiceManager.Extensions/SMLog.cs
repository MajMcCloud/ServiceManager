using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace ServiceManager.Extensions
{
    /// <summary>
    /// The log channel of the service and of everything it loads. A Windows service has no console, so
    /// what the code base used to write through Console.WriteLine was gone before anyone could read it.
    /// Every line goes into a text file in the logs folder of the installation, one file per day, and a
    /// Warning or an Error additionally goes into the application event log.
    ///
    /// The channel sits in ServiceManager.Extensions on purpose: that assembly is the one the host, the
    /// loader and every provider share, and it is loaded once, so a provider writes into the log of the
    /// running service instead of into a copy of its own.
    /// </summary>
    public static class SMLog
    {
        /// <summary>Start of every log file name, the date behind it selects the day.</summary>
        public const String FilePrefix = "ServiceManager-";

        private const String FileExtension = ".log";

        // A log file stays small, so reading a fixed window from its end is enough and keeps a reader
        // from pulling a whole month of lines into memory.
        private const long TailWindow = 512 * 1024;

        private static readonly Object m_Sync = new Object();

        private static bool m_EventLogChecked;

        private static bool m_EventLogBroken;


        /// <summary>
        /// Folder holding the log files. It sits next to the executable, like the extensions and the
        /// config folder, so an installation keeps everything in one place and a test run in bin stays
        /// inside bin. A trailing separator is added where it is missing.
        /// </summary>
        public static String LogPath { get; set; } = Path.Combine(AppContext.BaseDirectory, "logs");

        /// <summary>Day files older than this many days are dropped when a new day starts.</summary>
        public static int RetentionDays { get; set; } = 14;

        /// <summary>Warning and Error also go to the event log, which is only worth it without a console.</summary>
        public static bool UseEventLog { get; set; } = !Environment.UserInteractive;

        /// <summary>Interactive runs keep answering on the console, a service has none to write to.</summary>
        public static bool UseConsole { get; set; } = Environment.UserInteractive;

        /// <summary>The event log source. Registering a missing source needs administrator rights.</summary>
        public static String EventLogSource { get; set; } = "ServiceManager";

        /// <summary>The failure of the last failed write, so a reader can tell that lines are missing.</summary>
        public static Exception WriteFailure { get; private set; }


        /// <summary>Logs a line that only tells what happened.</summary>
        public static void Info(String message)
        {
            Write(eLogLevel.Info, message, null);
        }


        /// <summary>Logs a problem the service recovered from.</summary>
        public static void Warning(String message)
        {
            Write(eLogLevel.Warning, message, null);
        }


        /// <summary>Logs a problem the service recovered from, with the exception behind it.</summary>
        public static void Warning(String message, Exception error)
        {
            Write(eLogLevel.Warning, message, error);
        }


        /// <summary>Logs a failure an operator has to look at.</summary>
        public static void Error(String message)
        {
            Write(eLogLevel.Error, message, null);
        }


        /// <summary>Logs a failure an operator has to look at, with the exception behind it.</summary>
        public static void Error(String message, Exception error)
        {
            Write(eLogLevel.Error, message, error);
        }


        /// <summary>
        /// Hands a line to every enabled sink. A log line never fails a call: whatever a sink cannot
        /// write is dropped, and only the failure of the sink itself is reported, on the other sinks.
        /// </summary>
        public static void Write(eLogLevel level, String message, Exception error)
        {
            if (String.IsNullOrEmpty(message) && error == null)
            {
                return;
            }

            lock (m_Sync)
            {
                if (UseConsole)
                {
                    Console.WriteLine(Format(level, message, error));
                }

                WriteFile(level, message, error);

                if (UseEventLog && level != eLogLevel.Info)
                {
                    WriteEventLog(level, message, error);
                }
            }
        }


        /// <summary>Builds one line: the time, the level, the message and the exception under it.</summary>
        private static String Format(eLogLevel level, String message, Exception error)
        {
            var builder = new StringBuilder();

            builder.Append('[');
            builder.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture));
            builder.Append("] [");
            builder.Append(LevelText(level));
            builder.Append("] ");
            builder.Append(message);

            if (error != null)
            {
                builder.AppendLine();

                foreach (var line in error.ToString().Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
                {
                    builder.Append("    ").AppendLine(line);
                }
            }

            return builder.ToString();
        }


        private static String LevelText(eLogLevel level)
        {
            switch (level)
            {
                case eLogLevel.Warning:
                    return "WARN";
                case eLogLevel.Error:
                    return "ERROR";
                default:
                    return "INFO";
            }
        }


        private static void WriteFile(eLogLevel level, String message, Exception error)
        {
            try
            {
                var folder = LogPath;

                Directory.CreateDirectory(folder);

                var file = Path.Combine(folder, FileName(DateTime.Now));

                // The day rolls over without a restart, so the first line of a new day is the moment
                // the old files are cleaned up.
                var firstLineOfTheDay = !File.Exists(file);

                using (var stream = new FileStream(file, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                {
                    using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                    {
                        writer.WriteLine(Format(level, message, error));
                    }
                }

                WriteFailure = null;

                if (firstLineOfTheDay)
                {
                    Purge(folder);
                }
            }
            catch (Exception ex)
            {
                WriteFailure = ex;

                if (level == eLogLevel.Info)
                {
                    Error("ServiceManager could not write to its log file", ex);
                }
            }
        }


        private static void Purge(String folder)
        {
            if (RetentionDays <= 0)
            {
                return;
            }

            var oldest = DateTime.Now.Date.AddDays(-RetentionDays);

            foreach (var file in GetFiles())
            {
                var day = FileDay(Path.GetFileName(file));

                if (day == DateTime.MinValue || day >= oldest)
                {
                    continue;
                }

                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    WriteFailure = ex;
                }
            }
        }


        /// <summary>
        /// The event log is only reached once per process: writing through an unregistered source throws
        /// on every call, and registering one needs rights a plain interactive run does not have.
        /// </summary>
        private static void WriteEventLog(eLogLevel level, String message, Exception error)
        {
            // The event log is a Windows API and this assembly carries no platform in its target framework,
            // so the call site has to ask.
            if (!OperatingSystem.IsWindows())
            {
                return;
            }

            if (m_EventLogChecked && m_EventLogBroken)
            {
                return;
            }

            try
            {
                if (!m_EventLogChecked)
                {
                    m_EventLogChecked = true;

                    if (!EventLog.SourceExists(EventLogSource))
                    {
                        EventLog.CreateEventSource(EventLogSource, "Application");
                    }
                }

                var text = error != null ? message + Environment.NewLine + error : message;

                var type = level == eLogLevel.Error ? EventLogEntryType.Error : EventLogEntryType.Warning;

                EventLog.WriteEntry(EventLogSource, TrimForEventLog(text), type);
            }
            catch (Exception ex)
            {
                m_EventLogBroken = true;

                WriteFailure = ex;
            }
        }


        // The event log refuses an entry over 32766 characters, and a full stack trace of a provider
        // can come close when the exception carries an inner chain.
        private static String TrimForEventLog(String text)
        {
            if (text.Length <= 30000)
            {
                return text;
            }

            return text.Substring(0, 30000) + "...";
        }


        /// <summary>Name of the log file of one day.</summary>
        public static String FileName(DateTime day)
        {
            return FilePrefix + day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + FileExtension;
        }


        /// <summary>The day a log file name stands for, or DateTime.MinValue for a foreign name.</summary>
        public static DateTime FileDay(String fileName)
        {
            var name = Path.GetFileNameWithoutExtension(fileName);

            if (name == null || !name.StartsWith(FilePrefix, StringComparison.OrdinalIgnoreCase))
            {
                return DateTime.MinValue;
            }

            var rest = name.Substring(FilePrefix.Length);

            DateTime day;

            if (!DateTime.TryParseExact(rest, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out day))
            {
                return DateTime.MinValue;
            }

            return day;
        }


        /// <summary>Every log file, newest day first.</summary>
        public static List<String> GetFiles()
        {
            var folder = LogPath;

            if (!Directory.Exists(folder))
            {
                return new List<String>();
            }

            try
            {
                return Directory.GetFiles(folder, FilePrefix + "*" + FileExtension)
                    .OrderByDescending(a => Path.GetFileName(a), StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
            catch (Exception ex)
            {
                WriteFailure = ex;

                return new List<String>();
            }
        }


        /// <summary>
        /// Reads the newest lines of a log file. The file is opened for shared reading, so the service
        /// keeps writing while the interface shows its content.
        /// </summary>
        public static List<String> ReadTail(String file, int maxLines)
        {
            var lines = new List<String>();

            if (String.IsNullOrEmpty(file) || maxLines <= 0 || !File.Exists(file))
            {
                return lines;
            }

            try
            {
                using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var start = Math.Max(0, stream.Length - TailWindow);

                    stream.Seek(start, SeekOrigin.Begin);

                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        var first = true;

                        String line;

                        while ((line = reader.ReadLine()) != null)
                        {
                            // The window is cut at a byte and not at a line, so the first line read may
                            // be the tail of a longer one.
                            if (first && start > 0)
                            {
                                first = false;

                                continue;
                            }

                            first = false;

                            lines.Add(line);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteFailure = ex;
            }

            lines.Reverse();

            if (lines.Count > maxLines)
            {
                lines = lines.GetRange(0, maxLines);
            }

            lines.Reverse();

            return lines;
        }
    }
}
