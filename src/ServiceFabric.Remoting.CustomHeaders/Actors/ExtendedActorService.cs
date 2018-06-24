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
    /// <summary>
    /// An actor service that support custom message headers
    /// </summary>
    public class ExtendedActorService : ActorService
    {
        /// <summary>
        /// Creates an instance of ExtendedActorService
        /// </summary>
        /// <param name="context"></param>
        /// <param name="actorTypeInfo"></param>
        /// <param name="actorFactory"></param>
        /// <param name="stateManagerFactory"></param>
        /// <param name="stateProvider"></param>
        /// <param name="settings"></param>
        public ExtendedActorService(StatefulServiceContext context, ActorTypeInformation actorTypeInfo, Func<ActorService, ActorId, ActorBase> actorFactory = null, Func<ActorBase, IActorStateProvider, IActorStateManager> stateManagerFactory = null, IActorStateProvider stateProvider = null, ActorServiceSettings settings = null) : base(context, actorTypeInfo, actorFactory, stateManagerFactory, stateProvider, settings)
        {
        }

        /// <inheritdoc/>
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

        /// <summary>
        /// Optional hook to provide code executed before the message is handled by the client
        /// IServiceRemotingRequestMessage: the message
        /// ActorId: the actor id
        /// string: the method name
        /// </summary>
        /// <returns>object: state</returns>
        public Func<IServiceRemotingRequestMessage, ActorId, string, Task<object>> BeforeHandleRequestResponseAsync { get; set; }

        /// <summary>
        /// Optional hook to provide code executed afer the message is handled by the client
        /// IServiceRemotingResponseMessage: the message
        /// ActorId: the actor id
        /// string: the method name
        /// object: state
        /// </summary>
        public Func<IServiceRemotingResponseMessage, ActorId, string, object, Task> AfterHandleRequestResponseAsync { get; set; }
    }
}