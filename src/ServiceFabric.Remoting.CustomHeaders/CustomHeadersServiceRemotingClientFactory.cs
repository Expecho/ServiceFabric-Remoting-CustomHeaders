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
    public class CustomHeadersServiceRemotingClientFactory : IServiceRemotingClientFactory
    {
        public event EventHandler<CommunicationClientEventArgs<IServiceRemotingClient>> ClientConnected;
        public event EventHandler<CommunicationClientEventArgs<IServiceRemotingClient>> ClientDisconnected;

        private readonly CustomHeaders customHeaders;
        private readonly IServiceRemotingClientFactory serviceRemotingClientFactory;

        public CustomHeadersServiceRemotingClientFactory(IServiceRemotingClientFactory serviceRemotingClientFactory, CustomHeaders customHeaders = null)
        {
            this.customHeaders = customHeaders ?? new CustomHeaders();
            this.serviceRemotingClientFactory = serviceRemotingClientFactory;
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
            return new ServiceRemotingClientWrapper(client, customHeaders);
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
            return new ServiceRemotingClientWrapper(client, customHeaders);
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
            private readonly CustomHeaders customHeaders;

            public ServiceRemotingClientWrapper(IServiceRemotingClient client, CustomHeaders customHeaders)
            {
                Client = client;
                this.customHeaders = customHeaders ?? new CustomHeaders();
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
                foreach (var customHeader in customHeaders)
                {
                    byte[] headerValue = Encoding.ASCII.GetBytes(customHeader.Value);
                    header.AddHeader(customHeader.Key, headerValue);
                }
                
                return Client.RequestResponseAsync(requestRequestMessage);
            }

            public void SendOneWay(IServiceRemotingRequestMessage requestMessage)
            {
                var header = requestMessage.GetHeader();
                foreach (var customHeader in customHeaders)
                {
                    byte[] headerValue = Encoding.ASCII.GetBytes(customHeader.Value);
                    header.AddHeader(customHeader.Key, headerValue);
                }

                Client.SendOneWay(requestMessage);
            }
        }
    }
}
