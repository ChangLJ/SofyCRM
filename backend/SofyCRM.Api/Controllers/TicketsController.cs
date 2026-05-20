using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SofyCRM.Api.Auth;
using SofyCRM.Api.Data;
using SofyCRM.Api.Entities;

namespace SofyCRM.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/tickets")]
public class TicketsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _me;
    public TicketsController(AppDbContext db, ICurrentUser me) { _db = db; _me = me; }

    private IQueryable<Ticket> Scoped()
    {
        var q = _db.Tickets.AsNoTracking()
            .Include(t => t.Customer).Include(t => t.AssignedUser).Include(t => t.Project).AsQueryable();
        if (_me.IsAdmin)   return q;
        if (_me.IsService) return q.Where(t => t.AssignedUserId == _me.UserId);
        if (_me.IsSales)   return q.Where(t => t.Customer!.OwnerUserId == _me.UserId);
        return q;
    }

    [HttpGet]
    public async Task<ActionResult<object>> List(
        [FromQuery] TicketStatus? status,
        [FromQuery] TicketPriority? priority,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var q = Scoped();
        if (status   is not null) q = q.Where(t => t.Status == status);
        if (priority is not null) q = q.Where(t => t.Priority == priority);
        var total = await q.CountAsync();
        var items = await q.OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new { items, total, page, pageSize });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Ticket>> Get(Guid id)
    {
        var t = await Scoped().FirstOrDefaultAsync(x => x.Id == id);
        if (t is null) return NotFound();
        return Ok(t);
    }

    [HttpPost]
    public async Task<ActionResult<Ticket>> Create([FromBody] Ticket dto)
    {
        dto.Id = Guid.NewGuid();
        dto.CreatedBy = _me.UserId;
        dto.CreatedAt = dto.UpdatedAt = DateTime.UtcNow;
        // SLA 預設規則
        dto.SlaDueAt ??= DateTime.UtcNow.Add(dto.Priority switch
        {
            TicketPriority.Critical => TimeSpan.FromHours(4),
            TicketPriority.High     => TimeSpan.FromHours(24),
            TicketPriority.Medium   => TimeSpan.FromDays(3),
            _                       => TimeSpan.FromDays(7),
        });
        _db.Tickets.Add(dto);
        await _db.SaveChangesAsync();
        return Ok(dto);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Ticket>> Update(Guid id, [FromBody] Ticket dto)
    {
        var t = await _db.Tickets.FindAsync(id);
        if (t is null) return NotFound();
        t.Title = dto.Title; t.Content = dto.Content;
        t.Priority = dto.Priority; t.Status = dto.Status;
        t.AssignedUserId = dto.AssignedUserId; t.ProjectId = dto.ProjectId;
        t.SlaDueAt = dto.SlaDueAt;
        if (dto.Status == TicketStatus.Closed && t.ClosedAt is null) t.ClosedAt = DateTime.UtcNow;
        t.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(t);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var t = await _db.Tickets.FindAsync(id);
        if (t is null) return NotFound();
        _db.Tickets.Remove(t);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
