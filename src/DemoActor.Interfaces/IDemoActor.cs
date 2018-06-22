using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;

namespace DemoActor.Interfaces
{
    /// <summary>
    /// This interface defines the methods exposed by an actor.
    /// Clients use this interface to interact with the actor that implements it.
    /// </summary>
    public interface IDemoActor : IActor
    {
        Task<string> GetGreetingResponseAsync(CancellationToken cancellationToken);
    }
}
