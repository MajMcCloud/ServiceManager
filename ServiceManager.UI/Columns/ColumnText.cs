using ServiceManager.Base.Data;
using System;
using System.Globalization;

namespace ServiceManager.UI.Columns
{
    /// <summary>
    /// The text one column shows for one service. The window and the tests ask the same place, so the text of
    /// a column is not spread over the window.
    /// </summary>
    public static class ColumnText
    {
        public static String Get(eColumn column, ServiceItem item, ServiceAnalytics analytics, DateTime now)
        {
            switch (column)
            {
                case eColumn.Pid:
                    return analytics.IsRunning ? analytics.ProcessID.ToString(CultureInfo.InvariantCulture) : ValueFormat.Nothing;
                case eColumn.Title:
                    return item.Title;
                case eColumn.Status:
                    return ServiceAnalytics.StatusText(analytics.Status);
                case eColumn.Restarts:
                    return analytics.Restarts.ToString(CultureInfo.InvariantCulture);
                case eColumn.AutoRestart:
                    return item.ForceRestart ? "yes" : "no";
                case eColumn.Uptime:
                    return ValueFormat.Uptime(analytics.Started, now);
                case eColumn.LastReply:
                    return ValueFormat.LastReply(analytics.LastPing, now);
                case eColumn.Memory:
                    return ValueFormat.Memory(analytics.WorkingSetBytes);
                case eColumn.Cpu:
                    return ValueFormat.Cpu(analytics.CpuPercent);
                case eColumn.Threads:
                    return ValueFormat.Threads(analytics.ThreadCount);
                default:
                    //The activity column carries the image of the row and no text of its own
                    return String.Empty;
            }
        }
    }
}
