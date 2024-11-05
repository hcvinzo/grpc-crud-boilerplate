using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GrpcCrudBoilerplate.DataContext.Entities;

public class Permission
{
    [Key]
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required int CategoryId { get; set; }

    // Navigation property
    public PermissionCategory? Category { get; set; }
    public List<Role> Roles { get; set; } = new List<Role>();
}

