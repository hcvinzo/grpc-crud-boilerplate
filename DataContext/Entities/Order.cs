using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrpcCrudBoilerplate.DataContext.Entities;

public class Order
{
    [Key]
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public decimal Amount { get; set; }
    public int CreatedBy { get; set; }
    public int UpdatedBy { get; set; }

    // Navigation properties    
    public User? Creator { get; set; }
    public User? Updater { get; set; }
    public List<OrderItem> Items { get; set; } = new List<OrderItem>();
}

public class OrderItem
{
    [Key]
    public int Id { get; set; }
    public int OrderId { get; set; }
    public required string ProductName { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    // Navigation property back to the parent Order
    public required Order Order { get; set; }
}
