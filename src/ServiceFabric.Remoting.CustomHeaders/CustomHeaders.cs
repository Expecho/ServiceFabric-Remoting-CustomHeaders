using System.Collections.Generic;

namespace ServiceFabric.Remoting.CustomHeaders
{
    public class CustomHeaders : Dictionary<string, string>
    {
        public static CustomHeaders FromRemotingContext()
        {
            var customHeader = new CustomHeaders();

            foreach (var key in RemotingContext.Keys)
            {
                customHeader.Add(key, RemotingContext.GetData(key).ToString());
            }

            return customHeader;
        }
    }
}