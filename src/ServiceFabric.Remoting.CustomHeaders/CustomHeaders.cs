using System.Collections.Generic;
using System.Linq;

namespace ServiceFabric.Remoting.CustomHeaders
{
    /// <inheritdoc />
    /// <summary>
    /// Custom headers passed on using remoting calls
    /// </summary>
    public class CustomHeaders : Dictionary<string, string>
    {
        /// <summary>
        /// Reserved. Key of the header that contains the method invoked
        /// </summary>
        public const string MethodHeader = "x-fabric-method";

        /// <summary>
        /// Create a new instance based on the current <see cref="RemotingContext"/>
        /// </summary>
        /// <returns>An instance of <see cref="CustomHeaders"/> with headers populated using the current <see cref="RemotingContext"/></returns>
        public static CustomHeaders FromRemotingContext()
        {
            var customHeader = new CustomHeaders();

            foreach (var key in RemotingContext.Keys.Where(k => k != MethodHeader))
            {
                customHeader.Add(key, RemotingContext.GetData(key).ToString());
            }

            return customHeader;
        }
    }
}