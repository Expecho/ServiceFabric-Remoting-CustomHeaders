using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.V2;
using Microsoft.ServiceFabric.Services.Remoting.V2.Runtime;

namespace ServiceFabric.Remoting.CustomHeaders.ReliableServices
{
    public class ExtendedServiceRemotingMessageDispatcher : ServiceRemotingMessageDispatcher
    {
        public ExtendedServiceRemotingMessageDispatcher(ServiceContext serviceContext, IService service)
            : this(serviceContext, service, null)
        {
            
        }

        public ExtendedServiceRemotingMessageDispatcher(ServiceContext serviceContext, IService service, IServiceRemotingMessageBodyFactory serviceRemotingMessageBodyFactory = null)
            : base(serviceContext, service, serviceRemotingMessageBodyFactory)
        {
       
        }

        public ExtendedServiceRemotingMessageDispatcher(IEnumerable<Type> remotingTypes, ServiceContext serviceContext, object serviceImplementation, IServiceRemotingMessageBodyFactory serviceRemotingMessageBodyFactory = null)
            : base(remotingTypes, serviceContext, serviceImplementation, serviceRemotingMessageBodyFactory)
        {
        }

        public override void HandleOneWayMessage(IServiceRemotingRequestMessage requestMessage)
        {
            RemotingContext.FromRemotingMessage(requestMessage);
            base.HandleOneWayMessage(requestMessage);
        }

        public override async Task<IServiceRemotingResponseMessage> HandleRequestResponseAsync(IServiceRemotingRequestContext requestContext,
            IServiceRemotingRequestMessage requestMessage)
        {
            RemotingContext.FromRemotingMessage(requestMessage);
            if (BeforeHandleRequestResponseAsync != null)
                await BeforeHandleRequestResponseAsync.Invoke(requestMessage);
            var responseMessage = await base.HandleRequestResponseAsync(requestContext, requestMessage);
            if (AfterHandleRequestResponseAsync != null)
                await AfterHandleRequestResponseAsync.Invoke(responseMessage);

            return responseMessage;
        }

        public Func<IServiceRemotingRequestMessage, Task> BeforeHandleRequestResponseAsync { get; set; }

        public Func<IServiceRemotingResponseMessage, Task> AfterHandleRequestResponseAsync { get; set; }
    }
}
