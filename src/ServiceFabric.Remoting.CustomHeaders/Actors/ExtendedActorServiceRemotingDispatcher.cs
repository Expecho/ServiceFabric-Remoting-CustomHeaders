using System;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors.Remoting.V2;
using Microsoft.ServiceFabric.Actors.Remoting.V2.Runtime;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.V2;
using Microsoft.ServiceFabric.Services.Remoting.V2.Runtime;
using ServiceFabric.Remoting.CustomHeaders.Util;

namespace ServiceFabric.Remoting.CustomHeaders.Actors
{
    /// <summary>
    /// <see cref="ActorServiceRemotingDispatcher"/> that operates on the receiving side
    /// </summary>
    public class ExtendedActorServiceRemotingDispatcher : ActorServiceRemotingDispatcher
    {
        /// <inheritdoc/>
        public ExtendedActorServiceRemotingDispatcher(ActorService actorService, IServiceRemotingMessageBodyFactory serviceRemotingRequestMessageBodyFactory)
            : base(actorService, serviceRemotingRequestMessageBodyFactory)
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
            var handleExtended = false;
            Uri serviceUri = null;

            object state = null;
            Exception exception = null;

            var header = requestMessage.GetHeader() as IActorRemotingMessageHeaders;
            //check to see if message headers are assignable
            if (header !=null)
            {
                handleExtended = true;
                serviceUri = (Uri)header.GetCustomHeaders()[CustomHeaders.ReservedHeaderServiceUri];
                RemotingContext.FromRemotingMessageHeader(header);
            }

            if (BeforeHandleRequestResponseAsync != null && handleExtended)
                state = await BeforeHandleRequestResponseAsync.Invoke(new ActorRequestInfo(requestMessage, header.ActorId, header.MethodName, serviceUri));

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
                if (AfterHandleRequestResponseAsync != null && handleExtended)
                    await AfterHandleRequestResponseAsync.Invoke(new ActorResponseInfo(responseMessage, header.ActorId, header.MethodName, serviceUri, state, exception));
            }
            
            return responseMessage;
        }

        /// <summary>
        /// Optional hook to provide code executed before the message is handled by the client
        /// </summary>
        /// <returns>object: state</returns>
        public Func<ActorRequestInfo, Task<object>> BeforeHandleRequestResponseAsync { get; set; }

        /// <summary>
        /// Optional hook to provide code executed afer the message is handled by the client
        /// </summary>
        public Func<ActorResponseInfo, Task> AfterHandleRequestResponseAsync { get; set; }
    }
}
