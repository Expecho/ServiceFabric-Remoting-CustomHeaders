using System;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors.Remoting.V2;
using Microsoft.ServiceFabric.Actors.Remoting.V2.Runtime;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.V2;
using Microsoft.ServiceFabric.Services.Remoting.V2.Runtime;

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
            var header = (IActorRemotingMessageHeaders)requestMessage.GetHeader();
            var methodName = $"{header.InterfaceId}.{header.MethodId}";

            RemotingContext.FromRemotingMessageHeader(header);
            object state = null;
            if (BeforeHandleRequestResponseAsync != null)
                state = await BeforeHandleRequestResponseAsync.Invoke(new ActorRequestInfo(requestMessage, header.ActorId, methodName));
            var responseMessage = await base.HandleRequestResponseAsync(requestContext, requestMessage);
            if (AfterHandleRequestResponseAsync != null)
                await AfterHandleRequestResponseAsync.Invoke(new ActorResponseInfo(responseMessage, header.ActorId, methodName, state));

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
