namespace JV.ResultUtilities.Demo.Orders.Models;

public record OrderResponse(
    Guid Id,
    string CustomerName,
    string CustomerEmail,
    string Status,
    DateTime OrderDate,
    DateOnly ShippingDeadline,
    decimal TotalAmount,
    bool IsPriority,
    string? Notes,
    List<OrderLineResponse> Lines);

public record OrderLineResponse(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);
