using JV.ResultUtilities.Demo.Orders.Models;
using JV.ResultUtilities.ValidationMessage;
using JV.ResultUtilities.ValidationPipeline;

namespace JV.ResultUtilities.Demo.Orders;

/// <summary>
/// Validates CreateOrderRequest using ValidationPipeline with both sync and async rules.
/// Demonstrates: ValidationPipeline&lt;T&gt; with AddRule (sync and async), ValidateAsync
/// </summary>
public static class OrderValidator
{
    public static ValidationPipeline<CreateOrderRequest> CreatePipeline()
    {
        return new ValidationPipeline<CreateOrderRequest>()
            // Sync rules — basic field validation
            .AddRule(ValidateCustomerName)
            .AddRule(ValidateCustomerEmail)
            .AddRule(ValidateCustomerPhone)
            .AddRule(ValidateLines)
            .AddRule(ValidateShippingDeadline)
            .AddRule(ValidateDeliveryTime)
            // Demonstrates: AddRule with async Func<T, Task<Result>> — runs concurrently via Task.WhenAll
            .AddRule(ValidateLineQuantitiesAsync);
    }

    private static Result ValidateCustomerName(CreateOrderRequest request)
    {
        return string.IsNullOrWhiteSpace(request.CustomerName)
            ? Result.Error(OrderValidationKeys.CustomerNameRequired)
            : Result.Ok();
    }

    private static Result ValidateCustomerEmail(CreateOrderRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerEmail) ||
            !request.CustomerEmail.Contains('@'))
            return Result.Error(OrderValidationKeys.CustomerEmailInvalid, request.CustomerEmail ?? "");

        return Result.Ok();
    }

    private static Result ValidateCustomerPhone(CreateOrderRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerPhone) ||
            request.CustomerPhone.Length < 8)
        {
            return Result.Error(
                OrderValidationKeys.CustomerPhoneInvalid,
                request.CustomerPhone ?? "");
        }

        return Result.Ok();
    }

    private static Result ValidateLines(CreateOrderRequest request)
    {
        if (request.Lines is null || request.Lines.Count == 0)
            return Result.Error(OrderValidationKeys.LinesEmpty);

        return Result.Ok();
    }

    private static Result ValidateShippingDeadline(CreateOrderRequest request)
    {
        if (request.ShippingDeadline is null ||
            request.ShippingDeadline.Value < DateOnly.FromDateTime(DateTime.UtcNow))
            return Result.Error(OrderValidationKeys.ShippingDeadlinePast,
                request.ShippingDeadline?.ToString() ?? "");

        return Result.Ok();
    }

    private static Result ValidateDeliveryTime(CreateOrderRequest request)
    {
        // Business rule: delivery time must be between 8:00 and 20:00
        if (request.PreferredDeliveryTime is null ||
            request.PreferredDeliveryTime.Value < new TimeOnly(8, 0) ||
            request.PreferredDeliveryTime.Value > new TimeOnly(20, 0))
            return Result.Error(OrderValidationKeys.DeliveryTimeInvalid,
                request.PreferredDeliveryTime?.ToString() ?? "");

        return Result.Ok();
    }

    /// <summary>
    /// Async validation rule — demonstrates AddRule(Func&lt;T, Task&lt;Result&gt;&gt;).
    /// Validates that all line quantities are within acceptable range.
    /// In a real application this could check against external inventory services.
    /// </summary>
    private static async Task<Result> ValidateLineQuantitiesAsync(CreateOrderRequest request)
    {
        // Simulate async work (e.g., calling an external inventory service)
        await Task.Yield();

        if (request.Lines is null)
            return Result.Ok();

        foreach (var line in request.Lines)
        {
            if (line.Quantity < 1 || line.Quantity > 1000)
                return Result.Error(OrderValidationKeys.LineQuantityInvalid, new object[] { 1, 1000 });
        }

        return Result.Ok();
    }
}
