using JV.ResultUtilities.Demo.Products.Models;

namespace JV.ResultUtilities.Demo.Orders.Models;

public class OrderLine
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal => Quantity * UnitPrice;

    public Product? Product { get; set; }
}
