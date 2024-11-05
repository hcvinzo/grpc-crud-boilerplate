using GrpcCrudBoilerplate.DataContext.Entities;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetOrderWithDetailsAsync(int id, int? userId = null);
    Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime? startDate, DateTime? endDate, bool includeItems = false, int? userId = null);
    Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int userId);
}
