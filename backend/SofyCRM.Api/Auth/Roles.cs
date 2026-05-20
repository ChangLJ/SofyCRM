namespace SofyCRM.Api.Auth;

public static class Roles
{
    public const string Admin   = "Admin";
    public const string Sales   = "Sales";
    public const string Service = "Service";

    public const string AdminOrSales   = "Admin,Sales";
    public const string AdminOrService = "Admin,Service";
    public const string Any            = "Admin,Sales,Service";
}
