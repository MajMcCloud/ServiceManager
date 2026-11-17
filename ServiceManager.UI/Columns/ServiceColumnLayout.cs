using System;
using System.Collections.Generic;

namespace ServiceManager.UI.Columns
{
    /// <summary>
    /// Which columns the list shows and where each of them sits in a row. A column that is switched off takes
    /// no place, so nothing may address a row by a fixed index.
    /// </summary>
    public class ServiceColumnLayout
    {
        /// <summary>Every column the list knows, in the order they appear.</summary>
        public static readonly List<ServiceColumn> All = new List<ServiceColumn>
        {
            new ServiceColumn(eColumn.Activity, "Activity", 60, false, true),
            new ServiceColumn(eColumn.Pid, "PID", 60, true, true),
            new ServiceColumn(eColumn.Title, "Title", 0, true, true),
            new ServiceColumn(eColumn.Status, "Status", 150, true, true),
            new ServiceColumn(eColumn.Restarts, "Restarts", 60, true, true),
            new ServiceColumn(eColumn.AutoRestart, "Auto Restart", 80, true, true),
            new ServiceColumn(eColumn.Uptime, "Uptime", 70, true, false),
            new ServiceColumn(eColumn.LastReply, "Last reply", 70, true, false),
            new ServiceColumn(eColumn.Memory, "Memory", 80, true, true),
            new ServiceColumn(eColumn.Cpu, "CPU", 60, true, true),
            new ServiceColumn(eColumn.Threads, "Threads", 60, true, false)
        };

        readonly List<ServiceColumn> m_Shown = new List<ServiceColumn>();

        /// <summary>Every column that can be seen is there, nothing was chosen yet.</summary>
        public ServiceColumnLayout() : this(null)
        {
        }

        /// <summary>
        /// Builds the columns out of the definitions and what the person on this machine chose. A column that
        /// is not in the choices follows the default of its definition, so a column that is added later shows
        /// up without anyone having to change a stored file.
        /// </summary>
        public ServiceColumnLayout(Dictionary<String, Boolean> choices)
        {
            foreach (ServiceColumn column in All)
            {
                Boolean shown = column.DefaultShown;

                //A column that is never offered cannot be switched off, not even by a hand written file
                Boolean chosen;

                if (column.Optional && choices != null && choices.TryGetValue(column.Column.ToString(), out chosen))
                {
                    shown = chosen;
                }

                if (shown)
                {
                    m_Shown.Add(column);
                }
            }
        }

        /// <summary>The columns that are visible, in the order they appear in a row.</summary>
        public List<ServiceColumn> Shown
        {
            get { return m_Shown; }
        }

        /// <summary>Where the column sits inside a row, -1 when it is switched off.</summary>
        public int IndexOf(eColumn column)
        {
            for (int i = 0; i < m_Shown.Count; i++)
            {
                if (m_Shown[i].Column == column)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
