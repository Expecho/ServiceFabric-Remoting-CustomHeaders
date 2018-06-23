using System.Collections.Generic;
using System.Linq;

namespace ServiceFabric.Remoting.CustomHeaders
{
    public class CustomHeaders : Dictionary<string, string>
    {
        public const string MethodHeader = "x-fabric-method";

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