using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Services.Remoting.V2;

namespace ServiceFabric.Remoting.CustomHeaders.Actors
{
    /// <summary>
    /// Information about the request
    /// </summary>
    public class ActorRequestInfo
    {
        internal ActorRequestInfo(
            IServiceRemotingRequestMessage requestMessage,
            ActorId actorId,
            string method)
        {
            RequestMessage = requestMessage;
            ActorId = actorId;
            Method = method;
        }

        /// <summary>
        /// The request message 
        /// </summary>
        public IServiceRemotingRequestMessage RequestMessage { get; }

        /// <summary>
        /// The id of the actor involved
        /// </summary>
        public ActorId ActorId { get; }

        /// <summary>
        /// Name of the method invoked
        /// </summary>
        public string Method { get; }
    }
}