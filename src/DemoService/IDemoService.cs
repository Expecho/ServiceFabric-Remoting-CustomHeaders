using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;

namespace DemoService
{
    public interface IDemoService : IService
    {
        Task<string> SayHelloToActor();

        Task ThrowException();
    }
}