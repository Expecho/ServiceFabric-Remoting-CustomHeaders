using System;
using Microsoft.ServiceFabric.Services.Remoting.V2;

namespace ServiceFabric.Remoting.CustomHeaders.ReliableServices
{
    /// <summary>
    /// Information about the response
    /// </summary>
    public class ServiceResponseInfo
    {
        internal ServiceResponseInfo(
            IServiceRemotingResponseMessage responseMessage,
            string method,
            Uri service,
            object state,
            Exception exception)
        {
            ResponseMessage = responseMessage;
            Method = method;
            Service = service;
            State = state;
            Exception = exception;
        }

        /// <summary>
        /// The response message, null if an Exception has occured 
        /// </summary>
        public IServiceRemotingResponseMessage ResponseMessage { get; }

        /// <summary>
        /// Name of the method invoked
        /// </summary>
        public string Method { get; }

        /// <summary>
        /// Uri of the service invoked
        /// </summary>
        public Uri Service { get; }

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