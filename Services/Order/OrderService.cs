using AutoMapper;
using GrpcCrudBoilerplate.Contracts;
using GrpcCrudBoilerplate.DataContext;
using GrpcCrudBoilerplate.DataContext.Entities;
using GrpcCrudBoilerplate.Services.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GrpcCrudBoilerplate.Services.Order;

public interface IOrderService
{
    /// <summary>
    /// Create Orders
    /// </summary>
    /// <param name="orderDtos"></param>
    /// <returns></returns>
    Task<List<OrderDto>> CreateOrders(List<OrderDto> orderDtos);

    /// <summary>
    /// Retrieve Orders from given criteria
    /// </summary>
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    /// <param name="includeItems"></param> 
    /// <returns></returns>
    Task<List<OrderDto>> RetrieveOrders(DateTime? startDate = null, DateTime? endDate = null, bool includeItems = false);

    /// <summary>
    /// Retrieve Order by Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<OrderDto?> GetOrderById(int id);

    /// <summary>
    /// Update Order
    /// </summary>
    Task<OrderDto> UpdateOrder(int id, OrderDto orderDto);
}

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IUserContext _userContext;
    private readonly IAuthorizationService _authorizationService;
    public OrderService(IOrderRepository orderRepository, AppDbContext dbContext, IMapper mapper, IUserContext userContext, IAuthorizationService authorizationService)
    {
        _orderRepository = orderRepository;
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

        // Map DTOs to entities
        var orders = _mapper.Map<List<DataContext.Entities.Order>>(orderDtos);

        foreach (var order in orders)
        {
            order.CreatedAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;
            order.CreatedBy = _userContext.UserId;
            order.UpdatedBy = _userContext.UserId;

            // Calculate total amount
            order.Amount = order.Items.Sum(item => item.Quantity * item.UnitPrice);
        }

        // Add entities to the database
        var createdOrders = await _orderRepository.AddRangeAsync(orders);

        return _mapper.Map<List<OrderDto>>(createdOrders);
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

        // Determine the user ID to filter by based on their permissions
        int? userId = await _authorizationService.HasPermission(_userContext.UserId, "Order.ViewAny") ? null : (int?)_userContext.UserId;

        var orders = await _orderRepository.GetOrdersByDateRangeAsync(startDate, endDate, includeItems, userId);

        // Order by creation date descending (newest first)
        orders = orders.OrderByDescending(o => o.CreatedAt).ToList();

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

        // Determine the user ID to filter by based on their permissions
        int? userId = await _authorizationService.HasPermission(_userContext.UserId, "Order.ViewAny") ? null : (int?)_userContext.UserId;

        var order = await _orderRepository.GetOrderWithDetailsAsync(id, userId);

        if (order == null)
            return null;

        return _mapper.Map<OrderDto>(order);
    }

    /// <summary>
    /// Update Order
    /// </summary>
    /// <param name="id"></param>
    /// <param name="orderDto"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="UnauthorizedAccessException"></exception>
    public async Task<OrderDto> UpdateOrder(int id, OrderDto orderDto)
    {
        // Ensure the user has the necessary permission
        await _authorizationService.EnsurePermission(_userContext.UserId, ["Order.UpdateAny", "Order.UpdateOwn"]);

        // Determine if user can update any order or only their own
        int? userId = await _authorizationService.HasPermission(_userContext.UserId, "Order.UpdateAny")
            ? null
            : (int?)_userContext.UserId;

        // Get existing order with details
        var existingOrder = await _orderRepository.GetOrderWithDetailsAsync(id, userId);

        if (existingOrder == null)
        {
            throw new ArgumentException($"Order with ID {id} not found or you don't have permission to update it");
        }

        // Update order properties
        existingOrder.UpdatedAt = DateTime.UtcNow;
        existingOrder.UpdatedBy = _userContext.UserId;

        // Update items
        existingOrder.Items.Clear();
        foreach (var itemDto in orderDto.Items)
        {
            existingOrder.Items.Add(new OrderItem
            {
                ProductName = itemDto.ProductName,
                Quantity = itemDto.Quantity,
                UnitPrice = itemDto.UnitPrice,
                Order = existingOrder
            });
        }

        // Calculate total amount
        existingOrder.Amount = existingOrder.Items.Sum(item => item.Quantity * item.UnitPrice);

        // Update the order
        await _orderRepository.UpdateAsync(existingOrder);

        return _mapper.Map<OrderDto>(existingOrder);
    }

}
