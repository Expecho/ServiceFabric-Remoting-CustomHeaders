using System.Text;
using Microsoft.ServiceFabric.Services.Remoting.V2;

namespace ServiceFabric.Remoting.CustomHeaders.Util
{
    internal static class ServiceRemotingRequestMessageHeaderExtensionMethods
    {
        internal static string GetMethodName(this IServiceRemotingRequestMessageHeader header)
        {
            var methodName = string.Empty;
            if (header.TryGetHeaderValue(CustomHeaders.MethodHeader, out byte[] headerValue))
            {
                methodName = Encoding.ASCII.GetString(headerValue);
            }

            return methodName;
        }

        internal static CustomHeaders GetCustomHeaders(this IServiceRemotingRequestMessageHeader header)
        {
            var customHeaders = new CustomHeaders();
            if (header.TryGetHeaderValue(CustomHeaders.CustomHeader, out byte[] headerValue))
            {
                customHeaders = CustomHeaders.Deserialize(headerValue);
            }

            return customHeaders;
        }
    }
}
