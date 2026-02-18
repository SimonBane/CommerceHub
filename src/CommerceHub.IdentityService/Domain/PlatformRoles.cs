namespace CommerceHub.IdentityService.Domain;

/// <summary>
/// Role names used across the CommerceHub platform (RBAC).
/// </summary>
public static class PlatformRoles
{
    public const string Customer = "Customer";
    public const string Admin = "Admin";
    public const string SupportAgent = "SupportAgent";

    public static IReadOnlyList<string> All { get; } = [Customer, Admin, SupportAgent];
}
