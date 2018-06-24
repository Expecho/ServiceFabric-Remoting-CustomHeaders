# ServiceFabric.Remoting.CustomHeaders

This package allows injecting custom headers into remoting messages (Actors and Reliable Services, V2 remoting only) at runtime. The headers are available client side to read. 
It also provides BeforeHandleRequestResponseAsync and AfterHandleRequestResponseAsync to act on remoting events.'

Common used classes:

- [ExtendedActorProxy](https://github.com/Expecho/ServiceFabric-Remoting-CustomHeaders/blob/master/src/ServiceFabric.Remoting.CustomHeaders/Actors/ExtendedActorProxy.cs)
- [ExtendedServiceProxy](https://github.com/Expecho/ServiceFabric-Remoting-CustomHeaders/blob/master/src/ServiceFabric.Remoting.CustomHeaders/ReliableServices/ExtendedServiceProxy.cs)
- [CustomHeaders](https://github.com/Expecho/ServiceFabric-Remoting-CustomHeaders/blob/master/src/ServiceFabric.Remoting.CustomHeaders/CustomHeaders.cs)
- [RemotingContext](https://github.com/Expecho/ServiceFabric-Remoting-CustomHeaders/blob/master/src/ServiceFabric.Remoting.CustomHeaders/RemotingContext.cs)

## Examples

This repository includes a Service Fabric application for demonstration purposes. A [Console Application](https://github.com/Expecho/ServiceFabric-Remoting-CustomHeaders/blob/master/src/Demo/Program.cs) is used to access the application and shows the usage of the package.

## Usage scenarios

Custom headers can be used to pass data between the sender and the receiver like tracing information or security context data. Using the BeforeHandleRequestResponseAsync and AfterHandleRequestResponseAsync actions additional logging can be applied monitor the flow between remoting calls.

## How to use 

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

