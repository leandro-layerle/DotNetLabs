using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaginationLab.Api.Data;
using PaginationLab.Api.Models;

namespace PaginationLab.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _context;

    public OrdersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("setup-correctness")]
    public async Task<IActionResult> SetupCorrectness()
    {
        await _context.Orders.ExecuteDeleteAsync();

        await _context.Database.ExecuteSqlRawAsync(
            "DBCC CHECKIDENT ('Orders', RESEED, 0);");

        var orders = Enumerable.Range(1, 20)
            .Select(number => new Order
            {
                Number = $"ORD-{number:000}"
            })
            .ToList();

        _context.Orders.AddRange(orders);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Dataset de consistencia reiniciado.",
            count = orders.Count,
            ids = orders.Select(order => order.Id)
        });
    }

    [HttpGet("offset")]
    public async Task<IActionResult> Offset(int page = 1, int pageSize = 5)
    {
        if (page < 1 || pageSize < 1)
        {
            return BadRequest("page y pageSize deben ser mayores que cero.");
        }

        var orders = await _context.Orders
            .AsNoTracking()
            .OrderBy(order => order.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(order => new
            {
                order.Id,
                order.Number
            })
            .ToListAsync();

        return Ok(new
        {
            strategy = "offset",
            page,
            pageSize,
            ids = orders.Select(order => order.Id),
            orders
        });
    }

    [HttpGet("keyset")]
    public async Task<IActionResult> Keyset(int lastId = 0, int pageSize = 5)
    {
        if (lastId < 0 || pageSize < 1)
        {
            return BadRequest("Parámetros inválidos.");
        }

        var orders = await _context.Orders
            .AsNoTracking()
            .Where(order => order.Id > lastId)
            .OrderBy(order => order.Id)
            .Take(pageSize)
            .Select(order => new
            {
                order.Id,
                order.Number
            })
            .ToListAsync();

        var nextLastId = orders.Count == 0
            ? lastId
            : orders[^1].Id;

        return Ok(new
        {
            strategy = "keyset",
            lastId,
            pageSize,
            nextLastId,
            ids = orders.Select(order => order.Id),
            orders
        });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var order = await _context.Orders.FindAsync(id);

        if (order is null)
        {
            return NotFound();
        }

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = $"Order {id} eliminado.",
            deletedId = id
        });
    }

    [HttpGet("performance/offset")]
    public async Task<IActionResult> PerformanceOffset(int page = 1, int pageSize = 20)
    {
        if (page < 1 || pageSize < 1)
        {
            return BadRequest("page y pageSize deben ser mayores que cero.");
        }

        var skip = (page - 1) * pageSize;

        var query = _context.Orders
            .AsNoTracking()
            .OrderBy(order => order.Id)
            .Skip(skip)
            .Take(pageSize);

        var sql = query.ToQueryString();
        var stopwatch = Stopwatch.StartNew();
        var orders = await query.ToListAsync();
        stopwatch.Stop();

        return Ok(new
        {
            strategy = "offset",
            page,
            pageSize,
            skip,
            elapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds,
            sql,
            ids = orders.Select(order => order.Id)
        });
    }

    [HttpGet("performance/keyset")]
    public async Task<IActionResult> PerformanceKeyset(int lastId = 0, int pageSize = 20)
    {
        if (lastId < 0 || pageSize < 1)
        {
            return BadRequest("lastId debe ser mayor o igual a cero y pageSize mayor que cero.");
        }

        var query = _context.Orders
            .AsNoTracking()
            .Where(order => order.Id > lastId)
            .OrderBy(order => order.Id)
            .Take(pageSize);

        var sql = query.ToQueryString();
        var stopwatch = Stopwatch.StartNew();
        var orders = await query.ToListAsync();
        stopwatch.Stop();

        return Ok(new
        {
            strategy = "keyset",
            lastId,
            pageSize,
            elapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds,
            sql,
            ids = orders.Select(order => order.Id)
        });
    }
}
