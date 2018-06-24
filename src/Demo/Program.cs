using System;
using System.Globalization;
using DemoService;
using ServiceFabric.Remoting.CustomHeaders;
using ServiceFabric.Remoting.CustomHeaders.ReliableServices;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Press any key to start .... ");
            Console.ReadLine();

            //SimpleProxy();
            NonReusedProxy();
            //ReusedProxy();
        }

        static void NonReusedProxy()
        {
            // Create a new instance of CustomHeaders that is passed on each call.
            var customHeaders = new CustomHeaders
            {
                {"Header1", DateTime.Now.ToString(CultureInfo.InvariantCulture)},
                {"Header2", Guid.NewGuid().ToString()}
            };

            while (true)
            {
                var serviceUri = new Uri("fabric:/ServiceFabric.Remoting.CustomHeaders.DemoApplication/DemoService");
                var proxy = ExtendedServiceProxy.Create<IDemoService>(serviceUri, customHeaders);
                var actorMessage = proxy.SayHelloToActor().GetAwaiter().GetResult();

                Console.WriteLine($"Actor said '{actorMessage}'");
                Console.WriteLine("Press any key to restart (q to quit).... ");
                var key = Console.ReadLine();
                if (key.ToLowerInvariant() == "q")
                    break;
            }
        }


        static void ReusedProxy()
        {
            // Create a factory to provide a new CustomHeaders instance on each call
            var customHeaderProvider = new Func<CustomHeaders>(() => new CustomHeaders
            {
                {"Header1", DateTime.Now.ToString(CultureInfo.InvariantCulture)},
                {"Header2", Guid.NewGuid().ToString()}
            });
            var serviceUri = new Uri("fabric:/ServiceFabric.Remoting.CustomHeaders.DemoApplication/DemoService");
            var proxy = ExtendedServiceProxy.Create<IDemoService>(serviceUri, customHeaderProvider);

            while (true)
            {
                // the proxy is reused, but the header data is changed as the provider
                // is invoked during each SayHelloToActor call.
                var actorMessage = proxy.SayHelloToActor().GetAwaiter().GetResult();

                Console.WriteLine($"Actor said '{actorMessage}'");
                Console.WriteLine("Press any key to restart (q to quit).... ");
                var key = Console.ReadLine();
                if (key.ToLowerInvariant() == "q")
                    break;
            }
        }

        static void SimpleProxy()
        {
            var serviceUri = new Uri("fabric:/ServiceFabric.Remoting.CustomHeaders.DemoApplication/DemoService");

            // If none of the features is used the ExtendedServiceProxy can still be used as all behavior is opt-in
            var proxy = ExtendedServiceProxy.Create<IDemoService>(serviceUri);

            while (true)
            {
                var actorMessage = proxy.SayHelloToActor().GetAwaiter().GetResult();

                Console.WriteLine($"Actor said '{actorMessage}'");
                Console.WriteLine("Press any key to restart (q to quit).... ");
                var key = Console.ReadLine();
                if (key.ToLowerInvariant() == "q")
                    break;
            }
        }
    }
}
