
using CsvHelper.Configuration.Attributes;

namespace GrpcCrudBoilerplate.Contracts;

public class OrderDto
{
    [Optional]
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public decimal Amount { get; set; }
    public int CreatedBy { get; set; }
    public int UpdatedBy { get; set; }

    // Creator and Updater information
    public string? CreatorUsername { get; set; }
    public string? UpdaterUsername { get; set; }

    // Order items
    public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
}

public class OrderItemDto
{
    [Optional]
    public int Id { get; set; }
    public int OrderId { get; set; }
    public required string ProductName { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;
}
