namespace JV.ResultUtilities.Demo.Orders.Models;

public record CreateOrderRequest(
    string? CustomerName,
    string? CustomerEmail,
    string? CustomerPhone,
    DateOnly? ShippingDeadline,
    TimeOnly? PreferredDeliveryTime,
    bool IsPriority,
    string? Notes,
    List<CreateOrderLineRequest>? Lines);

public record CreateOrderLineRequest(
    Guid ProductId,
    int Quantity);
