using System;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;

namespace ServiceFabric.Remoting.CustomHeaders
{
    public class CustomHeaderServiceProxy
    {
        public static TServiceInterface Create<TServiceInterface>(Uri serviceUri, CustomHeaders customHeaders,
            ServicePartitionKey partitionKey = null,
            TargetReplicaSelector targetReplicaSelector = TargetReplicaSelector.Default, string listenerName = null)
            where TServiceInterface : IService
        {
            var proxyFactory = new ServiceProxyFactory(handler =>
                new CustomHeadersServiceRemotingClientFactory(
                    new FabricTransportServiceRemotingClientFactory(remotingCallbackMessageHandler: handler), customHeaders));
            return proxyFactory.CreateServiceProxy<TServiceInterface>(serviceUri); //, partitionKey, targetReplicaSelector, listenerName);
        }
    }
}