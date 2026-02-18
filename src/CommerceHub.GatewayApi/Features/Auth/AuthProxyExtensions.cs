namespace CommerceHub.GatewayApi.Features.Auth;

/// <summary>
/// Auth proxy is configured via YARP (appsettings ReverseProxy section).
/// /api/auth/* → IdentityService /auth/* with PathRemovePrefix /api.
/// No additional registration needed; YARP handles forwarding.
/// </summary>
public static class AuthProxyExtensions
{
    // Placeholder for auth-specific YARP customizations. Config lives in appsettings ReverseProxy.
}
