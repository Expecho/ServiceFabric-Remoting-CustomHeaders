using System;
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
            Uri service,
            object state,
            Exception exception)
        {
            ResponseMessage = responseMessage;
            ActorId = actorId;
            Method = method;
            ActorService = service;
            State = state;
            Exception = exception;
        }

        /// <summary>
        /// The response message, null if an Exception has occured
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
        /// Uri of the actor invoked
        /// </summary>
        public Uri ActorService { get; }

        /// <summary>
        /// Optional state
        /// </summary>
        public object State { get; }

        /// <summary>
        /// The exception, if any
        /// </summary>
        public Exception Exception { get; }
    }
}