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

            var customHeaders = new CustomHeaders
            {
                {"Header1", DateTime.Now.ToString(CultureInfo.InvariantCulture)},
                {"Header2", Guid.NewGuid().ToString()}
            };

            var serviceUri = new Uri("fabric:/ServiceFabric.Remoting.CustomHeaders.DemoApplication/DemoService");
            var proxy = CustomHeaderServiceProxy.Create<IDemoService>(serviceUri, customHeaders);

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
