namespace JV.ResultUtilities.Demo.Orders.Models;

public class Order
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime OrderDate { get; set; }
    public DateOnly ShippingDeadline { get; set; }
    public TimeOnly PreferredDeliveryTime { get; set; }
    public TimeSpan EstimatedDelivery { get; set; }
    public string? Notes { get; set; }
    public Uri? TrackingUri { get; set; }
    public bool IsPriority { get; set; }
    public List<OrderLine> Lines { get; set; } = [];

    public decimal TotalAmount => Lines.Sum(l => l.LineTotal);
}
