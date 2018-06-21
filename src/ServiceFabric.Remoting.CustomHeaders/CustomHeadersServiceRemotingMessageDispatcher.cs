using System;
using System.Collections.Generic;
using System.Fabric;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.V2;
using Microsoft.ServiceFabric.Services.Remoting.V2.Runtime;

namespace ServiceFabric.Remoting.CustomHeaders
{
    public class CustomHeadersServiceRemotingMessageDispatcher : ServiceRemotingMessageDispatcher
    {
        public CustomHeadersServiceRemotingMessageDispatcher(ServiceContext serviceContext, IService service)
            : this(serviceContext, service, null)
        {
            
        }

        public CustomHeadersServiceRemotingMessageDispatcher(ServiceContext serviceContext, IService service, IServiceRemotingMessageBodyFactory serviceRemotingMessageBodyFactory = null)
            : base(serviceContext, service, serviceRemotingMessageBodyFactory)
        {
       
        }

        public CustomHeadersServiceRemotingMessageDispatcher(IEnumerable<Type> remotingTypes, ServiceContext serviceContext, object serviceImplementation, IServiceRemotingMessageBodyFactory serviceRemotingMessageBodyFactory = null)
            : base(remotingTypes, serviceContext, serviceImplementation, serviceRemotingMessageBodyFactory)
        {
        }

        public override void HandleOneWayMessage(IServiceRemotingRequestMessage requestMessage)
        {
            SetCallContext(requestMessage);
            base.HandleOneWayMessage(requestMessage);
        }

        public override Task<IServiceRemotingResponseMessage> HandleRequestResponseAsync(IServiceRemotingRequestContext requestContext,
            IServiceRemotingRequestMessage requestMessage)
        {
            SetCallContext(requestMessage);
            return base.HandleRequestResponseAsync(requestContext, requestMessage);
        }

        private static void SetCallContext(IServiceRemotingRequestMessage requestMessage)
        {
            var header = requestMessage.GetHeader();
            var headers = (Dictionary<string, byte[]>)header.GetType().GetField("headers",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.GetValue(header);

            if (headers == null)
                return;

            foreach (var customHeader in headers)
            {
                if (customHeader.Key != null)
                {
                    RemotingContext.SetData(customHeader.Key, Encoding.ASCII.GetString(customHeader.Value));
                }
            }
        }
    }
}
