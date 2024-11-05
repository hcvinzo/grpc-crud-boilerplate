using Microsoft.AspNetCore.Http;

public interface IUserContext
{
    public string Username { get; }
    public int UserId { get; }
}

public class UserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public string Username { get; private set; } = "";
    public int UserId { get; private set; }
    public HashSet<string> Permissions { get; private set; } = new();

    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        Initialize();
    }

    private void Initialize()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            Username = user.Identity.Name ?? "";
            var userIdClaim = user.FindFirst("uid");
            UserId = int.Parse(userIdClaim?.Value ?? "");
        }
    }
}
