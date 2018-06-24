using System;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors.Remoting.V2.FabricTransport.Client;

namespace ServiceFabric.Remoting.CustomHeaders.Actors
{
    /// <summary>
    /// Class to create an <see cref="ActorProxy"/>>
    /// </summary>
    public class ExtendedActorProxy
    {
        /// <summary>
        /// Creates an <see cref="ActorProxy"/>
        /// </summary>
        /// <typeparam name="TActorInterface">The type of the actor to create</typeparam>
        /// <param name="actorId">The id of the actor to address</param>
        /// <param name="customHeaders">A <see cref="CustomHeaders"/> instance with data passed to the actor</param>
        /// <param name="applicationName"></param>
        /// <param name="serviceName"></param>
        /// <param name="listenerName"></param>
        /// <returns>An actor proxy object that implements IActorProxy and TActorInterface.</returns>
        public static TActorInterface Create<TActorInterface>(ActorId actorId, CustomHeaders customHeaders = null, string applicationName = null, string serviceName = null, string listenerName = null) where TActorInterface : IActor
        {
            return Create<TActorInterface>(actorId, () => customHeaders, applicationName, serviceName, listenerName);
        }

        /// <summary>
        /// Creates an <see cref="ActorProxy"/>
        /// </summary>
        /// <typeparam name="TActorInterface">The type of the actor to create</typeparam>
        /// <param name="actorId">The id of the actor to address</param>
        /// <param name="customHeaderProvider">A factory to create a <see cref="CustomHeaders"/> instance with data passed to the actor</param>
        /// <param name="applicationName"></param>
        /// <param name="serviceName"></param>
        /// <param name="listenerName"></param>
        /// <returns>An actor proxy object that implements IActorProxy and TActorInterface.</returns>
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