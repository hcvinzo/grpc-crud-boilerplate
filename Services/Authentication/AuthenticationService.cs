using System.Text.Json.Serialization;
using AutoMapper;
using GrpcCrudBoilerplate.Contracts;
using GrpcCrudBoilerplate.DataContext;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace GrpcCrudBoilerplate.Services.Authentication;

public interface IAuthenticationService
{
    Task<AuthDto> AuthenticateUser(string userName, string pw);
    Task<AuthDto> RefreshToken(string refreshToken);
}

public class AuthenticationService : IAuthenticationService
{
    private readonly IMapper _mapper;
    private readonly AppDbContext _dbContext;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthenticationService(IMapper mapper, AppDbContext dbContext, IJwtTokenService jwtTokenService)
    {
        _mapper = mapper;
        _dbContext = dbContext;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthDto> AuthenticateUser(string userName, string pw)
    {
        if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(pw))
        {
            throw new ArgumentException("Missing argument(s)");
        }

        var authRes = new AuthDto();
        authRes.Result.Success = false;
        authRes.Result.ErrorMessage = "Invalid credentials";

        // check user exists
        var user = await _dbContext.Users.FirstOrDefaultAsync(_ => _.Username.Equals(userName));
        if (user != null)
        {
            // check password
            bool checkPW = Argon2PasswordService.VerifyPassword(pw, user.PasswordHash);
            if (checkPW)
            {
                if (user.IsActive)
                {
                    authRes.Result.Success = true;
                    authRes.Result.ErrorMessage = "";
                    authRes.User = new AuthUser
                    {
                        UserName = user.Username
                    };

                    (string accessToken, string refreshToken) = _jwtTokenService.GenerateTokens(user.Username, user.Id);
                    authRes.Result.AccessToken = accessToken;
                    authRes.Result.RefreshToken = refreshToken;
                }
                else
                {
                    authRes.Result.ErrorMessage = "Account is disabled";
                }
            }
        }

        return authRes;
    }

    public async Task<AuthDto> RefreshToken(string refreshToken)
    {
        var authRes = new AuthDto();
        authRes.Result.Success = false;
        authRes.Result.ErrorMessage = "Invalid refresh token";

        if (string.IsNullOrEmpty(refreshToken))
        {
            throw new ArgumentException("Refresh token is required");
        }

        // Validate the refresh token and get the username
        var username = _jwtTokenService.ValidateRefreshToken(refreshToken);
        if (string.IsNullOrEmpty(username))
        {
            return authRes;
        }

        // Get user from database
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

        if (user == null)
        {
            authRes.Result.ErrorMessage = "User not found or inactive";
            return authRes;
        }

        // Generate new tokens
        var (newAccessToken, newRefreshToken) = _jwtTokenService.GenerateTokens(username, user.Id);

        authRes.Result.Success = true;
        authRes.Result.ErrorMessage = "";
        authRes.Result.AccessToken = newAccessToken;
        authRes.Result.RefreshToken = newRefreshToken;
        authRes.User = new AuthUser
        {
            UserName = username
        };

        // Update user's last login time
        user.LastLoginAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return authRes;
    }
}