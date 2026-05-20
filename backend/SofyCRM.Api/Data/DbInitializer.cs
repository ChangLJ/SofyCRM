using Microsoft.EntityFrameworkCore;
using SofyCRM.Api.Entities;

namespace SofyCRM.Api.Data;

/// <summary>
/// 容器啟動時：
/// 1. 等待 PostgreSQL 可連線
/// 2. 確保預設 Admin / Sales / Service 三位使用者存在，密碼以 BCrypt 雜湊
///
/// Schema / Seed 主要靠 /docker-entrypoint-initdb.d/{01_schema,02_seed}.sql
/// 這裡只負責「補足」與「健康檢查」。
/// </summary>
public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider sp, ILogger logger)
    {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // 等待 DB 可連線
        var retry = 0;
        while (retry++ < 30)
        {
            try
            {
                if (await db.Database.CanConnectAsync()) break;
            }
            catch (Exception ex) when (retry < 30)
            {
                logger.LogWarning("Waiting for database... ({Attempt}/30): {Msg}", retry, ex.Message);
            }
            await Task.Delay(2000);
        }

        await EnsureUserAsync(db, "admin@sofycrm.local",   "系統管理員", "Admin@123",   UserRole.Admin);
        await EnsureUserAsync(db, "sales@sofycrm.local",   "王業務",     "Sales@123",   UserRole.Sales);
        await EnsureUserAsync(db, "service@sofycrm.local", "陳客服",     "Service@123", UserRole.Service);

        // Demo 簡易帳號（直接以 "Admin" 為登入帳號 / "Admin@123" 為密碼）
        await EnsureUserAsync(db, "admin",                 "Admin",      "Admin@123",   UserRole.Admin);
    }

    private static async Task EnsureUserAsync(
        AppDbContext db, string email, string name, string password, UserRole role)
    {
        var existing = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (existing is null)
        {
            db.Users.Add(new User
            {
                Email        = email,
                Name         = name,
                Role         = role,
                Status       = UserStatus.Active,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 11),
            });
            await db.SaveChangesAsync();
        }
        else
        {
            // 若 hash 已存在則保留；只有完全空白才重新雜湊（避免覆寫使用者改過的密碼）
            if (string.IsNullOrWhiteSpace(existing.PasswordHash))
            {
                existing.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 11);
                await db.SaveChangesAsync();
            }
        }
    }
}
