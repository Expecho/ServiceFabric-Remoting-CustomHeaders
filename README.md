# ServiceFabric-Remoting-CustomHeaders

This NuGet package allows injecting custom headers into remoting messages at runtime. The headers are available client side using the `RemotingContext` context class ([source](https://github.com/Expecho/ServiceFabric-Remoting-CustomHeaders/blob/master/src/ServiceFabric.Remoting.CustomHeaders/RemotingContext.cs))

## Example

### Sender

            var customHeaders = new CustomHeaders
            {
                {"Header1", DateTime.Now.ToString(CultureInfo.InvariantCulture)},
                {"Header2", Guid.NewGuid().ToString()}
            };

            var serviceUri = new Uri("fabric:/ServiceFabric.Remoting.CustomHeaders.DemoApplication/DemoService");
            var proxy = ExtendedServiceProxy.Create<IDemoService>(serviceUri, customHeaders);
            var actorMessage = proxy.SayHello().GetAwaiter().GetResult();
            
### Receiver


        public async Task<string> SayHello()
        {
            var remotingContext =
                string.Join(", ", RemotingContext.Keys.Select(k => $"{k}: {RemotingContext.GetData(k)}"));

            ServiceEventSource.Current.ServiceMessage(Context, $"SayHelloToActor got context: {remotingContext}");
            return Task.FromResult($"Got the following message headers: {remotingContext}")
        }

