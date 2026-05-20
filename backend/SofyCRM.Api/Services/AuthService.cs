using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SofyCRM.Api.Auth;
using SofyCRM.Api.Data;
using SofyCRM.Api.Dtos;
using SofyCRM.Api.Entities;

namespace SofyCRM.Api.Services;

public interface IAuthService
{
    Task<LoginResultDto?> LoginAsync(LoginRequestDto req, string? ip, string? ua);
    Task<LoginResultDto?> RefreshAsync(string refreshToken, string? ip, string? ua);
    Task LogoutAsync(string refreshToken);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IJwtTokenService _jwt;
    private readonly JwtOptions _opt;

    public AuthService(AppDbContext db, IJwtTokenService jwt, IOptions<JwtOptions> opt)
    {
        _db = db;
        _jwt = jwt;
        _opt = opt.Value;
    }

    public async Task<LoginResultDto?> LoginAsync(LoginRequestDto req, string? ip, string? ua)
    {
        var email = req.Email.Trim().ToLowerInvariant();
        var user  = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user is null || user.Status != UserStatus.Active) return null;
        if (!BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash)) return null;

        return await IssueAsync(user, ip, ua);
    }

    public async Task<LoginResultDto?> RefreshAsync(string refreshToken, string? ip, string? ua)
    {
        var session = await _db.Sessions.Include(s => s.User)
            .FirstOrDefaultAsync(s => s.RefreshToken == refreshToken);

        if (session is null || session.RevokedAt is not null || session.ExpiresAt < DateTime.UtcNow)
            return null;
        if (session.User is null || session.User.Status != UserStatus.Active) return null;

        session.RevokedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return await IssueAsync(session.User, ip, ua);
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var session = await _db.Sessions.FirstOrDefaultAsync(s => s.RefreshToken == refreshToken);
        if (session is null) return;
        session.RevokedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    private async Task<LoginResultDto> IssueAsync(User user, string? ip, string? ua)
    {
        var (access, expires) = _jwt.CreateAccessToken(user);
        var refresh = _jwt.GenerateRefreshToken();

        _db.Sessions.Add(new Session
        {
            UserId       = user.Id,
            RefreshToken = refresh,
            ExpiresAt    = DateTime.UtcNow.AddDays(_opt.RefreshTokenDays),
            IpAddress    = ip,
            UserAgent    = ua,
        });
        await _db.SaveChangesAsync();

        return new LoginResultDto
        {
            AccessToken       = access,
            RefreshToken      = refresh,
            ExpiresAt         = expires,
            User = new UserDto
            {
                Id    = user.Id,
                Name  = user.Name,
                Email = user.Email,
                Role  = user.Role.ToString(),
                Phone = user.Phone,
            }
        };
    }
}
