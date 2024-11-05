using Grpc.Core;
using GrpcCrudBoilerplate.Contracts;
using GrpcCrudBoilerplate.Infrastructure.Authorization;
using GrpcCrudBoilerplate.Services.Order;
using GrpcCrudBoilerplate.v1;
using Microsoft.AspNetCore.Authorization;

namespace GrpcCrudBoilerplate.GrpcServices.v1;

[Authorize]
public class OrderService : Order.OrderBase
{
    private readonly IOrderService _orderService;

    public OrderService(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Creates new Order
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    [RequirePermission("Order.Create")]
    public override async Task<CreateOrderResponse> CreateOrder(CreateOrderRequest request, ServerCallContext context)
    {
        var orderDto = new OrderDto
        {
            Items = request.Items.Select(item => new OrderItemDto
            {
                ProductName = item.ProductName,
                Quantity = (decimal)item.Quantity,
                UnitPrice = (decimal)item.UnitPrice
            }).ToList()
        };

        var createdOrders = await _orderService.CreateOrders(new List<OrderDto> { orderDto });
        var createdOrder = createdOrders.FirstOrDefault();

        if (createdOrder != null)
        {
            return new CreateOrderResponse
            {
                Id = createdOrder.Id,
                CreatedAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(createdOrder.CreatedAt.ToUniversalTime()),
                UpdatedAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(createdOrder.UpdatedAt.ToUniversalTime()),
                Amount = (double)createdOrder.Amount,
                Items = { createdOrder.Items.Select(item => new OrderItem
            {
                ProductName = item.ProductName,
                Quantity = (double)item.Quantity,
                UnitPrice = (double)item.UnitPrice,
                TotalPrice = (double)item.TotalPrice
            }) }
            };
        }
        else
        {
            return new CreateOrderResponse();
        }
    }

    /// <summary>
    /// Retrieves Order by Id
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    /// <exception cref="RpcException"></exception>
    [RequirePermission(["Order.ViewAny", "Order.ViewOwn"])]
    public override async Task<GetOrderResponse> GetOrder(GetOrderRequest request, ServerCallContext context)
    {
        var order = await _orderService.GetOrderById(request.Id);

        if (order == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Order with ID {request.Id} not found"));
        }

        return new GetOrderResponse
        {
            Id = order.Id,
            CreatedAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(order.CreatedAt.ToUniversalTime()),
            UpdatedAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(order.UpdatedAt.ToUniversalTime()),
            Amount = (double)order.Amount,
            CreatorUsername = order.CreatorUsername ?? "",
            UpdaterUsername = order.UpdaterUsername ?? "",
            Items = { order.Items.Select(item => new OrderItem
            {
                ProductName = item.ProductName,
                Quantity = (double)item.Quantity,
                UnitPrice = (double)item.UnitPrice,
                TotalPrice = (double)item.TotalPrice
            }) }
        };
    }
}