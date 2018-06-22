using System;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Remoting.V2;
using Microsoft.ServiceFabric.Actors.Remoting.V2.Runtime;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.V2;
using Microsoft.ServiceFabric.Services.Remoting.V2.Runtime;

namespace ServiceFabric.Remoting.CustomHeaders.Actors
{
    public class ExtendedActorServiceRemotingDispatcher : ActorServiceRemotingDispatcher
    {
        public ExtendedActorServiceRemotingDispatcher(ActorService actorService, IServiceRemotingMessageBodyFactory serviceRemotingRequestMessageBodyFactory)
            : base(actorService, serviceRemotingRequestMessageBodyFactory)
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
            var header = (IActorRemotingMessageHeaders)requestMessage.GetHeader();
            
            RemotingContext.FromRemotingMessage(requestMessage);
            if(BeforeHandleRequestResponseAsync != null)
                await BeforeHandleRequestResponseAsync.Invoke(requestMessage, header.ActorId);
            var responseMessage = await base.HandleRequestResponseAsync(requestContext, requestMessage);
            if (AfterHandleRequestResponseAsync != null)
                await AfterHandleRequestResponseAsync.Invoke(responseMessage, header.ActorId);

            return responseMessage;
        }

        public Func<IServiceRemotingRequestMessage, ActorId, Task> BeforeHandleRequestResponseAsync { get; set; }

        public Func<IServiceRemotingResponseMessage, ActorId, Task> AfterHandleRequestResponseAsync { get; set; }
    }
}
