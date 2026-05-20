using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SofyCRM.Api.Auth;
using SofyCRM.Api.Data;
using SofyCRM.Api.Entities;

namespace SofyCRM.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/projects")]
public class ProjectsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _me;
    public ProjectsController(AppDbContext db, ICurrentUser me) { _db = db; _me = me; }

    private IQueryable<Project> Scoped()
    {
        var q = _db.Projects.AsNoTracking()
            .Include(p => p.Customer).Include(p => p.PmUser).AsQueryable();
        if (_me.IsAdmin)   return q;
        if (_me.IsService) return q.Where(p => p.PmUserId == _me.UserId
                                            || _db.ProjectTasks.Any(t => t.ProjectId == p.Id && t.AssignedUserId == _me.UserId));
        if (_me.IsSales)   return q.Where(p => p.Customer!.OwnerUserId == _me.UserId);
        return q;
    }

    [HttpGet]
    public async Task<ActionResult<object>> List(
        [FromQuery] ProjectStatus? status,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var q = Scoped();
        if (status is not null) q = q.Where(p => p.Status == status);
        var total = await q.CountAsync();
        var items = await q.OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new { items, total, page, pageSize });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Project>> Get(Guid id)
    {
        var p = await Scoped()
            .Include(x => x.Tasks)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (p is null) return NotFound();
        return Ok(p);
    }

    [HttpPost]
    [Authorize(Roles = Roles.AdminOrService)]
    public async Task<ActionResult<Project>> Create([FromBody] Project dto)
    {
        dto.Id = Guid.NewGuid();
        dto.CreatedAt = dto.UpdatedAt = DateTime.UtcNow;
        _db.Projects.Add(dto);
        await _db.SaveChangesAsync();
        return Ok(dto);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.AdminOrService)]
    public async Task<ActionResult<Project>> Update(Guid id, [FromBody] Project dto)
    {
        var p = await _db.Projects.FindAsync(id);
        if (p is null) return NotFound();
        p.ProjectName = dto.ProjectName;
        p.PmUserId    = dto.PmUserId;
        p.StartDate   = dto.StartDate;
        p.EndDate     = dto.EndDate;
        p.Status      = dto.Status;
        p.Description = dto.Description;
        p.CustomerId  = dto.CustomerId;
        p.UpdatedAt   = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(p);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var p = await _db.Projects.FindAsync(id);
        if (p is null) return NotFound();
        _db.Projects.Remove(p);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // -------- Tasks --------
    [HttpGet("{id:guid}/tasks")]
    public async Task<ActionResult<List<ProjectTask>>> Tasks(Guid id) =>
        Ok(await _db.ProjectTasks.AsNoTracking().Where(t => t.ProjectId == id)
            .OrderBy(t => t.DueDate).ToListAsync());

    [HttpPost("{id:guid}/tasks")]
    [Authorize(Roles = Roles.AdminOrService)]
    public async Task<ActionResult<ProjectTask>> AddTask(Guid id, [FromBody] ProjectTask dto)
    {
        dto.Id = Guid.NewGuid();
        dto.ProjectId = id;
        dto.CreatedAt = dto.UpdatedAt = DateTime.UtcNow;
        _db.ProjectTasks.Add(dto);
        await _db.SaveChangesAsync();
        return Ok(dto);
    }

    [HttpPut("tasks/{taskId:guid}")]
    [Authorize(Roles = Roles.AdminOrService)]
    public async Task<ActionResult<ProjectTask>> UpdateTask(Guid taskId, [FromBody] ProjectTask dto)
    {
        var t = await _db.ProjectTasks.FindAsync(taskId);
        if (t is null) return NotFound();
        t.Title = dto.Title; t.Description = dto.Description;
        t.Status = dto.Status; t.AssignedUserId = dto.AssignedUserId;
        t.EstimatedHours = dto.EstimatedHours; t.ActualHours = dto.ActualHours;
        t.DueDate = dto.DueDate; t.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(t);
    }

    [HttpDelete("tasks/{taskId:guid}")]
    [Authorize(Roles = Roles.AdminOrService)]
    public async Task<IActionResult> DeleteTask(Guid taskId)
    {
        var t = await _db.ProjectTasks.FindAsync(taskId);
        if (t is null) return NotFound();
        _db.ProjectTasks.Remove(t);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
