namespace CommerceHub.IdentityService.Configuration;

public sealed class JwtConfigurationOptions
{
    public string Issuer { get; init; } = "CommerceHub.IdentityService";
    public string Audience { get; init; } = "CommerceHub.Gateway";
    public string SigningKey { get; init; } = "CHANGE_ME_DEVELOPMENT_SIGNING_KEY";
}

