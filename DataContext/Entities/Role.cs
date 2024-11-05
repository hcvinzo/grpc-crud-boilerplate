using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GrpcCrudBoilerplate.DataContext.Entities;

public class Role
{
    [Key]
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    // Navigation properties    
    public List<RolePermission> Permissions { get; set; } = new List<RolePermission>();
}

