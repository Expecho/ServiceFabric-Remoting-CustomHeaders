using System;
using System.Diagnostics;
using System.Threading.Tasks;
using DemoService;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;
using ServiceFabric.Remoting.CustomHeaders;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            //FixedHeaderValues();
            DynamicHeaderValues();
        }

        static void FixedHeaderValues()
        {
            Console.WriteLine("Press any key to start .... ");
            Console.ReadLine();

            // Create a new instance of CustomHeaders that is passed on each call.
            var customHeaders = new CustomHeaders
            {
                {"Header1", DateTime.Now},
                {"Header2", Guid.NewGuid()}
            };

            var serviceUri = new Uri("fabric:/ServiceFabric.Remoting.CustomHeaders.DemoApplication/DemoService");

            var proxyFactory = new ServiceProxyFactory(handler =>
                new ExtendedServiceRemotingClientFactory(
                    new FabricTransportServiceRemotingClientFactory(remotingCallbackMessageHandler: handler), customHeaders));
            var proxy = proxyFactory.CreateServiceProxy<IDemoService>(serviceUri);
            var actorResponse = proxy.SayHelloToActor().GetAwaiter().GetResult();

            Console.WriteLine($"Actor said '{actorResponse}'");
            Console.WriteLine("Press any key to stop. ");
            Console.ReadLine();
        }

        static void DynamicHeaderValues()
        {
            Console.WriteLine("Press any key to start .... ");
            var key = Console.ReadLine();

            // Create a factory to provide a new CustomHeaders instance on each call
            var customHeadersProvider = new Func<CustomHeaders>(() => new CustomHeaders
            {
                {"Header1", DateTime.Now},
                {"Header2", Guid.NewGuid()},
                {"PressedKey", key}
            });

            var serviceUri = new Uri("fabric:/ServiceFabric.Remoting.CustomHeaders.DemoApplication/DemoService");
            var proxyFactory = new ServiceProxyFactory(handler =>
                new ExtendedServiceRemotingClientFactory(
                    new FabricTransportServiceRemotingClientFactory(remotingCallbackMessageHandler: handler), customHeadersProvider)
                {
                    // Optional, log the call before being handled
                    BeforeSendRequestResponseAsync = requestInfo =>
                    {
                        var sw = new Stopwatch();
                        sw.Start();
                        Console.WriteLine($"BeforeSendRequestAsync {requestInfo.Service} {requestInfo.Method}");
                        return Task.FromResult<object>(sw);
                    },
                    // Optional, log the call after being handled
                    AfterSendRequestResponseAsync = responseInfo =>
                    {
                        var sw = (Stopwatch)responseInfo.State;
                        Console.WriteLine($"AfterSendRequestAsync {responseInfo.Service} {responseInfo.Method} took {sw.ElapsedMilliseconds}ms");
                        return Task.CompletedTask;
                    }
                });
            var proxy = proxyFactory.CreateServiceProxy<IDemoService>(serviceUri);

            while (true)
            {
                // the proxy is reused, but the header data is changed as the provider
                // is invoked during each SayHelloToActor call.
                var actorResponse = proxy.SayHelloToActor().GetAwaiter().GetResult();

                Console.WriteLine($"Actor said '{actorResponse}'");
                Console.WriteLine("Press any key to restart (q to quit).... ");
                key = Console.ReadLine();
                if (key.ToLowerInvariant() == "q")
                    break;
            }
        }
    }
}
