using System.Collections.Generic;
using System.Diagnostics;
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
            // Read the data from the custom header
            var remotingContext =
                string.Join(", ", RemotingContext.Keys.Select(k => $"{k}: {RemotingContext.GetData(k)}"));

            ServiceEventSource.Current.ServiceMessage(Context, $"SayHelloToActor got context: {remotingContext}");

            // Call the actor using the same headers as received by this method
            var proxy = ExtendedActorProxy.Create<IDemoActor>(new ActorId(1), CustomHeaders.FromRemotingContext);
            var response = await proxy.GetGreetingResponseAsync(CancellationToken.None);

            return $"DemoService passed context '{remotingContext}' to actor and got as response: {response}";
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
                        // Optional, log the call before being handled
                        BeforeHandleRequestResponseAsync = (message, method) =>
                        {
                            var sw = new Stopwatch();
                            sw.Start();
                            ServiceEventSource.Current.ServiceMessage(Context, $"BeforeHandleRequestResponseAsync {method}");
                            return Task.FromResult<object>(sw);
                        },
                        // Optional, log the call after being handled
                        AfterHandleRequestResponseAsync = (message, method, state) =>
                        {
                            var sw = (Stopwatch) state;
                            ServiceEventSource.Current.ServiceMessage(Context, $"AfterHandleRequestResponseAsync {method} took {sw.ElapsedMilliseconds}ms");
                            return Task.CompletedTask;
                        }
                    }));
        }
    }
}
