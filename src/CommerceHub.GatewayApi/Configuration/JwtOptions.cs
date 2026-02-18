namespace CommerceHub.GatewayApi.Configuration;

/// <summary>
/// JWT validation options. Must match IdentityService (Issuer, Audience, SigningKey).
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = "CommerceHub.IdentityService";
    public string Audience { get; init; } = "CommerceHub.Gateway";
    public string SigningKey { get; init; } = "CHANGE_ME_DEVELOPMENT_SIGNING_KEY";
}
