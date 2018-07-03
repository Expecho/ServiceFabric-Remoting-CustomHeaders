using System;
using System.ComponentModel.Design;
using System.Fabric;
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
    public class ExtendedServiceRemotingClientFactory : IServiceRemotingClientFactory
    {
        /// <inheritdoc/>
        public event EventHandler<CommunicationClientEventArgs<IServiceRemotingClient>> ClientConnected;

        /// <inheritdoc/>
        public event EventHandler<CommunicationClientEventArgs<IServiceRemotingClient>> ClientDisconnected;

        private readonly IServiceRemotingClientFactory serviceRemotingClientFactory;
        private readonly Func<CustomHeaders> customHeadersProvider;

        /// <summary>
        /// Optional hook to provide code executed before the message is send to the client
        /// IServiceRemotingRequestMessage: the message
        /// string: the method name
        /// </summary>
        /// <returns>object: state</returns>
        public Func<IServiceRemotingRequestMessage, string, Task<object>> BeforeSendRequestResponseAsync { get; set; }

        /// <summary>
        /// Optional hook to provide code executed after the message is send to the client
        /// IServiceRemotingResponseMessage: the message
        /// string: the method name
        /// object: state
        /// </summary>
        public Func<IServiceRemotingResponseMessage, string, object, Task> AfterSendRequestResponseAsync { get; set; }

        /// <summary>
        /// Creates a new instance
        /// </summary>
        /// <param name="serviceRemotingClientFactory"></param>
        /// <param name="customHeaders"></param>
        public ExtendedServiceRemotingClientFactory(IServiceRemotingClientFactory serviceRemotingClientFactory, CustomHeaders customHeaders) :
            this(serviceRemotingClientFactory, () => customHeaders)
        {
        }

        /// <summary>
        /// Creates a new instance
        /// </summary>
        /// <param name="serviceRemotingClientFactory"></param>
        /// <param name="customHeadersProvider"></param>
        public ExtendedServiceRemotingClientFactory(IServiceRemotingClientFactory serviceRemotingClientFactory, Func<CustomHeaders> customHeadersProvider)
        {
            this.serviceRemotingClientFactory = serviceRemotingClientFactory;
            this.customHeadersProvider = customHeadersProvider;
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
            return new ServiceRemotingClientWrapper(client, customHeadersProvider, BeforeSendRequestResponseAsync, AfterSendRequestResponseAsync);
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
            return new ServiceRemotingClientWrapper(client, customHeadersProvider, BeforeSendRequestResponseAsync, AfterSendRequestResponseAsync);
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
            private readonly Func<IServiceRemotingRequestMessage, string, Task<object>> beforeSendRequestResponseAsync;
            private readonly Func<IServiceRemotingResponseMessage, string, object, Task> afterSendRequestResponseAsync;

            public ServiceRemotingClientWrapper(IServiceRemotingClient client, Func<CustomHeaders> customHeadersProvider, Func<IServiceRemotingRequestMessage, string, Task<object>> beforeSendRequestResponseAsync, Func<IServiceRemotingResponseMessage, string, object, Task> afterSendRequestResponseAsync)
            {
                Client = client;
                this.customHeadersProvider = customHeadersProvider;
                this.beforeSendRequestResponseAsync = beforeSendRequestResponseAsync;
                this.afterSendRequestResponseAsync = afterSendRequestResponseAsync;
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

            public async Task<IServiceRemotingResponseMessage> RequestResponseAsync(IServiceRemotingRequestMessage requestMessage)
            {
                var header = requestMessage.GetHeader();
                var customHeaders = customHeadersProvider.Invoke() ?? new CustomHeaders();

                if(!header.TryGetHeaderValue(CustomHeaders.CustomHeader, out var headerValue))
                    header.AddHeader(CustomHeaders.CustomHeader, customHeaders.Serialize());

                var methodName = $"{header.InterfaceId}.{header.MethodId}";

                object state = null;
                if (beforeSendRequestResponseAsync != null)
                    state = await beforeSendRequestResponseAsync.Invoke(requestMessage, methodName);
                var responseMessage = await Client.RequestResponseAsync(requestMessage);
                if (afterSendRequestResponseAsync != null)
                    await afterSendRequestResponseAsync.Invoke(responseMessage, methodName, state);

                return responseMessage;
            }

            public void SendOneWay(IServiceRemotingRequestMessage requestMessage)
            {
                var header = requestMessage.GetHeader();
                var customHeaders = customHeadersProvider.Invoke() ?? new CustomHeaders();
                header.AddHeader(CustomHeaders.CustomHeader, customHeaders.Serialize());

                Client.SendOneWay(requestMessage);
            }
        }
    }
}
