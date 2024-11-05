
namespace GrpcCrudBoilerplate.Contracts;

public class AuthDto
{
    public AuthResult Result { get; set; } = new AuthResult();
    public AuthUser? User { get; set; }
}

public class AuthResult
{
    public bool Success { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public string? ErrorMessage { get; set; }
}

public class AuthUser
{
    public required string UserName { get; set; }
}
