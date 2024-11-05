using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GrpcCrudBoilerplate.DataContext;

namespace GrpcCrudBoilerplate.Services.Authorization
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly AppDbContext _dbContext;
        private readonly IMemoryCache _cache;

        public AuthorizationService(AppDbContext dbContext, IMemoryCache cache)
        {
            _dbContext = dbContext;
            _cache = cache;
        }

        /// <summary>
        /// Check if the user has the given permission
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="permission"></param>
        /// <returns></returns>
        public async Task<bool> HasPermission(int userId, string permission)
        {
            if (userId == 0)
                return false;

            var userPermissions = await GetUserPermissions(userId);
            return userPermissions.Contains(permission);
        }

        /// <summary>
        /// Check if the user has any of the given permissions
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="permissions"></param>
        /// <param name="requireAll">If true, the user must have all permissions. If false, the user must have at least one of the permissions.</param>
        /// <returns></returns>
        public async Task<bool> HasPermission(int userId, string[] permissions, bool requireAll = false)
        {
            if (userId == 0 || permissions == null || permissions.Length == 0)
                return false;

            var userPermissions = await GetUserPermissions(userId);

            return requireAll
                ? permissions.All(permission => userPermissions.Contains(permission))
                : permissions.Any(permission => userPermissions.Contains(permission));
        }

        /// <summary>
        /// Ensure that the user has the given permission
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="permission"></param>
        /// <returns></returns>
        /// <exception cref="UnauthorizedAccessException"></exception>
        public async Task EnsurePermission(int userId, string permission)
        {
            if (!await HasPermission(userId, permission))
            {
                throw new UnauthorizedAccessException($"User {userId} does not have permission: {permission}");
            }
        }

        /// <summary>
        /// Ensure that the user has all of the given permissions
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="permissions"></param>
        /// <param name="requireAll">If true, the user must have all permissions. If false, the user must have at least one of the permissions.</param>
        /// <returns></returns>
        /// <exception cref="UnauthorizedAccessException"></exception>
        public async Task EnsurePermission(int userId, string[] permissions, bool requireAll = true)
        {
            if (!await HasPermission(userId, permissions, requireAll))
            {
                var message = requireAll
                    ? $"User {userId} must have all required permissions: {string.Join(", ", permissions)}"
                    : $"User {userId} must have at least one of these permissions: {string.Join(", ", permissions)}";

                throw new UnauthorizedAccessException(message);
            }
        }

        public async Task<HashSet<string>> GetUserPermissions(int userId)
        {
            var cacheKey = $"user_permissions_{userId}";

            if (!_cache.TryGetValue(cacheKey, out HashSet<string>? userPermissions))
            {
                userPermissions = (await _dbContext.Users
                    .Where(u => u.Id == userId)
                    .SelectMany(u => u.UserRoles)
                    .SelectMany(r => r.Role.Permissions)
                    .Select(p => p.Permission.Name)
                    .ToListAsync())
                    .ToHashSet();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));

                _cache.Set(cacheKey, userPermissions, cacheOptions);
            }

            return userPermissions ?? new HashSet<string>();
        }

    }
}
