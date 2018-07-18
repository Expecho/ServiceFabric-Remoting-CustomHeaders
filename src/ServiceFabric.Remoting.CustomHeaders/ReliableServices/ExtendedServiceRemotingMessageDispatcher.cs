using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.V2;
using Microsoft.ServiceFabric.Services.Remoting.V2.Runtime;
using ServiceFabric.Remoting.CustomHeaders.Util;

namespace ServiceFabric.Remoting.CustomHeaders.ReliableServices
{
    /// <summary>
    /// <see cref="ServiceRemotingMessageDispatcher"/> that operates on the receiving side
    /// </summary>
    public class ExtendedServiceRemotingMessageDispatcher : ServiceRemotingMessageDispatcher
    {
        /// <inheritdoc/>
        public ExtendedServiceRemotingMessageDispatcher(ServiceContext serviceContext, IService service)
            : this(serviceContext, service, null)
        {
            
        }

        /// <inheritdoc/>
        public ExtendedServiceRemotingMessageDispatcher(ServiceContext serviceContext, IService service, IServiceRemotingMessageBodyFactory serviceRemotingMessageBodyFactory = null)
            : base(serviceContext, service, serviceRemotingMessageBodyFactory)
        {
       
        }

        /// <inheritdoc/>
        public ExtendedServiceRemotingMessageDispatcher(IEnumerable<Type> remotingTypes, ServiceContext serviceContext, object serviceImplementation, IServiceRemotingMessageBodyFactory serviceRemotingMessageBodyFactory = null)
            : base(remotingTypes, serviceContext, serviceImplementation, serviceRemotingMessageBodyFactory)
        {
        }

        /// <inheritdoc/>
        public override void HandleOneWayMessage(IServiceRemotingRequestMessage requestMessage)
        {
            RemotingContext.FromRemotingMessageHeader(requestMessage.GetHeader());
            base.HandleOneWayMessage(requestMessage);
        }

        /// <inheritdoc/>
        public override async Task<IServiceRemotingResponseMessage> HandleRequestResponseAsync(IServiceRemotingRequestContext requestContext,
            IServiceRemotingRequestMessage requestMessage)
        {
            var header = requestMessage.GetHeader();

            var serviceUri = (Uri) header.GetCustomHeaders()[CustomHeaders.ReservedHeaderServiceUri];
            
            RemotingContext.FromRemotingMessageHeader(header);

            object state = null;
            Exception exception = null;

            if (BeforeHandleRequestResponseAsync != null)
                state = await BeforeHandleRequestResponseAsync.Invoke(new ServiceRequestInfo(requestMessage, header.MethodName, serviceUri));

            IServiceRemotingResponseMessage responseMessage = null;

            try
            {
                responseMessage = await base.HandleRequestResponseAsync(requestContext, requestMessage);
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                if (AfterHandleRequestResponseAsync != null)
                    await AfterHandleRequestResponseAsync.Invoke(new ServiceResponseInfo(responseMessage, header.MethodName, serviceUri, state, exception));
            }

            return responseMessage;
        }

        /// <summary>
        /// Optional hook to provide code executed before the message is handled by the client
        /// </summary>
        /// <returns>object: state</returns>
        public Func<ServiceRequestInfo, Task<object>> BeforeHandleRequestResponseAsync { get; set; }

        /// <summary>
        /// Optional hook to provide code executed after the message is handled by the client
        /// </summary>
        public Func<ServiceResponseInfo, Task> AfterHandleRequestResponseAsync { get; set; }
    }
}
