namespace CommerceHub.GatewayApi.Configuration;

/// <summary>
/// YARP route and cluster identifiers. Config lives in appsettings (ReverseProxy section).
/// </summary>
public static class YarpConfiguration
{
    public const string IdentityClusterId = "identity";
    public const string IdentityRouteId = "auth-proxy";
}
