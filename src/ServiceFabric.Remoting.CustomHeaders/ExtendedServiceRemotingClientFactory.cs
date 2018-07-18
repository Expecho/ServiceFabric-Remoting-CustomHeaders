using System;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2;
using Microsoft.ServiceFabric.Services.Remoting.V2.Client;
using ServiceFabric.Remoting.CustomHeaders.ReliableServices;

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
        /// </summary>
        /// <returns>object: state</returns>
        public Func<ServiceRequestInfo, Task<object>> BeforeSendRequestResponseAsync { get; set; }

        /// <summary>
        /// Optional hook to provide code executed after the message is send to the client
        /// </summary>
        public Func<ServiceResponseInfo, Task> AfterSendRequestResponseAsync { get; set; }

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
            private readonly Func<ServiceRequestInfo, Task<object>> beforeSendRequestResponseAsync;
            private readonly Func<ServiceResponseInfo, Task> afterSendRequestResponseAsync;

            public ServiceRemotingClientWrapper(IServiceRemotingClient client, Func<CustomHeaders> customHeadersProvider, Func<ServiceRequestInfo, Task<object>> beforeSendRequestResponseAsync, Func<ServiceResponseInfo, Task> afterSendRequestResponseAsync)
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
                var customHeaders = CreateCustomHeaders();
                customHeaders.Add(CustomHeaders.ReservedHeaderServiceUri, ResolvedServicePartition.ServiceName);

                if (!header.TryGetHeaderValue(CustomHeaders.CustomHeader, out var headerValue))
                    header.AddHeader(CustomHeaders.CustomHeader, customHeaders.Serialize());

                object state = null;
                if (beforeSendRequestResponseAsync != null)
                    state = await beforeSendRequestResponseAsync.Invoke(new ServiceRequestInfo(requestMessage, header.MethodName, ResolvedServicePartition.ServiceName));
                IServiceRemotingResponseMessage responseMessage = null;

                Exception exception = null;

                try
                {
                    responseMessage = await Client.RequestResponseAsync(requestMessage);
                }
                catch (Exception ex)
                {
                    exception = ex;
                    throw;
                }
                finally
                {
                    if (afterSendRequestResponseAsync != null)
                        await afterSendRequestResponseAsync.Invoke(new ServiceResponseInfo(responseMessage, header.MethodName, ResolvedServicePartition.ServiceName, state, exception));
                }
                
                return responseMessage;
            }

            public void SendOneWay(IServiceRemotingRequestMessage requestMessage)
            {
                var header = requestMessage.GetHeader();
                var customHeaders = customHeadersProvider.Invoke() ?? new CustomHeaders();
                header.AddHeader(CustomHeaders.CustomHeader, customHeaders.Serialize());

                Client.SendOneWay(requestMessage);
            }

            private CustomHeaders CreateCustomHeaders()
            {
                try
                {
                    return customHeadersProvider.Invoke();
                }
                catch (Exception)
                {
                    return new CustomHeaders();
                }
            }
        }
    }
}
