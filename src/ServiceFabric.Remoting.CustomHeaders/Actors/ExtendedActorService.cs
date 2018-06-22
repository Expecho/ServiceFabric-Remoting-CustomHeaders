using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Remoting.V2.FabricTransport.Runtime;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.V2;

namespace ServiceFabric.Remoting.CustomHeaders.Actors
{
    public class ExtendedActorService : ActorService
    {
        public ExtendedActorService(StatefulServiceContext context, ActorTypeInformation actorTypeInfo, Func<ActorService, ActorId, ActorBase> actorFactory = null, Func<ActorBase, IActorStateProvider, IActorStateManager> stateManagerFactory = null, IActorStateProvider stateProvider = null, ActorServiceSettings settings = null) : base(context, actorTypeInfo, actorFactory, stateManagerFactory, stateProvider, settings)
        {
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            yield return new ServiceReplicaListener(
                context =>
                {
                    var messageBodyFactory =
                        new ServiceRemotingDataContractSerializationProvider().CreateMessageBodyFactory();
                    var messageDispatcher = new ExtendedActorServiceRemotingDispatcher(this, messageBodyFactory)
                    {
                        BeforeHandleRequestResponseAsync = BeforeHandleRequestResponseAsync,
                        AfterHandleRequestResponseAsync = AfterHandleRequestResponseAsync
                    };
                    return new FabricTransportActorServiceRemotingListener(context, messageDispatcher);
                }, "V2Listener");
        }

        public Func<IServiceRemotingRequestMessage, ActorId, Task> BeforeHandleRequestResponseAsync { get; set; }

        public Func<IServiceRemotingResponseMessage, ActorId, Task> AfterHandleRequestResponseAsync { get; set; }
    }
}