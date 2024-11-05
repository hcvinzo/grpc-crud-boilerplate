using System;
using System.ComponentModel.DataAnnotations;

namespace GrpcCrudBoilerplate.DataContext.Entities;

public class UserRole
{
    [Key]
    public required int UserId { get; set; }
    public User? User { get; set; }

    public required int RoleId { get; set; }
    public Role? Role { get; set; }

    public DateTime AssignedAt { get; set; }


}

