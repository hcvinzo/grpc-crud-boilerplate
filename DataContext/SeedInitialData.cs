using GrpcCrudBoilerplate.DataContext.Entities;

namespace GrpcCrudBoilerplate.DataContext;

public static class SeedInitialData
{
    public static void Initialize(AppDbContext context)
    {
        AddUsers(context);
        AddPermissionCategories(context);
        AddPermissions(context);
        AddRoles(context);
        AddRolePermissions(context);
        AddUserRoles(context);
        AddOrders(context);
    }

    public static void AddUsers(AppDbContext context)
    {
        if (!context.Users.Any())
        {
            context.Users.Add(new User
            {
                Username = "user1",
                Email = "user1@blabla.com",
                CreatedAt = DateTime.UtcNow,
                PasswordHash = Argon2PasswordService.HashPassword("1234"),
                Id = 1,
                IsActive = true
            });

            context.Users.Add(new User
            {
                Username = "user2",
                Email = "user2@blabla.com",
                CreatedAt = DateTime.UtcNow,
                PasswordHash = Argon2PasswordService.HashPassword("1234"),
                Id = 2,
                IsActive = true
            });

            context.SaveChanges();
        }
    }

    public static void AddPermissionCategories(AppDbContext context)
    {
        if (!context.PermissionCategories.Any())
        {
            context.PermissionCategories.Add(new PermissionCategory
            {
                Name = "Order",
                Description = "Order Management Permissions",
                Id = 1
            });

            context.SaveChanges();
        }
    }

    public static void AddPermissions(AppDbContext context)
    {
        if (!context.Permissions.Any())
        {
            context.Permissions.Add(new Permission
            {
                Id = 1,
                Name = "Order.Create",
                Description = "Can create new order",
                CategoryId = 1
            });

            context.Permissions.Add(new Permission
            {
                Id = 2,
                Name = "Order.UpdateAny",
                Description = "Can update any order",
                CategoryId = 1
            });

            context.Permissions.Add(new Permission
            {
                Id = 3,
                Name = "Order.UpdateOwn",
                Description = "Can only update orders created by him/herself",
                CategoryId = 1
            });

            context.Permissions.Add(new Permission
            {
                Id = 4,
                Name = "Order.ViewAny",
                Description = "Can view any orders",
                CategoryId = 1
            });

            context.Permissions.Add(new Permission
            {
                Id = 5,
                Name = "Order.ViewOwn",
                Description = "Can only view orders created by him/herself",
                CategoryId = 1
            });

            context.SaveChanges();
        }
    }

    public static void AddRoles(AppDbContext context)
    {
        if (!context.Roles.Any())
        {
            var role1 = context.Roles.Add(new Role
            {
                Id = 1,
                Name = "OrderManagementFull",
                Description = "Can create new order and update any order",
            });

            var role2 = context.Roles.Add(new Role
            {
                Id = 2,
                Name = "OrderEntry",
                Description = "Can create new order and update own orders"
            });

            context.SaveChanges();
        }
    }

    public static void AddRolePermissions(AppDbContext context)
    {
        if (!context.RolePermissions.Any())
        {
            context.RolePermissions.AddRange(new[]{ new RolePermission
            {
                RoleId = 1,
                PermissionId = 1
            },
            new RolePermission {
                RoleId=1,
                PermissionId=2
            },
            new RolePermission {
                RoleId=1,
                PermissionId=4
            },
            new RolePermission{
                RoleId=2,
                PermissionId=1
            },
            new RolePermission{
                RoleId=2,
                PermissionId=3
            },
            new RolePermission {
                RoleId=2,
                PermissionId=5
            },});

            context.SaveChanges();
        }
    }

    public static void AddUserRoles(AppDbContext context)
    {
        if (!context.UserRoles.Any())
        {
            context.UserRoles.Add(new UserRole
            {
                UserId = 1,
                RoleId = 1
            });

            context.UserRoles.Add(new UserRole
            {
                UserId = 2,
                RoleId = 2
            });

            context.SaveChanges();
        }
    }


    public static void AddOrders(AppDbContext context)
    {
        if (!context.Orders.Any())
        {
            var order1 = new Order { CreatedAt = DateTime.UtcNow.AddDays(-1), Amount = 135, CreatedBy = 1, UpdatedBy = 1, UpdatedAt = DateTime.UtcNow };
            order1.Items.AddRange(new List<OrderItem> {
                new OrderItem { ProductName = "Product 1", Quantity = 10, UnitPrice = 10, Order = order1 },
                new OrderItem { ProductName = "Product 2", Quantity = 5, UnitPrice = 7, Order = order1 }
            });

            var order2 = new Order { CreatedAt = DateTime.UtcNow, Amount = 430, CreatedBy = 2, UpdatedBy = 2, UpdatedAt = DateTime.UtcNow };
            order2.Items.AddRange(new List<OrderItem> {
                new OrderItem { ProductName = "Product 3", Quantity = 6, UnitPrice = 5, Order = order2 },
                new OrderItem { ProductName = "Product 4", Quantity = 4, UnitPrice = 10, Order = order2 }
            });

            context.Orders.AddRange(order1, order2);
            context.SaveChanges();
        }
    }
}

