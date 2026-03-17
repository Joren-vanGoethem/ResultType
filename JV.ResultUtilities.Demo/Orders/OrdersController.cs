using JV.ResultUtilities.Demo.Infrastructure;
using JV.ResultUtilities.Demo.Orders.Models;
using JV.ResultUtilities.Demo.Translations;
using JV.ResultUtilities.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace JV.ResultUtilities.Demo.Orders;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(
    OrderService orderService,
    ITranslator translator) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await orderService.GetByIdAsync(id);

        return await result.MatchAsync(
            onSuccess: order => Task.FromResult<IActionResult>(Ok(order)),
            onFailure: messages =>
            {
                if (messages.Any(m => m.KeyDefinition?.Key == "order.not_found"))
                    return Task.FromResult<IActionResult>(NotFound());
                return Task.FromResult(result.ToActionResult(translator));
            });
    }

    [HttpGet("{id:guid}/summary")]
    public async Task<IActionResult> GetSummary(Guid id)
    {
        return await orderService.GetOrderSummaryAsync(id)
            .ToActionResultAsync(translator);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
    {
        return await orderService.CreateAsync(request)
            .ToActionResultAsync(translator,
                order => CreatedAtAction(nameof(GetById), new { id = order.Id }, order));
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
    {
        return await orderService.UpdateStatusAsync(id, request)
            .ToActionResultAsync(translator);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var result = await orderService.CancelAsync(id);
        return result.ToActionResult(translator);
    }

    [HttpPost("check-availability")]
    public async Task<IActionResult> CheckAvailability([FromBody] List<CreateOrderLineRequest> lines)
    {
        return await orderService.CheckBulkAvailabilityAsync(lines)
            .ToActionResultAsync(translator);
    }

    [HttpPost("{id:guid}/validate")]
    public async Task<IActionResult> ValidateIntegrity(Guid id)
    {
        return await orderService.ValidateOrderIntegrityAsync(id)
            .ToActionResultAsync(translator);
    }
}
