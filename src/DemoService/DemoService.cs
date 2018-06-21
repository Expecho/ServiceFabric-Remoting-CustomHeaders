using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using ServiceFabric.Remoting.CustomHeaders;

namespace DemoService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class DemoService : StatelessService, IDemoService
    {
        public DemoService(StatelessServiceContext context)
            : base(context)
        { }

        public Task<string> SayHelloToActor()
        {
            var remotingContext =
                string.Join(", ", RemotingContext.Keys.Select(k => $"{k}: {RemotingContext.GetData(k)}"));

            ServiceEventSource.Current.ServiceMessage(Context, $"SayHelloToActor got context: {remotingContext}");

            return Task.FromResult(remotingContext);
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            yield return new ServiceInstanceListener(context =>
                new FabricTransportServiceRemotingListener(context,
                    new CustomHeadersServiceRemotingMessageDispatcher(context, this)));
        }
    }
}
