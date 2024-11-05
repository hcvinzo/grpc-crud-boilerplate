namespace GrpcCrudBoilerplate.Infrastructure.Authorization;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false)]
public class RequirePermissionAttribute : Attribute
{
    public string[] Permissions { get; }
    public bool RequireAll { get; }

    public RequirePermissionAttribute(string permission)
    {
        Permissions = new[] { permission };
        RequireAll = true;
    }

    public RequirePermissionAttribute(string[] permissions, bool requireAll = true)
    {
        Permissions = permissions;
        RequireAll = requireAll;
    }
}