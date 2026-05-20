using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SofyCRM.Api.Auth;
using SofyCRM.Api.Data;
using SofyCRM.Api.Entities;

namespace SofyCRM.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/quotations")]
public class QuotationsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _me;
    public QuotationsController(AppDbContext db, ICurrentUser me) { _db = db; _me = me; }

    private IQueryable<Quotation> Scoped()
    {
        var q = _db.Quotations.AsNoTracking()
            .Include(x => x.Customer).Include(x => x.Items).AsQueryable();
        if (_me.IsAdmin) return q;
        if (_me.IsSales) return q.Where(x => x.CreatedBy == _me.UserId
                                          || x.Customer!.OwnerUserId == _me.UserId);
        return q.Where(_ => false);
    }

    [HttpGet]
    public async Task<ActionResult<object>> List(
        [FromQuery] QuotationStatus? status,
        [FromQuery] Guid? customerId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var q = Scoped();
        if (status is not null)     q = q.Where(x => x.Status == status);
        if (customerId is not null) q = q.Where(x => x.CustomerId == customerId);

        var total = await q.CountAsync();
        var items = await q.OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new { items, total, page, pageSize });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Quotation>> Get(Guid id)
    {
        var q = await Scoped().FirstOrDefaultAsync(x => x.Id == id);
        if (q is null) return NotFound();
        return Ok(q);
    }

    [HttpPost]
    [Authorize(Roles = Roles.AdminOrSales)]
    public async Task<ActionResult<Quotation>> Create([FromBody] Quotation dto)
    {
        dto.Id = Guid.NewGuid();
        dto.CreatedBy = _me.UserId;
        // 規則：PO-yyyyMMdd-XXX（每日從 001 流水遞增），總是後端產生避免手動竄改
        var today      = DateTime.UtcNow.Date;
        var prefix     = $"PO-{today:yyyyMMdd}-";
        var countToday = await _db.Quotations.CountAsync(x => x.CreatedAt >= today && x.CreatedAt < today.AddDays(1));
        dto.QuotationNo = $"{prefix}{(countToday + 1):D3}";

        dto.TotalAmount = dto.Items.Sum(i => i.Qty * i.UnitPrice);
        dto.CreatedAt = dto.UpdatedAt = DateTime.UtcNow;
        _db.Quotations.Add(dto);
        await _db.SaveChangesAsync();
        return Ok(dto);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.AdminOrSales)]
    public async Task<ActionResult<Quotation>> Update(Guid id, [FromBody] Quotation dto)
    {
        var q = await _db.Quotations.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id);
        if (q is null) return NotFound();
        if (_me.IsSales && q.CreatedBy != _me.UserId) return Forbid();

        q.Status      = dto.Status;
        q.ValidUntil  = dto.ValidUntil;
        q.Notes       = dto.Notes;
        q.Version     = dto.Version;
        q.CustomerId  = dto.CustomerId;
        q.OpportunityId = dto.OpportunityId;

        _db.QuotationItems.RemoveRange(q.Items);
        q.Items = dto.Items.Select(i =>
        {
            i.Id = Guid.NewGuid();
            i.QuotationId = q.Id;
            return i;
        }).ToList();
        q.TotalAmount = q.Items.Sum(i => i.Qty * i.UnitPrice);
        q.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(q);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var q = await _db.Quotations.FindAsync(id);
        if (q is null) return NotFound();
        _db.Quotations.Remove(q);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
