using JV.ResultUtilities.Extensions;
using JV.ResultUtilities.ValidationMessage;

namespace JV.ResultUtilities.Demo.Orders;

/// <summary>
/// All ValidationKeyDefinitions for the Order domain.
/// Demonstrates: All remaining ParameterType variants — Email, PhoneNumber, Guid, DateOnly,
/// TimeOnly, TimeSpan, DateTime, Boolean, Uri — to exercise the full builder API.
/// </summary>
public static class OrderValidationKeys
{
    public static readonly ValidationKeyDefinition CustomerNameRequired =
        ValidationKeyDefinition.Create("order.customer_name.required");

    // Demonstrates: WithStringParameter for invalid input (use CreateLenient when strict types cause issues)
    public static readonly ValidationKeyDefinition CustomerEmailInvalid =
        ValidationKeyDefinition.Create("order.customer_email.invalid")
            .WithStringParameter("email"); // Using String because the email might be invalid input

    // Uses WithStringParameter because the phone value in the error IS the invalid input.
    // Note: ValidationMessage.CreateLenient can also be used to skip parameter type validation.
    public static readonly ValidationKeyDefinition CustomerPhoneInvalid =
        ValidationKeyDefinition.Create("order.customer_phone.invalid")
            .WithStringParameter("phone");

    public static readonly ValidationKeyDefinition LinesEmpty =
        ValidationKeyDefinition.Create("order.lines.empty");

    public static readonly ValidationKeyDefinition LineQuantityInvalid =
        ValidationKeyDefinition.Create("order.line.quantity.invalid")
            .WithIntParameter("min")
            .WithIntParameter("max");

    // Demonstrates: WithGuidParameter
    public static readonly ValidationKeyDefinition LineProductNotFound =
        ValidationKeyDefinition.Create("order.line.product_not_found")
            .WithGuidParameter("productId");

    public static readonly ValidationKeyDefinition LineInsufficientStock =
        ValidationKeyDefinition.Create("order.line.insufficient_stock")
            .WithIntParameter("available")
            .WithStringParameter("productName")
            .WithIntParameter("requested");

    // Demonstrates: WithDateOnlyParameter
    public static readonly ValidationKeyDefinition ShippingDeadlinePast =
        ValidationKeyDefinition.Create("order.shipping_deadline.past")
            .WithDateOnlyParameter("deadline");

    // Demonstrates: WithTimeOnlyParameter
    public static readonly ValidationKeyDefinition DeliveryTimeInvalid =
        ValidationKeyDefinition.Create("order.delivery_time.invalid")
            .WithTimeOnlyParameter("time");

    // Demonstrates: WithTimeSpanParameter
    public static readonly ValidationKeyDefinition EstimatedDeliveryInvalid =
        ValidationKeyDefinition.Create("order.estimated_delivery.invalid")
            .WithTimeSpanParameter("duration");

    // Demonstrates: WithDateTimeParameter
    public static readonly ValidationKeyDefinition OrderDateInvalid =
        ValidationKeyDefinition.Create("order.date.invalid")
            .WithDateTimeParameter("date");

    // Demonstrates: WithBooleanParameter
    public static readonly ValidationKeyDefinition PriorityNotAvailable =
        ValidationKeyDefinition.Create("order.priority.not_available")
            .WithBooleanParameter("requested");

    // Demonstrates: WithUriParameter
    public static readonly ValidationKeyDefinition TrackingUriInvalid =
        ValidationKeyDefinition.Create("order.tracking_uri.invalid")
            .WithUriParameter("uri");

    public static readonly ValidationKeyDefinition NotFound =
        ValidationKeyDefinition.Create("order.not_found")
            .WithGuidParameter("id");

    public static readonly ValidationKeyDefinition CannotCancel =
        ValidationKeyDefinition.Create("order.cannot_cancel")
            .WithGuidParameter("id")
            .WithStringParameter("status");

    public static readonly ValidationKeyDefinition CannotShip =
        ValidationKeyDefinition.Create("order.cannot_ship")
            .WithGuidParameter("id")
            .WithStringParameter("status");

    // Demonstrates: WithEmailParameter — used for valid email confirmation scenarios
    public static readonly ValidationKeyDefinition EmailConfirmation =
        ValidationKeyDefinition.Create("order.email.confirmation")
            .WithEmailParameter("email");

    public static readonly ValidationKeyDefinition SaveFailed =
        ValidationKeyDefinition.Create("order.save_failed")
            .WithStringParameter("error");
}
