using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GrpcCrudBoilerplate.DataContext.Entities;

public class User
{
    [Key]
    public int Id { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt
    { get; set; }
    public DateTime? LastLoginAt { get; set; }

    // Navigation properties
    public List<Order> Orders { get; set; } = new List<Order>();
    public List<UserRole> UserRoles { get; set; } = new List<UserRole>();

}


