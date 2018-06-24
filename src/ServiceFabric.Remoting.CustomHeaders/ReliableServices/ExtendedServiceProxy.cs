using System;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;

namespace ServiceFabric.Remoting.CustomHeaders.ReliableServices
{
    /// <summary>
    /// Class to create an <see cref="ServiceProxy"/>>
    /// </summary>
    public class ExtendedServiceProxy
    {
        /// <summary>
        /// Creates an <see cref="ServiceProxy"/>
        /// </summary>
        /// <typeparam name="TServiceInterface">The type of service to create</typeparam>
        /// <param name="serviceUri">The uri of the service</param>
        /// <param name="customHeaders">A <see cref="CustomHeaders"/> instance with data passed to the service</param>
        /// <param name="partitionKey"></param>
        /// <param name="targetReplicaSelector"></param>
        /// <param name="listenerName"></param>
        /// <returns>A service proxy object that implements IServiceProxy and TServiceInterface.</returns>
        public static TServiceInterface Create<TServiceInterface>(Uri serviceUri, CustomHeaders customHeaders = null,
            ServicePartitionKey partitionKey = null,
            TargetReplicaSelector targetReplicaSelector = TargetReplicaSelector.Default, string listenerName = null)
            where TServiceInterface : IService
        {
            return Create<TServiceInterface>(serviceUri, () => customHeaders, partitionKey, targetReplicaSelector, listenerName);
        }

        /// <summary>
        /// Creates an <see cref="ServiceProxy"/>
        /// </summary>
        /// <typeparam name="TServiceInterface">The type of service to create</typeparam>
        /// <param name="serviceUri">The uri of the service</param>
        /// <param name="customHeaderProvider">A factory to create a <see cref="CustomHeaders"/> instance with data passed to the service</param>
        /// <param name="partitionKey"></param>
        /// <param name="targetReplicaSelector"></param>
        /// <param name="listenerName"></param>
        /// <returns>A service proxy object that implements IServiceProxy and TServiceInterface.</returns>
        public static TServiceInterface Create<TServiceInterface>(Uri serviceUri, Func<CustomHeaders> customHeaderProvider,
            ServicePartitionKey partitionKey = null,
            TargetReplicaSelector targetReplicaSelector = TargetReplicaSelector.Default, string listenerName = null)
            where TServiceInterface : IService
        {
            var methodNameProvider = new MethodNameProvider();

            var proxyFactory = new ServiceProxyFactory(handler =>
                new ExtendedServiceRemotingClientFactory(
                    new FabricTransportServiceRemotingClientFactory(remotingCallbackMessageHandler: handler), customHeaderProvider, methodNameProvider));
            var proxy = proxyFactory.CreateServiceProxy<TServiceInterface>(serviceUri, partitionKey, targetReplicaSelector, listenerName);
            methodNameProvider.AddMethodsForProxyOrService(proxy.GetType().GetInterfaces(), typeof(IService));
            return proxy;
        }
    }
}