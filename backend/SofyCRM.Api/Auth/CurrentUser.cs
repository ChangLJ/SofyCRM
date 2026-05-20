using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SofyCRM.Api.Entities;

namespace SofyCRM.Api.Auth;

public interface ICurrentUser
{
    Guid?     UserId { get; }
    string?   Email  { get; }
    UserRole? Role   { get; }
    bool      IsAdmin   { get; }
    bool      IsSales   { get; }
    bool      IsService { get; }
}

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _http;
    public CurrentUser(IHttpContextAccessor http) => _http = http;

    private ClaimsPrincipal? User => _http.HttpContext?.User;

    public Guid? UserId =>
        Guid.TryParse(User?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    public string? Email => User?.FindFirstValue(ClaimTypes.Email);

    public UserRole? Role =>
        Enum.TryParse<UserRole>(User?.FindFirstValue(ClaimTypes.Role), out var r) ? r : null;

    public bool IsAdmin   => Role == UserRole.Admin;
    public bool IsSales   => Role == UserRole.Sales;
    public bool IsService => Role == UserRole.Service;
}
