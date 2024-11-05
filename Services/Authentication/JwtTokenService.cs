using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace GrpcCrudBoilerplate.Services.Authentication;

public interface IJwtTokenService
{
    (string accessToken, string refreshToken) GenerateTokens(string username, int userId);
    (string accessToken, string refreshToken) RefreshToken(string refreshToken, int userId);
    string? ValidateRefreshToken(string refreshToken);
}

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string accessToken, string refreshToken) GenerateTokens(string username, int userId)
    {
        var accessToken = GenerateAccessToken(username, userId);
        var refreshToken = GenerateRefreshToken();
        return (accessToken, refreshToken);
    }

    public (string accessToken, string refreshToken) RefreshToken(string refreshToken, int userId)
    {
        var username = ValidateRefreshToken(refreshToken);
        if (string.IsNullOrEmpty(username))
            throw new InvalidOperationException("Username is not available");

        var newAccessToken = GenerateAccessToken(username, userId);
        var newRefreshToken = GenerateRefreshToken();
        return (newAccessToken, newRefreshToken);
    }
    public string? ValidateRefreshToken(string refreshToken)
    {
        try
        {
            var principal = GetPrincipalFromExpiredToken(refreshToken);
            return principal.Identity?.Name;
        }
        catch
        {
            return null;
        }
    }

    private string GenerateAccessToken(string username, int userId)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim("uid", userId.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured")));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(15),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured"))),
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
        if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("Invalid token");

        return principal;
    }
}
