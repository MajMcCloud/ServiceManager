using System;
using System.Globalization;

namespace ServiceManager.UI.Columns
{
    /// <summary>
    /// The text a measured value gets in the service list. A value that was never measured reads as a dash,
    /// so a column never shows a zero where there is nothing.
    /// </summary>
    public static class ValueFormat
    {
        /// <summary>What an empty cell shows.</summary>
        public const String Nothing = "-";

        /// <summary>How long a process has been up, for example "3d 4h", "2h 15m" or "48s".</summary>
        public static String Uptime(DateTime? started, DateTime now)
        {
            if (started == null || started > now)
            {
                return Nothing;
            }

            return span(now - started.Value);
        }

        /// <summary>How long ago a service last answered. A service that never did reads as "never".</summary>
        public static String LastReply(DateTime lastReply, DateTime now)
        {
            if (lastReply == DateTime.MinValue || lastReply > now)
            {
                return "never";
            }

            return span(now - lastReply);
        }

        /// <summary>The memory of a process, on one of the usual steps.</summary>
        public static String Memory(Int64? bytes)
        {
            if (bytes == null || bytes.Value < 0)
            {
                return Nothing;
            }

            Double value = bytes.Value;

            if (value >= 1024 * 1024 * 1024)
            {
                return (value / (1024 * 1024 * 1024)).ToString("0.0", CultureInfo.InvariantCulture) + " GB";
            }

            if (value >= 1024 * 1024)
            {
                return (value / (1024 * 1024)).ToString("0.0", CultureInfo.InvariantCulture) + " MB";
            }

            if (value >= 1024)
            {
                return (value / 1024).ToString("0", CultureInfo.InvariantCulture) + " KB";
            }

            return value.ToString("0", CultureInfo.InvariantCulture) + " B";
        }

        /// <summary>The share of the machine, one decimal.</summary>
        public static String Cpu(Double? percent)
        {
            if (percent == null)
            {
                return Nothing;
            }

            return percent.Value.ToString("0.0", CultureInfo.InvariantCulture) + " %";
        }

        /// <summary>How many threads a process has.</summary>
        public static String Threads(Int32? count)
        {
            if (count == null)
            {
                return Nothing;
            }

            return count.Value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// A stretch of time in the coarsest unit that still tells something: days and hours above a day,
        /// hours and minutes above an hour, minutes and seconds above a minute.
        /// </summary>
        static String span(TimeSpan value)
        {
            if (value.TotalDays >= 1)
            {
                return String.Format(CultureInfo.InvariantCulture, "{0}d {1}h", (Int32)value.TotalDays, value.Hours);
            }

            if (value.TotalHours >= 1)
            {
                return String.Format(CultureInfo.InvariantCulture, "{0}h {1}m", (Int32)value.TotalHours, value.Minutes);
            }

            if (value.TotalMinutes >= 1)
            {
                return String.Format(CultureInfo.InvariantCulture, "{0}m {1}s", (Int32)value.TotalMinutes, value.Seconds);
            }

            return String.Format(CultureInfo.InvariantCulture, "{0}s", (Int32)value.TotalSeconds);
        }
    }
}
