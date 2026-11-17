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
            new ServiceColumn(eColumn.Activity, "Activity", 60, false),
            new ServiceColumn(eColumn.Pid, "PID", 60, false),
            new ServiceColumn(eColumn.Title, "Title", 0, false),
            new ServiceColumn(eColumn.Status, "Status", 150, false),
            new ServiceColumn(eColumn.Restarts, "Restarts", 60, false),
            new ServiceColumn(eColumn.AutoRestart, "Auto Restart", 80, false)
        };

        readonly List<ServiceColumn> m_Shown = new List<ServiceColumn>();

        public ServiceColumnLayout()
        {
            foreach (ServiceColumn column in All)
            {
                m_Shown.Add(column);
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
