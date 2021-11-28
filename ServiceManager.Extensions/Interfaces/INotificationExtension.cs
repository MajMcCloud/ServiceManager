using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceManager.Extensions.Interfaces
{
    public interface INotificationExtension
    {
        Guid Id { get; }

        String Name { get; }

        String Description { get; }

        


        Task Send(String message);

        void Configure();


    }
}
