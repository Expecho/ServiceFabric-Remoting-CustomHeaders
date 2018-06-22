using System;
using System.Collections.Generic;
using System.Fabric;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Remoting.V2.FabricTransport.Runtime;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.V2;

namespace ServiceFabric.Remoting.CustomHeaders.Actors
{
    public class CustomHeadersActorService : ActorService
    {
        public CustomHeadersActorService(StatefulServiceContext context, ActorTypeInformation actorTypeInfo, Func<ActorService, ActorId, ActorBase> actorFactory = null, Func<ActorBase, IActorStateProvider, IActorStateManager> stateManagerFactory = null, IActorStateProvider stateProvider = null, ActorServiceSettings settings = null) : base(context, actorTypeInfo, actorFactory, stateManagerFactory, stateProvider, settings)
        {
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            yield return new ServiceReplicaListener(
                context =>
                {
                    var messageDispatcher = new CustomHeadersActorMessageHandler(this, new ServiceRemotingDataContractSerializationProvider().CreateMessageBodyFactory());
                    return new FabricTransportActorServiceRemotingListener(context, messageDispatcher);
                }, "V2Listener");
        }
    }
}