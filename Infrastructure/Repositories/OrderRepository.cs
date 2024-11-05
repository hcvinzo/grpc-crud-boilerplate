using Microsoft.EntityFrameworkCore;
using GrpcCrudBoilerplate.DataContext;
using GrpcCrudBoilerplate.DataContext.Entities;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Order?> GetOrderWithDetailsAsync(int id, int? userId = null)
    {
        var query = _dbSet.AsQueryable();

        if (userId.HasValue)
            query = query.Where(o => o.CreatedBy == userId.Value);

        return await query
            .Include(o => o.Items)
            .Include(o => o.Creator)
            .Include(o => o.Updater)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime? startDate, DateTime? endDate, bool includeItems = false, int? userId = null)
    {
        var query = _dbSet.AsQueryable();

        // Include items only if requested
        if (includeItems)
            query = query.Include(o => o.Items);

        if (startDate.HasValue)
            query = query.Where(o => o.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(o => o.CreatedAt <= endDate.Value);

        // Filter by user if userId is provided
        if (userId.HasValue)
            query = query.Where(o => o.CreatedBy == userId.Value);

        return await query
            .Include(o => o.Creator)
            .Include(o => o.Updater)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int userId)
    {
        return await _dbSet
            .Include(o => o.Items)
            .Include(o => o.Creator)
            .Include(o => o.Updater)
            .Where(o => o.CreatedBy == userId)
            .ToListAsync();
    }
}
