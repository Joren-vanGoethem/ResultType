using JV.ResultUtilities.Demo.Infrastructure;
using JV.ResultUtilities.Demo.Orders.Models;
using JV.ResultUtilities.Extensions;
using Microsoft.EntityFrameworkCore;

namespace JV.ResultUtilities.Demo.Orders;

/// <summary>
/// Wraps EF Core calls for Orders. Returns Result types throughout.
/// Demonstrates: Result.TryAsync(), Result.Ok(), Result.Error()
/// </summary>
public class OrderRepository(AppDbContext db)
{
    public async Task<Result<Order>> GetByIdAsync(Guid id)
    {
        var order = await db.Orders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null)
            return Result.Error(OrderValidationKeys.NotFound, id);

        return Result.Ok(order);
    }

    public async Task<Result<List<Order>>> GetAllAsync()
    {
        var orders = await db.Orders
            .Include(o => o.Lines)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
        return Result.Ok(orders);
    }

    public async Task<Result<Order>> AddAsync(Order order)
    {
        return await Result.TryAsync(async () =>
        {
            db.Orders.Add(order);
            await db.SaveChangesAsync();
            return order;
        }, OrderValidationKeys.SaveFailed);
    }

    public async Task<Result<Order>> UpdateAsync(Order order)
    {
        return await Result.TryAsync(async () =>
        {
            db.Orders.Update(order);
            await db.SaveChangesAsync();
            return order;
        }, OrderValidationKeys.SaveFailed);
    }
}
