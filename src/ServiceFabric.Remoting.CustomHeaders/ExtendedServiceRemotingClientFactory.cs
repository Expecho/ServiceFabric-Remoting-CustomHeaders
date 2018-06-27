using System;
using System.Fabric;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2;
using Microsoft.ServiceFabric.Services.Remoting.V2.Client;

namespace ServiceFabric.Remoting.CustomHeaders
{
    /// <summary>
    /// <see cref="IServiceRemotingClientFactory"/> that operates on the sending side
    /// </summary>
    internal class ExtendedServiceRemotingClientFactory : IServiceRemotingClientFactory
    {
        /// <inheritdoc/>
        public event EventHandler<CommunicationClientEventArgs<IServiceRemotingClient>> ClientConnected;

        /// <inheritdoc/>
        public event EventHandler<CommunicationClientEventArgs<IServiceRemotingClient>> ClientDisconnected;

        private readonly IServiceRemotingClientFactory serviceRemotingClientFactory;
        private readonly Func<CustomHeaders> customHeadersProvider;
        private readonly MethodNameProvider methodNameProvider;

        public ExtendedServiceRemotingClientFactory(IServiceRemotingClientFactory serviceRemotingClientFactory, Func<CustomHeaders> customHeadersProvider, MethodNameProvider methodNameProvider)
        {
            this.serviceRemotingClientFactory = serviceRemotingClientFactory;
            this.customHeadersProvider = customHeadersProvider;
            this.methodNameProvider = methodNameProvider;
        }

        public async Task<IServiceRemotingClient> GetClientAsync(
            ResolvedServicePartition previousRsp,
            TargetReplicaSelector targetReplicaSelector,
            string listenerName,
            OperationRetrySettings retrySettings,
            CancellationToken cancellationToken)
        {
            var client = await serviceRemotingClientFactory.GetClientAsync(
                previousRsp,
                targetReplicaSelector,
                listenerName,
                retrySettings,
                cancellationToken);
            return new ServiceRemotingClientWrapper(client, customHeadersProvider, methodNameProvider);
        }

        public async Task<IServiceRemotingClient> GetClientAsync(
            Uri serviceUri,
            ServicePartitionKey partitionKey,
            TargetReplicaSelector targetReplicaSelector,
            string listenerName,
            OperationRetrySettings retrySettings,
            CancellationToken cancellationToken)
        {
            var client = await serviceRemotingClientFactory.GetClientAsync(
                serviceUri,
                partitionKey,
                targetReplicaSelector,
                listenerName,
                retrySettings,
                cancellationToken);
            return new ServiceRemotingClientWrapper(client, customHeadersProvider, methodNameProvider);
        }

        public Task<OperationRetryControl> ReportOperationExceptionAsync(
            IServiceRemotingClient client,
            ExceptionInformation exceptionInformation,
            OperationRetrySettings retrySettings,
            CancellationToken cancellationToken)
        {
            return serviceRemotingClientFactory.ReportOperationExceptionAsync(
                ((ServiceRemotingClientWrapper)client).Client,
                exceptionInformation,
                retrySettings,
                cancellationToken);
        }

        public IServiceRemotingMessageBodyFactory GetRemotingMessageBodyFactory()
        {
            return serviceRemotingClientFactory.GetRemotingMessageBodyFactory();
        }

        private class ServiceRemotingClientWrapper : IServiceRemotingClient
        {
            private readonly Func<CustomHeaders> customHeadersProvider;
            private readonly MethodNameProvider methodNameProvider;

            public ServiceRemotingClientWrapper(IServiceRemotingClient client, Func<CustomHeaders> customHeadersProvider, MethodNameProvider methodNameProvider)
            {
                Client = client;
                this.customHeadersProvider = customHeadersProvider;
                this.methodNameProvider = methodNameProvider;
            }

            internal IServiceRemotingClient Client { get; }

            public ResolvedServiceEndpoint Endpoint
            {
                get => Client.Endpoint;
                set => Client.Endpoint = value;
            }

            public string ListenerName
            {
                get => Client.ListenerName;
                set => Client.ListenerName = value;
            }

            public ResolvedServicePartition ResolvedServicePartition
            {
                get => Client.ResolvedServicePartition;
                set => Client.ResolvedServicePartition = value;
            }

            public Task<IServiceRemotingResponseMessage> RequestResponseAsync(IServiceRemotingRequestMessage requestRequestMessage)
            {
                var header = requestRequestMessage.GetHeader();
                var customHeaders = customHeadersProvider.Invoke() ?? new CustomHeaders();

                header.AddHeader(CustomHeaders.MethodHeader, Encoding.ASCII.GetBytes(methodNameProvider.GetMethodName(header.InterfaceId, header.MethodId)));
                header.AddHeader(CustomHeaders.CustomHeader, customHeaders.Serialize());

                return Client.RequestResponseAsync(requestRequestMessage);
            }

            public void SendOneWay(IServiceRemotingRequestMessage requestMessage)
            {
                var header = requestMessage.GetHeader();
                var customHeaders = customHeadersProvider.Invoke() ?? new CustomHeaders();
                header.AddHeader(CustomHeaders.MethodHeader, Encoding.ASCII.GetBytes(methodNameProvider.GetMethodName(header.InterfaceId, header.MethodId)));
                header.AddHeader(CustomHeaders.CustomHeader, customHeaders.Serialize());

                Client.SendOneWay(requestMessage);
            }
        }
    }
}
