using System;
using System.Collections.Generic;
using System.Fabric;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.V2;
using Microsoft.ServiceFabric.Services.Remoting.V2.Runtime;

namespace ServiceFabric.Remoting.CustomHeaders.ReliableServices
{
    /// <summary>
    /// <see cref="ServiceRemotingMessageDispatcher"/> that operates on the receiving side
    /// </summary>
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
            var header = requestMessage.GetHeader();
            string methodName = string.Empty;
            if (header.TryGetHeaderValue(CustomHeaders.MethodHeader, out byte[] headerValue))
            {
                methodName = Encoding.ASCII.GetString(headerValue);
            }

            RemotingContext.FromRemotingMessage(requestMessage);
            if (BeforeHandleRequestResponseAsync != null)
                await BeforeHandleRequestResponseAsync.Invoke(requestMessage, methodName);
            var responseMessage = await base.HandleRequestResponseAsync(requestContext, requestMessage);
            if (AfterHandleRequestResponseAsync != null)
                await AfterHandleRequestResponseAsync.Invoke(responseMessage, methodName);

            return responseMessage;
        }

        public Func<IServiceRemotingRequestMessage, string, Task> BeforeHandleRequestResponseAsync { get; set; }

        public Func<IServiceRemotingResponseMessage, string, Task> AfterHandleRequestResponseAsync { get; set; }
    }
}
