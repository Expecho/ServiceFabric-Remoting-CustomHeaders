using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors.Runtime;
using ServiceFabric.Remoting.CustomHeaders.Actors;

namespace DemoActor
{
    internal static class Program
    {
        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        private static void Main()
        {
            try
            {
                // This line registers an Actor Service to host your actor class with the Service Fabric runtime.
                // The contents of your ServiceManifest.xml and ApplicationManifest.xml files
                // are automatically populated when you build this project.
                // For more information, see https://aka.ms/servicefabricactorsplatform

                ActorRuntime.RegisterActorAsync<DemoActor> (
                   (context, actorType) =>
                   {
                       var service = new ExtendedActorService(context, actorType)
                       {
                           // Optional, allows call interception. Executed before the response is handled
                           BeforeHandleRequestResponseAsync = (message, method, id) =>
                           {
                               ActorEventSource.Current.Message($"BeforeHandleRequestResponseAsync {method} for actor {id.ToString()}");
                               return Task.CompletedTask;
                           },
                           // Optional, allows call interception. Executed after the response is handled
                           AfterHandleRequestResponseAsync = (message, method, id) =>
                           {
                               ActorEventSource.Current.Message($"AfterHandleRequestResponseAsync {method} for actor {id.ToString()}");
                               return Task.CompletedTask;
                           }
                       };
                       return service;
                   }).GetAwaiter().GetResult();

                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                ActorEventSource.Current.ActorHostInitializationFailed(e.ToString());
                throw;
            }
        }
    }
}
