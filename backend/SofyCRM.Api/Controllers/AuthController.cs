using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SofyCRM.Api.Auth;
using SofyCRM.Api.Data;
using SofyCRM.Api.Dtos;
using SofyCRM.Api.Services;

namespace SofyCRM.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly AppDbContext _db;
    private readonly ICurrentUser _me;

    public AuthController(IAuthService auth, AppDbContext db, ICurrentUser me)
    {
        _auth = auth;
        _db = db;
        _me = me;
    }

    private string? Ip => HttpContext.Connection.RemoteIpAddress?.ToString();
    private string? Ua => Request.Headers.UserAgent.ToString();

    [HttpPost("login")]
    public async Task<ActionResult<LoginResultDto>> Login([FromBody] LoginRequestDto req)
    {
        var result = await _auth.LoginAsync(req, Ip, Ua);
        if (result is null) return Unauthorized(new { message = "Email or password incorrect" });
        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<LoginResultDto>> Refresh([FromBody] RefreshRequestDto req)
    {
        var result = await _auth.RefreshAsync(req.RefreshToken, Ip, Ua);
        if (result is null) return Unauthorized(new { message = "Invalid refresh token" });
        return Ok(result);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshRequestDto req)
    {
        await _auth.LogoutAsync(req.RefreshToken);
        return NoContent();
    }

    [Authorize]
    [HttpGet("/api/v1/users/me")]
    public async Task<ActionResult<UserDto>> Me()
    {
        if (_me.UserId is null) return Unauthorized();
        var u = await _db.Users.FindAsync(_me.UserId.Value);
        if (u is null) return Unauthorized();
        return Ok(new UserDto
        {
            Id = u.Id, Name = u.Name, Email = u.Email,
            Role = u.Role.ToString(), Phone = u.Phone,
            Status = u.Status.ToString(), CreatedAt = u.CreatedAt
        });
    }
}
