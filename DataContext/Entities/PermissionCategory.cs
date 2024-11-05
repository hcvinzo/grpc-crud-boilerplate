using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GrpcCrudBoilerplate.DataContext.Entities;

public class PermissionCategory
{
    [Key]
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    // Navigation property
    public List<Permission> Permissions { get; set; } = new List<Permission>();
}

