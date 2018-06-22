using System;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors.Remoting.V2.FabricTransport.Client;

namespace ServiceFabric.Remoting.CustomHeaders.Actors
{
    public class CustomHeadersActorProxy
    {
        public static TActorInterface CreateActorProxy<TActorInterface>(ActorId actorId, CustomHeaders customHeaders, string applicationName = null, string serviceName = null, string listenerName = null) where TActorInterface : IActor
        {
            return Create<TActorInterface>(actorId, () => customHeaders, applicationName, serviceName, listenerName);
        }

        public static TActorInterface Create<TActorInterface>(ActorId actorId, Func<CustomHeaders> customHeaderProvider, string applicationName = null, string serviceName = null, string listenerName = null) where TActorInterface : IActor
        {
            var proxyFactory = new ActorProxyFactory(handler =>
                new CustomHeadersServiceRemotingClientFactory(
                    new FabricTransportActorRemotingClientFactory(handler), customHeaderProvider));
            return proxyFactory.CreateActorProxy<TActorInterface>(actorId, applicationName, serviceName, listenerName);
        }
    }
}