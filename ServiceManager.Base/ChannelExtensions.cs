using ServiceManager.Base.wcf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ServiceManager.Base
{
    public static class ChannelExtensions
    {

        public static void Try(this Channel channel, Action<ICallbacks> method)
        {
            if (channel.ServerChannel == null)
                return;


            try
            {
                var t = new Task(delegate
                {
                    method(channel.ServerChannel);
                });

                t.Start();

                t.Wait();
            }
            catch(CommunicationObjectAbortedException ex)
            {
                channel.Failed = true;
            }
            catch(AggregateException ex)
            {
                if(ex.InnerException.GetType() == typeof(CommunicationObjectAbortedException))
                {
                    channel.Failed = true;
                }
            }
            catch
            {

            }
        }


    }

}
