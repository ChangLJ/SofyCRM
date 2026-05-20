using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SofyCRM.Api.Auth;
using SofyCRM.Api.Data;
using SofyCRM.Api.Dtos;
using SofyCRM.Api.Entities;

namespace SofyCRM.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/customers")]
public class CustomersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _me;
    public CustomersController(AppDbContext db, ICurrentUser me) { _db = db; _me = me; }

    // 套用 RBAC：Sales 只能看自己的，Service 看不到（需透過 tickets/projects），Admin 看全部
    private IQueryable<Customer> ScopedQuery()
    {
        var q = _db.Customers.AsNoTracking().Include(c => c.OwnerUser).AsQueryable();
        if (_me.IsAdmin) return q;
        if (_me.IsSales) return q.Where(c => c.OwnerUserId == _me.UserId);

        // Service：透過已指派的 ticket / project 反推可見客戶
        return q.Where(c =>
            _db.Tickets.Any(t => t.CustomerId == c.Id && t.AssignedUserId == _me.UserId) ||
            _db.Projects.Any(p => p.CustomerId == c.Id && p.PmUserId       == _me.UserId));
    }

    [HttpGet]
    public async Task<ActionResult<object>> List(
        [FromQuery] string? keyword,
        [FromQuery] CustomerStatus? status,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var q = ScopedQuery();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var k = keyword.Trim().ToLower();
            q = q.Where(c => c.CompanyName.ToLower().Contains(k)
                          || (c.TaxId != null && c.TaxId.Contains(k)));
        }
        if (status is not null) q = q.Where(c => c.Status == status);

        var total = await q.CountAsync();
        var items = await q.OrderByDescending(c => c.CreatedAt)
                           .Skip((page - 1) * pageSize).Take(pageSize)
                           .ToListAsync();

        return Ok(new { items, total, page, pageSize });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Customer>> Get(Guid id)
    {
        var c = await ScopedQuery()
            .Include(x => x.Contacts)
            .Include(x => x.Followups.OrderByDescending(f => f.CreatedAt))
            .Include(x => x.Opportunities)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (c is null) return NotFound();
        return Ok(c);
    }

    [HttpPost]
    [Authorize(Roles = Roles.AdminOrSales)]
    public async Task<ActionResult<Customer>> Create([FromBody] Customer dto)
    {
        dto.Id = Guid.NewGuid();
        dto.OwnerUserId ??= _me.UserId;
        dto.CreatedAt = dto.UpdatedAt = DateTime.UtcNow;
        _db.Customers.Add(dto);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = dto.Id }, dto);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.AdminOrSales)]
    public async Task<ActionResult<Customer>> Update(Guid id, [FromBody] Customer dto)
    {
        var c = await _db.Customers.FindAsync(id);
        if (c is null) return NotFound();
        if (_me.IsSales && c.OwnerUserId != _me.UserId) return Forbid();

        c.CompanyName = dto.CompanyName;
        c.TaxId   = dto.TaxId;
        c.Address = dto.Address;
        c.Industry = dto.Industry;
        c.OwnerUserId = dto.OwnerUserId;
        c.Status = dto.Status;
        c.Tags   = dto.Tags ?? Array.Empty<string>();
        c.Notes  = dto.Notes;
        c.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(c);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var c = await _db.Customers.FindAsync(id);
        if (c is null) return NotFound();
        _db.Customers.Remove(c);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ---------- Contacts ----------
    [HttpGet("{id:guid}/contacts")]
    public async Task<ActionResult<List<CustomerContact>>> Contacts(Guid id) =>
        Ok(await _db.CustomerContacts.AsNoTracking().Where(x => x.CustomerId == id).ToListAsync());

    [HttpPost("{id:guid}/contacts")]
    [Authorize(Roles = Roles.AdminOrSales)]
    public async Task<ActionResult<CustomerContact>> AddContact(Guid id, [FromBody] CustomerContact dto)
    {
        dto.Id = Guid.NewGuid();
        dto.CustomerId = id;
        _db.CustomerContacts.Add(dto);
        await _db.SaveChangesAsync();
        return Ok(dto);
    }

    [HttpDelete("contacts/{contactId:guid}")]
    [Authorize(Roles = Roles.AdminOrSales)]
    public async Task<IActionResult> DeleteContact(Guid contactId)
    {
        var c = await _db.CustomerContacts.FindAsync(contactId);
        if (c is null) return NotFound();
        _db.CustomerContacts.Remove(c);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
