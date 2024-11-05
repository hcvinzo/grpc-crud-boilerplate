using Grpc.Core;
using GrpcCrudBoilerplate.v1;
using Microsoft.AspNetCore.Authorization;
using Serilog;

namespace GrpcCrudBoilerplate.GrpcServices.v1;

[Authorize]
public class GreeterService : Greeter.GreeterBase
{
    public GreeterService()
    {
    }


    /// <summary>
    /// Say hello to the given name
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        Log.Information("Saying hello to {Name}", request.Name);

        return Task.FromResult(new HelloReply
        {
            Message = "Hello " + request.Name
        });
    }

    /// <summary>
    /// Say hello to the given name in Turkish
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override Task<MerhabaReply> SayMerhaba(HelloRequest request, ServerCallContext context)
    {
        Log.Information("Saying hello in Turkish to {Name}", request.Name);

        return Task.FromResult(new MerhabaReply
        {
            Message = $"Merhaba {request.Name}"
        });
    }
}
