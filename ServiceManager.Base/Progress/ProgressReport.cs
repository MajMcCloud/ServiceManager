using System;

namespace ServiceManager.Base.Progress
{
    /// <summary>
    /// What a running operation reports about the step it is in. A report without a total leaves the
    /// bar running, a report with one shows how far the operation has come.
    /// </summary>
    public class ProgressReport
    {
        /// <summary>The sentence to show the user.</summary>
        public String Message { get; private set; }

        /// <summary>The step that is running, counted from one. Negative when the total is unknown.</summary>
        public Int32 Current { get; private set; }

        /// <summary>How many steps the operation has. Negative when it is not known up front.</summary>
        public Int32 Total { get; private set; }

        private ProgressReport(String message, Int32 current, Int32 total)
        {
            Message = message ?? String.Empty;

            if (current < 0 || total <= 0)
            {
                Current = -1;
                Total = -1;

                return;
            }

            Current = current > total ? total : current;
            Total = total;
        }

        /// <summary>The report for an operation whose length is not known.</summary>
        public static ProgressReport Running(String message)
        {
            return new ProgressReport(message, -1, -1);
        }

        /// <summary>The report for step current of total, both counted from one.</summary>
        public static ProgressReport Step(String message, Int32 current, Int32 total)
        {
            return new ProgressReport(message, current, total);
        }

        /// <summary>True when the bar has nothing to measure against.</summary>
        public bool IsIndeterminate
        {
            get { return Current < 0 || Total <= 0; }
        }

        /// <summary>How far the operation has come, from 0 to 100, or -1 while that stays unknown.</summary>
        public Int32 Percent
        {
            get
            {
                if (IsIndeterminate)
                {
                    return -1;
                }

                return (Int32)((Double)Current / Total * 100);
            }
        }
    }
}
