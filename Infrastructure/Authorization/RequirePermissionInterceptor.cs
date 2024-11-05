using System.Reflection;
using Grpc.Core;
using Grpc.Core.Interceptors;
using GrpcCrudBoilerplate.DataContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using GrpcCrudBoilerplate.Services.Authorization;

namespace GrpcCrudBoilerplate.Infrastructure.Authorization;

public class RequirePermissionInterceptor : Interceptor
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IUserContext _userContext;
    public RequirePermissionInterceptor(IAuthorizationService authorizationService, IUserContext userContext)
    {
        _authorizationService = authorizationService;
        _userContext = userContext;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var attributeRequirePermission = context.GetHttpContext().GetEndpoint()?.Metadata
            .GetMetadata<RequirePermissionAttribute>();

        if (attributeRequirePermission != null)
        {
            if (_userContext.UserId == 0)
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, "User not authenticated"));
            }

            if (!await _authorizationService.HasPermission(
                _userContext.UserId,
                attributeRequirePermission.Permissions,
                attributeRequirePermission.RequireAll))
            {
                throw new RpcException(new Status(StatusCode.PermissionDenied, "Permission denied"));
            }
        }

        return await continuation(request, context);
    }
}