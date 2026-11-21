using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceManager.Base.Data.Settings
{
    public class Runtime
    {
        

        public enum eMode
        {
            NetPipes = 0,
            Tcp = 1
        }


        public eMode Mode { get; set; }

        public int TCPPort { get; set; }

        /// <summary>
        /// Delay of service startup in seconds.
        /// </summary>
        public int Startup_Delay { get; set; }

        /// <summary>
        /// Only members of the Administrators group may connect to the service. Kept beside the new
        /// rule so an older version reads the same file.
        /// </summary>
        public bool RequireAdministrator { get; set; }

        /// <summary>
        /// Who may connect to the endpoint. Null means a settings file written before this property
        /// existed, and RequireAdministrator answers for that file.
        /// </summary>
        public Channel.eAccess? Access { get; set; }

        /// <summary>
        /// The accounts that may connect with a ServiceManager password. They are only asked for while
        /// the rule is Users.
        /// </summary>
        public List<EndpointUser> Users { get; set; } = new List<EndpointUser>();

        /// <summary>
        /// How long a service waits for a Windows dependency that is not running, in seconds. Zero starts
        /// nothing while a dependency is missing.
        /// </summary>
        public int Dependency_Wait_Timeout { get; set; }

        public Runtime()
        {
            this.Mode = eMode.NetPipes;
            this.TCPPort = 50000;
            this.Startup_Delay = 0;
            this.RequireAdministrator = true;
            this.Dependency_Wait_Timeout = 120;
        }

        /// <summary>
        /// The rule the endpoint answers with. A file without the new property is answered through the
        /// old one, so an installation keeps the rule it has always had.
        /// </summary>
        public Channel.eAccess ResolveAccess()
        {
            if (this.Access.HasValue)
            {
                return this.Access.Value;
            }

            return this.RequireAdministrator ? Channel.eAccess.Administrators : Channel.eAccess.Everyone;
        }

        /// <summary>
        /// Stores the rule. The old property is written alongside it so an older version reads the same
        /// file the same way.
        /// </summary>
        public void ApplyAccess(Channel.eAccess Access)
        {
            this.Access = Access;

            this.RequireAdministrator = Access != Channel.eAccess.Everyone;
        }

    }
}
