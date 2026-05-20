using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SofyCRM.Api.Auth;
using SofyCRM.Api.Data;

namespace SofyCRM.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _me;
    public NotificationsController(AppDbContext db, ICurrentUser me) { _db = db; _me = me; }

    [HttpGet]
    public async Task<ActionResult<object>> List(
        [FromQuery] bool? unreadOnly,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var q = _db.Notifications.AsNoTracking().Where(n => n.UserId == _me.UserId);
        if (unreadOnly == true) q = q.Where(n => !n.IsRead);

        var total = await q.CountAsync();
        var items = await q.OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new { items, total, page, pageSize });
    }

    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id)
    {
        var n = await _db.Notifications.FindAsync(id);
        if (n is null || n.UserId != _me.UserId) return NotFound();
        n.IsRead = true;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead()
    {
        var list = await _db.Notifications.Where(n => n.UserId == _me.UserId && !n.IsRead).ToListAsync();
        foreach (var n in list) n.IsRead = true;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
