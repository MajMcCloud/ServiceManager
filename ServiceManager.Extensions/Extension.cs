using ServiceManager.Extensions.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceManager.Extensions
{
    public  class Extension
    {
        public String Name { get; set; }

        public Guid Id { get; set; }

        public String Path { get; set; }    

        public INotificationExtension Notification { get; set; }

    }
}
