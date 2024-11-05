using AutoMapper;
using GrpcCrudBoilerplate.Contracts;
using GrpcCrudBoilerplate.DataContext;
using GrpcCrudBoilerplate.DataContext.Entities;
using GrpcCrudBoilerplate.Services.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GrpcCrudBoilerplate.Services.Order;

public interface IOrderService
{
    Task<List<OrderDto>> CreateOrders(List<OrderDto> orderDtos);
    Task<List<OrderDto>> RetrieveOrders(DateTime? startDate = null, DateTime? endDate = null, bool includeItems = false);
    Task<OrderDto?> GetOrderById(int id);
}

public class OrderService : IOrderService
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IUserContext _userContext;
    private readonly IAuthorizationService _authorizationService;
    public OrderService(AppDbContext dbContext, IMapper mapper, IUserContext userContext, IAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _userContext = userContext;
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Create Orders
    /// </summary>
    /// <param name="orderDtos"></param>
    /// <returns></returns>
    public async Task<List<OrderDto>> CreateOrders(List<OrderDto> orderDtos)
    {
        // Ensure the user has the necessary permission
        await _authorizationService.EnsurePermission(_userContext.UserId, "Order.Create");

        var orders = new List<DataContext.Entities.Order>();

        foreach (var orderDto in orderDtos)
        {
            var order = _mapper.Map<DataContext.Entities.Order>(orderDto);

            order.CreatedBy = order.UpdatedBy = _userContext.UserId;

            // Set creation and update timestamps
            order.CreatedAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;

            // Calculate total amount from order items
            order.Amount = order.Items.Sum(item => item.Quantity * item.UnitPrice);

            orders.Add(order);
        }

        await _dbContext.Orders.AddRangeAsync(orders);
        await _dbContext.SaveChangesAsync();

        return _mapper.Map<List<OrderDto>>(orders);
    }

    /// <summary>
    /// Retrieve Orders from given criteria
    /// </summary>
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    /// <param name="includeItems"></param> 
    /// <returns></returns>
    public async Task<List<OrderDto>> RetrieveOrders(DateTime? startDate = null, DateTime? endDate = null, bool includeItems = false)
    {
        // Ensure the user has the necessary permission
        await _authorizationService.EnsurePermission(_userContext.UserId, ["Order.ViewAny", "Order.ViewOwn"]);

        var query = _dbContext.Orders
            .Include(o => o.Creator)
            .Include(o => o.Updater)
            .AsQueryable();

        // Include items only if requested
        if (includeItems)
        {
            query = query.Include(o => o.Items);
        }

        // Apply date filters if provided
        if (startDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt <= endDate.Value);
        }

        // Filter by user if the user has only "Order.ViewOwn" permission
        if (!await _authorizationService.HasPermission(_userContext.UserId, "Order.ViewAny"))
        {
            query = query.Where(o => o.CreatedBy == _userContext.UserId);
        }

        // Order by creation date descending (newest first)
        query = query.OrderByDescending(o => o.CreatedAt);

        var orders = await query.ToListAsync();
        return _mapper.Map<List<OrderDto>>(orders);
    }

    /// <summary>
    /// Retrieves Order by Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<OrderDto?> GetOrderById(int id)
    {
        // Ensure the user has the necessary permission
        await _authorizationService.EnsurePermission(_userContext.UserId, ["Order.ViewAny", "Order.ViewOwn"]);

        var order = await _dbContext.Orders
            .Include(o => o.Items)
            .Include(o => o.Creator)
            .Include(o => o.Updater)
            .FirstOrDefaultAsync(o => o.Id == id);

        // Filter by user if the user has only "Order.ViewOwn" permission
        if (!await _authorizationService.HasPermission(_userContext.UserId, "Order.ViewAny"))
        {
            order = order?.CreatedBy == _userContext.UserId ? order : null;
        }

        if (order == null)
            return null;

        return _mapper.Map<OrderDto>(order);
    }
}
