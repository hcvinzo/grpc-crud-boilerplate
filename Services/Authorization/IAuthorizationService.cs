namespace GrpcCrudBoilerplate.Services.Authorization
{
    public interface IAuthorizationService
    {
        Task<bool> HasPermission(int userId, string permission);
        Task<bool> HasPermission(int userId, string[] permissions, bool requireAll = true);
        Task EnsurePermission(int userId, string permission);
        Task EnsurePermission(int userId, string[] permissions, bool requireAll = true);
        Task<HashSet<string>> GetUserPermissions(int userId);
    }
}