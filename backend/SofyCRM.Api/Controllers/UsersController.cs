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
[Route("api/v1/users")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _me;
    public UsersController(AppDbContext db, ICurrentUser me) { _db = db; _me = me; }

    // 任何已登入者皆可讀取（給負責人 / 經辦選單使用），但只回傳必要欄位
    [HttpGet]
    public async Task<ActionResult<PagedResult<UserDto>>> List(
        [FromQuery] string? keyword,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 100)
    {
        var q = _db.Users.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var k = keyword.Trim().ToLower();
            q = q.Where(u => u.Email.ToLower().Contains(k) || u.Name.ToLower().Contains(k));
        }
        var total = await q.CountAsync();
        var items = await q.OrderByDescending(u => u.CreatedAt)
                           .Skip((page - 1) * pageSize).Take(pageSize)
                           .Select(u => new UserDto
                           {
                               Id = u.Id, Name = u.Name, Email = u.Email,
                               Role = u.Role.ToString(), Phone = u.Phone,
                               Status = u.Status.ToString(), CreatedAt = u.CreatedAt
                           })
                           .ToListAsync();
        return Ok(new PagedResult<UserDto> { Items = items, Total = total, Page = page, PageSize = pageSize });
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(u => u.Email == email))
            return Conflict(new { message = "Email already exists" });

        var u = new User
        {
            Email = email, Name = dto.Name, Phone = dto.Phone, Role = dto.Role,
            Status = UserStatus.Active,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, workFactor: 11),
        };
        _db.Users.Add(u);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = u.Id }, ToDto(u));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<UserDto>> GetById(Guid id)
    {
        var u = await _db.Users.FindAsync(id);
        if (u is null) return NotFound();
        return Ok(ToDto(u));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<UserDto>> Update(Guid id, [FromBody] UpdateUserDto dto)
    {
        var u = await _db.Users.FindAsync(id);
        if (u is null) return NotFound();
        if (dto.Name   is not null) u.Name = dto.Name;
        if (dto.Phone  is not null) u.Phone = dto.Phone;
        if (dto.Role   is not null) u.Role = dto.Role.Value;
        if (dto.Status is not null) u.Status = dto.Status.Value;
        await _db.SaveChangesAsync();
        return Ok(ToDto(u));
    }

    [HttpPost("{id:guid}/reset-password")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> ResetPassword(Guid id, [FromBody] ChangePasswordDto dto)
    {
        var u = await _db.Users.FindAsync(id);
        if (u is null) return NotFound();
        u.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword, workFactor: 11);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Disable(Guid id)
    {
        var u = await _db.Users.FindAsync(id);
        if (u is null) return NotFound();
        u.Status = UserStatus.Disabled;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static UserDto ToDto(User u) => new()
    {
        Id = u.Id, Name = u.Name, Email = u.Email,
        Role = u.Role.ToString(), Phone = u.Phone,
        Status = u.Status.ToString(), CreatedAt = u.CreatedAt
    };
}
