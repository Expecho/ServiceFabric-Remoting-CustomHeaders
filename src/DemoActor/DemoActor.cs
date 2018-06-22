using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using DemoActor.Interfaces;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors.Remoting.V2.FabricTransport.Client;
using ServiceFabric.Remoting.CustomHeaders;

namespace DemoActor
{
    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    internal class DemoActor : Actor, IDemoActor
    {
        /// <summary>
        /// Initializes a new instance of DemoActor
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public DemoActor(ActorService actorService, ActorId actorId) 
            : base(actorService, actorId)
        {
        }

        Task<string> IDemoActor.GetGreetingResponseAsync(CancellationToken cancellationToken)
        {
            var remotingContext =
                string.Join(", ", RemotingContext.Keys.Select(k => $"{k}: {RemotingContext.GetData(k)}"));

            ActorEventSource.Current.ActorMessage(this, $"GetGreetingResponseAsync got context: {remotingContext}");

            return Task.FromResult($"Hello From Actor (with context '{remotingContext}')");
        }
    }
}
