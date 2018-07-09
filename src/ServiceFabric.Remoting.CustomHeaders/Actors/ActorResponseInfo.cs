using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Services.Remoting.V2;

namespace ServiceFabric.Remoting.CustomHeaders.Actors
{
    /// <summary>
    /// Information about the response
    /// </summary>
    public class ActorResponseInfo
    {
        internal ActorResponseInfo(
            IServiceRemotingResponseMessage responseMessage,
            ActorId actorId,
            string method,
            object state)
        {
            ResponseMessage = responseMessage;
            ActorId = actorId;
            Method = method;
            State = state;
        }

        /// <summary>
        /// The response message 
        /// </summary>
        public IServiceRemotingResponseMessage ResponseMessage { get; }
        
        /// <summary>
        /// The id of the actor involved
        /// </summary>
        public ActorId ActorId { get; }

        /// <summary>
        /// Name of the method invoked
        /// </summary>
        public string Method { get; }

        /// <summary>
        /// Optional state
        /// </summary>
        public object State { get; }
    }
}