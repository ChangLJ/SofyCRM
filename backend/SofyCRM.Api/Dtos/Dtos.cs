using System.ComponentModel.DataAnnotations;
using SofyCRM.Api.Entities;

namespace SofyCRM.Api.Dtos;

// ============ Auth ============
public class LoginRequestDto
{
    /// <summary>登入帳號（可為 Email 或純字串如 "Admin"）</summary>
    [Required] public string Email    { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
}

public class RefreshRequestDto
{
    [Required] public string RefreshToken { get; set; } = string.Empty;
}

public class LoginResultDto
{
    public string AccessToken  { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt  { get; set; }
    public UserDto User        { get; set; } = new();
}

public class UserDto
{
    public Guid    Id    { get; set; }
    public string  Name  { get; set; } = string.Empty;
    public string  Email { get; set; } = string.Empty;
    public string  Role  { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string  Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; }
}

public class CreateUserDto
{
    [Required, MaxLength(100)] public string Name     { get; set; } = string.Empty;
    [Required, EmailAddress]   public string Email    { get; set; } = string.Empty;
    [Required, MinLength(6)]   public string Password { get; set; } = string.Empty;
    [Required] public UserRole Role { get; set; }
    public string? Phone { get; set; }
}

public class UpdateUserDto
{
    [MaxLength(100)] public string? Name   { get; set; }
    public string?   Phone   { get; set; }
    public UserRole? Role    { get; set; }
    public UserStatus? Status { get; set; }
}

public class ChangePasswordDto
{
    [Required, MinLength(6)] public string NewPassword { get; set; } = string.Empty;
}

// ============ Pagination ============
public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int Total { get; set; }
    public int Page  { get; set; }
    public int PageSize { get; set; }
}
