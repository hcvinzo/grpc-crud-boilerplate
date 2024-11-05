using System.Globalization;
using Grpc.Core;
using GrpcCrudBoilerplate.Services.Authentication;
using GrpcCrudBoilerplate.v1;
using Microsoft.AspNetCore.Authorization;
using Serilog;

namespace GrpcCrudBoilerplate.GrpcServices.v1;

public class AuthService : Auth.AuthBase
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IAuthenticationService _authenticationService;

    public AuthService(IJwtTokenService jwtTokenService, IAuthenticationService authenticationService)
    {
        _jwtTokenService = jwtTokenService;
        _authenticationService = authenticationService;
    }
    /// <summary>
    /// Authenticate and generate a JWT token
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    /// <exception cref="RpcException"></exception>
    public override async Task<AuthReply> Authenticate(AuthRequest request, ServerCallContext context)
    {
        Log.Information("Authetntication request for {Name} from {IpAddress}", request.Username, context.Peer);

        var authRes = await _authenticationService.AuthenticateUser(request.Username, request.Password);

        if (authRes.Result.Success)
        {
            return await Task.FromResult(new AuthReply { Token = authRes.Result.AccessToken, RefreshToken = authRes.Result.RefreshToken });
        }
        else
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, authRes.Result.ErrorMessage ?? "Not authenticated"));
        }
    }

    /// <summary>
    /// Refresh the access token using a refresh token
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<RefreshTokenResponse> RefreshToken(RefreshTokenRequest request, ServerCallContext context)
    {
        Log.Information("Refresh token request");

        // Refresh tokens
        var refreshResult = await _authenticationService.RefreshToken(request.RefreshToken);
        if (refreshResult.Result.Success)
        {
            // Use new tokens
            return await Task<RefreshTokenResponse>.FromResult(new RefreshTokenResponse
            {
                AccessToken = refreshResult.Result.AccessToken,
                RefreshToken = refreshResult.Result.RefreshToken
            });
        }
        else
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, refreshResult.Result.ErrorMessage ?? "Not authenticated"));
        }

    }
}