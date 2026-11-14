using System;
using System.Collections.Generic;

namespace ServiceManager.Base.Data
{
    /// <summary>
    /// One row of the dependency tree. The node carries no logic: it is what <see cref="DependencyTree"/>
    /// hands to the window, so the window only has to draw it and to mark what it is told to mark.
    /// </summary>
    public class DependencyTreeNode
    {
        public enum eNodeKind
        {
            Service = 0,
            WindowsService = 1,
            Host = 2
        }

        public eNodeKind Kind { get; set; } = eNodeKind.Service;

        public String Title { get; set; }

        /// <summary>The id of the service in the settings file, Guid.Empty for a Windows service.</summary>
        public Guid ID { get; set; }

        /// <summary>False when the service is switched off in the settings file. It stays in the tree.</summary>
        public bool Enabled { get; set; } = true;

        /// <summary>True when the service sits on a circle in the settings file.</summary>
        public bool InCycle { get; set; }

        /// <summary>
        /// True when the service is already shown above. This is where a circle closes, and the node is the
        /// end of the branch: following it would walk the circle for ever.
        /// </summary>
        public bool IsRepeat { get; set; }

        public List<DependencyTreeNode> Children { get; set; } = new List<DependencyTreeNode>();
    }
}
