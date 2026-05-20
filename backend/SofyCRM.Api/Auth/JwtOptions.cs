namespace SofyCRM.Api.Auth;

public class JwtOptions
{
    public string Issuer   { get; set; } = "SofyCRM";
    public string Audience { get; set; } = "SofyCRM.Client";
    public string Secret   { get; set; } = "ChangeMe-SofyCRM-Super-Secret-Key";
    public int    AccessTokenMinutes { get; set; } = 60;
    public int    RefreshTokenDays   { get; set; } = 14;
}
