namespace JV.ResultUtilities.Demo.Orders.Models;

public record UpdateOrderStatusRequest(
    OrderStatus Status,
    string? TrackingUri);
