using System;

namespace ServiceManager.UI.Columns
{
    /// <summary>
    /// One column of the service list: the heading it shows, the width it starts with, whether it can be
    /// switched off and whether it is there before anyone chose. The definitions live here rather than in the
    /// designer, because whatever control replaces the list has to be fed from the same list.
    /// </summary>
    public class ServiceColumn
    {
        public ServiceColumn(eColumn column, String title, int width, bool optional, bool defaultShown)
        {
            Column = column;
            Title = title;
            Width = width;
            Optional = optional;
            DefaultShown = defaultShown;
        }

        public eColumn Column { get; private set; }

        public String Title { get; private set; }

        /// <summary>A width of zero leaves the control at its own default.</summary>
        public int Width { get; private set; }

        /// <summary>False for a column that is always there.</summary>
        public bool Optional { get; private set; }

        /// <summary>Whether a column that nobody has switched off yet is there.</summary>
        public bool DefaultShown { get; private set; }
    }
}
