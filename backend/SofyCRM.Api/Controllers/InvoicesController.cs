using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SofyCRM.Api.Auth;
using SofyCRM.Api.Data;
using SofyCRM.Api.Entities;

namespace SofyCRM.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/invoices")]
public class InvoicesController : ControllerBase
{
    private readonly AppDbContext _db;
    public InvoicesController(AppDbContext db) { _db = db; }

    [HttpGet]
    public async Task<ActionResult<object>> List(
        [FromQuery] PaymentStatus? status,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var q = _db.Invoices.AsNoTracking().Include(i => i.Customer).AsQueryable();
        if (status is not null) q = q.Where(i => i.PaymentStatus == status);
        var total = await q.CountAsync();
        var items = await q.OrderByDescending(i => i.IssuedDate)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new { items, total, page, pageSize });
    }

    [HttpPost]
    [Authorize(Roles = Roles.AdminOrSales)]
    public async Task<ActionResult<Invoice>> Create([FromBody] Invoice dto)
    {
        dto.Id = Guid.NewGuid();
        dto.CreatedAt = dto.UpdatedAt = DateTime.UtcNow;
        if (string.IsNullOrWhiteSpace(dto.InvoiceNo))
            dto.InvoiceNo = $"INV-{DateTime.UtcNow:yyyyMM}-{Random.Shared.Next(1000, 9999)}";
        _db.Invoices.Add(dto);
        await _db.SaveChangesAsync();
        return Ok(dto);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.AdminOrSales)]
    public async Task<ActionResult<Invoice>> Update(Guid id, [FromBody] Invoice dto)
    {
        var i = await _db.Invoices.FindAsync(id);
        if (i is null) return NotFound();
        i.Amount = dto.Amount; i.IssuedDate = dto.IssuedDate; i.DueDate = dto.DueDate;
        i.PaymentStatus = dto.PaymentStatus; i.PaidAmount = dto.PaidAmount;
        i.Notes = dto.Notes; i.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(i);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var i = await _db.Invoices.FindAsync(id);
        if (i is null) return NotFound();
        _db.Invoices.Remove(i);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
