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
            object state)
        {
            ResponseMessage = responseMessage;
            Method = method;
            State = state;
        }

        /// <summary>
        /// The response message 
        /// </summary>
        public IServiceRemotingResponseMessage ResponseMessage { get; }

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