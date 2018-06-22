using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DemoActor.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using ServiceFabric.Remoting.CustomHeaders;
using ServiceFabric.Remoting.CustomHeaders.Actors;
using ServiceFabric.Remoting.CustomHeaders.ReliableServices;

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

        public async Task<string> SayHelloToActor()
        {
            var remotingContext =
                string.Join(", ", RemotingContext.Keys.Select(k => $"{k}: {RemotingContext.GetData(k)}"));

            ServiceEventSource.Current.ServiceMessage(Context, $"SayHelloToActor got context: {remotingContext}");
            var proxy = ExtendedActorProxy.Create<IDemoActor>(new ActorId(1), CustomHeaders.FromRemotingContext);
            var response = await proxy.GetGreetingResponseAsync(CancellationToken.None);

            return $"DemoService passed context '{remotingContext}' to actor and got as response: '{response}'";
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            yield return new ServiceInstanceListener(context =>
                new FabricTransportServiceRemotingListener(context,
                    new ExtendedServiceRemotingMessageDispatcher(context, this)
                    {
                        BeforeHandleRequestResponseAsync = message =>
                        {
                            ServiceEventSource.Current.ServiceMessage(Context, "BeforeHandleRequestResponseAsync");
                            return Task.CompletedTask;
                        },
                        AfterHandleRequestResponseAsync = message =>
                        {
                            ServiceEventSource.Current.ServiceMessage(Context, "AfterHandleRequestResponseAsync");
                            return Task.CompletedTask;
                        }
                    }));
        }
    }
}
