namespace CommerceHub.GatewayApi.Configuration;

/// <summary>
/// Gateway-wide options (backend base URLs, service names, etc.).
/// </summary>
public sealed class GatewayOptions
{
    public const string SectionName = "Gateway";

    /// <summary>
    /// Identity service base URL or service-discovery name (e.g. http://commercehub-identityservice).
    /// Used by YARP for the identity cluster when not using service discovery URLs.
    /// </summary>
    public string IdentityServiceBaseUrl { get; init; } = "http://commercehub-identityservice";
}
