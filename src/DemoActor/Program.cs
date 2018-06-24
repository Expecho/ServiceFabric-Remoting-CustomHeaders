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
                           // Optional, log the call before the message is handled
                           BeforeHandleRequestResponseAsync = (message, id, method) =>
                           {
                               ActorEventSource.Current.Message($"BeforeHandleRequestResponseAsync {method} for actor {id.ToString()}");
                               return Task.FromResult<object>(null);
                           },
                           // Optional, log the call after the message is handled
                           AfterHandleRequestResponseAsync = (message, id, method, state) =>
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
