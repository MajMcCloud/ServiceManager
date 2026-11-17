using System;

namespace ServiceManager.UI.Columns
{
    /// <summary>
    /// One column of the service list: the heading it shows, the width it starts with and whether it can be
    /// switched off. The definitions live here rather than in the designer, because whatever control replaces
    /// the list has to be fed from the same list.
    /// </summary>
    public class ServiceColumn
    {
        public ServiceColumn(eColumn column, String title, int width, bool optional)
        {
            Column = column;
            Title = title;
            Width = width;
            Optional = optional;
        }

        public eColumn Column { get; private set; }

        public String Title { get; private set; }

        /// <summary>A width of zero leaves the control at its own default.</summary>
        public int Width { get; private set; }

        /// <summary>False for a column that is always there.</summary>
        public bool Optional { get; private set; }
    }
}
