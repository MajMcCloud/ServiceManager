using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceManager.Base.data.settings
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

        public Runtime()
        {
            this.Mode = eMode.NetPipes;
            this.TCPPort = 50000;
        }

    }
}
