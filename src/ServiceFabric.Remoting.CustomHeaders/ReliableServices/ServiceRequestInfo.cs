using Microsoft.ServiceFabric.Services.Remoting.V2;

namespace ServiceFabric.Remoting.CustomHeaders.ReliableServices
{
    /// <summary>
    /// Information about the request
    /// </summary>
    public class ServiceRequestInfo
    {
        internal ServiceRequestInfo(
            IServiceRemotingRequestMessage requestMessage,
            string method)
        {
            RequestMessage = requestMessage;
            Method = method;
        }

        /// <summary>
        /// The request message 
        /// </summary>
        public IServiceRemotingRequestMessage RequestMessage { get; }

        /// <summary>
        /// Name of the method invoked
        /// </summary>
        public string Method { get; }
    }
}