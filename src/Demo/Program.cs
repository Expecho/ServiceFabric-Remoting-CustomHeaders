using System;
using System.Globalization;
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
            Console.WriteLine("Press any key to start .... ");
            Console.ReadLine();

            //NonReusedProxy();
            ReusedProxy();
        }

        static void NonReusedProxy()
        {
            // Create a new instance of CustomHeaders that is passed on each call.
            var customHeaders = new CustomHeaders
            {
                {"Header1", DateTime.Now},
                {"Header2", Guid.NewGuid()}
            };

            while (true)
            {
                var serviceUri = new Uri("fabric:/ServiceFabric.Remoting.CustomHeaders.DemoApplication/DemoService");
                
                var proxyFactory = new ServiceProxyFactory(handler =>
                    new ExtendedServiceRemotingClientFactory(
                        new FabricTransportServiceRemotingClientFactory(remotingCallbackMessageHandler: handler), customHeaders));
                var proxy = proxyFactory.CreateServiceProxy<IDemoService>(serviceUri);
                var actorResponse = proxy.SayHelloToActor().GetAwaiter().GetResult();

                Console.WriteLine($"Actor said '{actorResponse}'");
                Console.WriteLine("Press any key to restart (q to quit).... ");
                var key = Console.ReadLine();
                if (key.ToLowerInvariant() == "q")
                    break;
            }
        }


        static void ReusedProxy()
        {
            // Create a factory to provide a new CustomHeaders instance on each call
            var customHeadersProvider = new Func<CustomHeaders>(() => new CustomHeaders
            {
                {"Header1", DateTime.Now},
                {"Header2", Guid.NewGuid()}
            });

            var serviceUri = new Uri("fabric:/ServiceFabric.Remoting.CustomHeaders.DemoApplication/DemoService");
            var proxyFactory = new ServiceProxyFactory(handler =>
                new ExtendedServiceRemotingClientFactory(
                    new FabricTransportServiceRemotingClientFactory(remotingCallbackMessageHandler: handler), customHeadersProvider));
            var proxy = proxyFactory.CreateServiceProxy<IDemoService>(serviceUri);

            while (true)
            {
                // the proxy is reused, but the header data is changed as the provider
                // is invoked during each SayHelloToActor call.
                var actorResponse = proxy.SayHelloToActor().GetAwaiter().GetResult();

                Console.WriteLine($"Actor said '{actorResponse}'");
                Console.WriteLine("Press any key to restart (q to quit).... ");
                var key = Console.ReadLine();
                if (key.ToLowerInvariant() == "q")
                    break;
            }
        }
    }
}
