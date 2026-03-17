using JV.ResultUtilities.Demo.Orders.Models;
using JV.ResultUtilities.Demo.Products;
using JV.ResultUtilities.Extensions;

namespace JV.ResultUtilities.Demo.Orders;

/// <summary>
/// Business logic for Orders. Demonstrates the async Result operations, collection extensions,
/// merge, ThrowIfFailure, implicit conversions, and more.
/// </summary>
public class OrderService(
    OrderRepository orderRepository,
    ProductRepository productRepository,
    ILogger<OrderService> logger)
{
    /// <summary>
    /// Get order by ID.
    /// Demonstrates: implicit conversion (TValue -> Result&lt;T&gt;), implicit conversion
    /// (ValidationMessage -> Result&lt;T&gt;), MatchAsync (used in controller)
    /// </summary>
    public async Task<Result<OrderResponse>> GetByIdAsync(Guid id)
    {
        var orderResult = await orderRepository.GetByIdAsync(id);
        if (orderResult.IsFailure)
        {
            // Demonstrates: implicit conversion — ValidationMessage -> Result<OrderResponse>
            // Returning a single ValidationMessage directly as a Result<T>
            return JV.ResultUtilities.ValidationMessage.ValidationMessage.Create(
                OrderValidationKeys.NotFound, id);
        }

        // Demonstrates: implicit conversion — TValue -> Result<T>
        // Returning a bare value from a method that returns Result<T>
        return ToResponse(orderResult.Value);
    }

    /// <summary>
    /// Get order summary as diagnostic string.
    /// Demonstrates: ToString(), ToStringWithParameters() on ResultType
    /// </summary>
    public async Task<Result<string>> GetOrderSummaryAsync(Guid id)
    {
        var result = await orderRepository.GetByIdAsync(id);

        // Demonstrates: ToString() and ToStringWithParameters()
        if (result.IsFailure)
        {
            logger.LogWarning("Order lookup failed: {Errors}", result.ToString());
            logger.LogWarning("Order lookup failed (detailed): {Errors}", result.ToStringWithParameters());
        }

        return result.Map(order =>
            $"Order {order.Id}: {order.CustomerName}, {order.Lines.Count} items, " +
            $"Total: {order.TotalAmount:C}, Status: {order.Status}");
    }

    /// <summary>
    /// Create an order with full validation, stock checking, and persistence.
    /// Demonstrates: ValidationPipeline.ValidateAsync, TraverseAllAsync (all-or-nothing stock check),
    /// MergeResults, BindAsync chain, DoAsync (side-effect), Result.TryAsync,
    /// implicit conversion (Result&lt;T&gt; failure forwarding)
    /// </summary>
    public async Task<Result<OrderResponse>> CreateAsync(CreateOrderRequest request)
    {
        var pipeline = OrderValidator.CreatePipeline();

        // Demonstrates: ValidateAsync — runs all sync rules, then async rules concurrently
        var validationResult = await pipeline.ValidateAsync(request);
        if (validationResult.IsFailure)
        {
            // Demonstrates: Cast<T> — forward a failed Result<A> to Result<B>
            return validationResult.Cast<OrderResponse>();
        }

        // Demonstrates: TraverseAllAsync — validate all lines, collect ALL errors
        var linesResult = await request.Lines.TraverseAllAsync(async line =>
        {
            var productResult = await productRepository.GetByIdAsync(line.ProductId);

            // Demonstrates: Bind — chain stock validation onto the product lookup
            return productResult.Bind(product =>
            {
                if (product.StockQuantity < line.Quantity)
                    return Result.Error(OrderValidationKeys.LineInsufficientStock,
                        new object[] { product.StockQuantity, product.Name, line.Quantity });

                return Result.Ok(new OrderLine
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Quantity = line.Quantity,
                    UnitPrice = product.Price
                });
            });
        });

        if (linesResult.IsFailure)
            return Result.Error(linesResult.ValidationMessages);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            CustomerPhone = request.CustomerPhone,
            Status = OrderStatus.Pending,
            OrderDate = DateTime.UtcNow,
            ShippingDeadline = request.ShippingDeadline!.Value,
            PreferredDeliveryTime = request.PreferredDeliveryTime!.Value,
            EstimatedDelivery = TimeSpan.FromDays(3),
            IsPriority = request.IsPriority,
            Notes = request.Notes,
            Lines = linesResult.Value.ToList()
        };

        // Demonstrates: BindAsync — chain async save operation
        var saveResult = await Result.Ok(order)
            .BindAsync(async o => await orderRepository.AddAsync(o));

        // Demonstrates: DoAsync — async side-effect (e.g., send notification)
        return await saveResult
            .DoAsync(async o =>
            {
                logger.LogInformation("Order {OrderId} created for {Customer} with {LineCount} items",
                    o.Id, o.CustomerName, o.Lines.Count);
                await Task.CompletedTask; // placeholder for actual notification
            })
            .MapAsync(async o =>
            {
                await Task.CompletedTask;
                return ToResponse(o);
            });

    }

    /// <summary>
    /// Update order status (confirm, ship).
    /// Demonstrates: Ensure, Filter (alias for Ensure), BindSync, Match
    /// </summary>
    public async Task<Result<OrderResponse>> UpdateStatusAsync(Guid id, UpdateOrderStatusRequest request)
    {
        var result = await orderRepository.GetByIdAsync(id);

        return await result
            .Ensure(o => o.Status != OrderStatus.Cancelled,
                OrderValidationKeys.CannotShip, id, "Cancelled")
            .Ensure(o => o.Status != OrderStatus.Delivered,
                OrderValidationKeys.CannotShip, id, "Delivered")
            // Demonstrates: Ensure — validates a condition on the value
            .Ensure(o => request.Status != OrderStatus.Shipped || o.Status == OrderStatus.Confirmed,
                OrderValidationKeys.CannotShip, id, result.IsSuccessful ? result.Value.Status.ToString() : "unknown")
            .Map(order =>
            {
                order.Status = request.Status;
                if (request.Status == OrderStatus.Shipped && request.TrackingUri is not null)
                    order.TrackingUri = new Uri(request.TrackingUri);
                return order;
            })
            // Demonstrates: BindAsync — chain async persistence
            .BindAsync(order => orderRepository.UpdateAsync(order))
            // Demonstrates: MapSync — sync mapper on Task<Result<T>>
            .MapSync(order => ToResponse(order));
    }

    /// <summary>
    /// Cancel an order — uses ThrowIfFailure for the critical path.
    /// Demonstrates: ThrowIfFailure, ResultException (caught by ResultExceptionHandler),
    /// Merge on Result&lt;T&gt; with params Result[]
    /// </summary>
    public async Task<Result<OrderResponse>> CancelAsync(Guid id)
    {
        var orderResult = await orderRepository.GetByIdAsync(id);

        // Demonstrates: Merge on Result<T> — merge with additional validation Result
        var statusCheck = orderResult.IsSuccessful
            ? (orderResult.Value.Status is OrderStatus.Shipped or OrderStatus.Delivered
                ? Result.Error(OrderValidationKeys.CannotCancel, new object[] { id, orderResult.Value.Status.ToString() })
                : Result.Ok())
            : Result.Ok(); // errors already in orderResult

        var merged = orderResult.Merge(statusCheck);
        // Demonstrates: Merge(params Result[]) — combines validation messages from non-generic
        // Result into the typed Result<Order>, keeping the original Value

        // Demonstrates: ThrowIfFailure — throws ResultException if the result is a failure
        // This is caught by ResultExceptionHandler in the middleware pipeline
        merged.ThrowIfFailure();

        var order = merged.Value;
        order.Status = OrderStatus.Cancelled;

        return (await orderRepository.UpdateAsync(order))
            .Map(ToResponse);
    }

    /// <summary>
    /// Check bulk stock availability — returns what's available, skips unavailable.
    /// Demonstrates: TraversePartialAsync (parallel execution, keep only successes),
    /// MergeResults on IEnumerable&lt;Result&gt;
    /// </summary>
    public async Task<Result<IEnumerable<OrderLineResponse>>> CheckBulkAvailabilityAsync(
        List<CreateOrderLineRequest> lines)
    {
        // Demonstrates: TraversePartialAsync — runs in parallel, returns only successful results
        var result = await lines.TraversePartialAsync(async line =>
        {
            var productResult = await productRepository.GetByIdAsync(line.ProductId);
            return productResult.Bind(product =>
                product.StockQuantity >= line.Quantity
                    ? Result.Ok(new OrderLineResponse(
                        product.Id, product.Name, line.Quantity,
                        product.Price, line.Quantity * product.Price))
                    : Result.Error(OrderValidationKeys.LineInsufficientStock,
                        new object[] { product.StockQuantity, product.Name, line.Quantity }));
        });

        return result;
    }

    /// <summary>
    /// Demonstrates: MergeResults on IEnumerable&lt;Result&gt;
    /// Validates multiple independent business rules and merges all results.
    /// </summary>
    public async Task<Result> ValidateOrderIntegrityAsync(Guid id)
    {
        var orderResult = await orderRepository.GetByIdAsync(id);
        if (orderResult.IsFailure)
            return Result.Error(orderResult.ValidationMessages);

        var order = orderResult.Value;

        // Create multiple independent validation checks
        var checks = new List<Result>
        {
            order.Lines.Count > 0 ? Result.Ok() : Result.Error(OrderValidationKeys.LinesEmpty),
            order.Status != OrderStatus.Cancelled ? Result.Ok() : Result.Error(OrderValidationKeys.CannotShip, new object[] { id, "Cancelled" }),
            order.ShippingDeadline >= DateOnly.FromDateTime(DateTime.UtcNow) ? Result.Ok() : Result.Error(OrderValidationKeys.ShippingDeadlinePast, order.ShippingDeadline)
        };

        // Demonstrates: MergeResults — aggregates all errors from a collection of Results
        return checks.MergeResults();
    }

    private static OrderResponse ToResponse(Order order) => new(
        order.Id,
        order.CustomerName,
        order.CustomerEmail,
        order.Status.ToString(),
        order.OrderDate,
        order.ShippingDeadline,
        order.TotalAmount,
        order.IsPriority,
        order.Notes,
        order.Lines.Select(l => new OrderLineResponse(
            l.ProductId, l.ProductName, l.Quantity, l.UnitPrice, l.LineTotal)).ToList());
}
