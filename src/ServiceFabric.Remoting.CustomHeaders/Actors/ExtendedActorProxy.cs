using System;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors.Remoting.V2.FabricTransport.Client;

namespace ServiceFabric.Remoting.CustomHeaders.Actors
{
    public class ExtendedActorProxy
    {
        public static TActorInterface CreateActorProxy<TActorInterface>(ActorId actorId, CustomHeaders customHeaders = null, string applicationName = null, string serviceName = null, string listenerName = null) where TActorInterface : IActor
        {
            return Create<TActorInterface>(actorId, () => customHeaders, applicationName, serviceName, listenerName);
        }

        public static TActorInterface Create<TActorInterface>(ActorId actorId, Func<CustomHeaders> customHeaderProvider, string applicationName = null, string serviceName = null, string listenerName = null) where TActorInterface : IActor
        {
            var methodNameProvider = new MethodNameProvider();

            var proxyFactory = new ActorProxyFactory(handler =>
                new ExtendedServiceRemotingClientFactory(
                    new FabricTransportActorRemotingClientFactory(handler), customHeaderProvider, methodNameProvider));
            var proxy = proxyFactory.CreateActorProxy<TActorInterface>(actorId, applicationName, serviceName, listenerName);
            methodNameProvider.AddMethodsForProxyOrService(proxy.GetType().GetInterfaces(), typeof(IActor));
            return proxy;
        }
    }
}