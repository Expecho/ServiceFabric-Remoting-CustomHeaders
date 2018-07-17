using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.ServiceFabric.Services.Remoting.V2;
using ServiceFabric.Remoting.CustomHeaders.Util;

namespace ServiceFabric.Remoting.CustomHeaders
{
    /// <summary>
    /// Provides a way to set contextual data that flows with the call and 
    /// async context of a test or invocation.
    /// </summary>
    public static class RemotingContext
    {
        private static readonly ConcurrentDictionary<string, AsyncLocal<object>> State = new ConcurrentDictionary<string, AsyncLocal<object>>();

        /// <summary>
        /// Stores a given object and associates it with the specified name.
        /// </summary>
        /// <param name="name">The name with which to associate the new item in the call context.</param>
        /// <param name="data">The object to store in the call context.</param>
        public static void SetData(string name, object data) =>
            State.GetOrAdd(name, _ => new AsyncLocal<object>()).Value = data;

        /// <summary>
        /// Gets an object with the specified name from the <see cref="RemotingContext"/>.
        /// </summary>
        /// <param name="name">The name of the item in the call context.</param>
        /// <returns>The object in the call context associated with the specified name, or <see langword="null"/> if not found.</returns>
        public static object GetData(string name) =>
            State.TryGetValue(name, out AsyncLocal<object> data) ? data.Value : null;

        /// <summary>
        /// Gets the list of keys stored
        /// </summary>
        public static IEnumerable<string> Keys => State.Keys;

        internal static void FromRemotingMessageHeader(IServiceRemotingRequestMessageHeader header)
        {
            foreach (var customHeader in header.GetCustomHeaders().Where(h => h.Key != CustomHeaders.ReservedHeaderServiceUri))
            {
                if (customHeader.Key != null)
                {
                    SetData(customHeader.Key, customHeader.Value);
                }
            }
        }
    }
}
