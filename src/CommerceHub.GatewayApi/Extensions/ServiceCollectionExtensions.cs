using System.Text;
using System.Threading.RateLimiting;
using CommerceHub.GatewayApi.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Yarp.ReverseProxy.Swagger.Extensions;

namespace CommerceHub.GatewayApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGatewayServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<GatewayOptions>(configuration.GetSection(GatewayOptions.SectionName));

        AddJwtAuthentication(services, configuration);
        AddRateLimiting(services);
        AddReverseProxy(services, configuration);
        AddOpenApiAndScalar(services);

        return services;
    }

    private static void AddJwtAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection(JwtOptions.SectionName);
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            var signingKey = jwtSection["SigningKey"] ?? "CHANGE_ME_DEVELOPMENT_SIGNING_KEY";
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSection["Issuer"] ?? "CommerceHub.IdentityService",
                ValidAudience = jwtSection["Audience"] ?? "CommerceHub.Gateway",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey))
            };
        });

        services.AddAuthorization();
    }

    private static void AddRateLimiting(IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var path = context.Request.Path.Value ?? "";
                if (path.StartsWith("/api/auth/", StringComparison.OrdinalIgnoreCase))
                {
                    return RateLimitPartition.GetFixedWindowLimiter(
                        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 10,
                            Window = TimeSpan.FromMinutes(1)
                        });
                }
                return RateLimitPartition.GetNoLimiter("default");
            });

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });
    }

    private static void AddReverseProxy(IServiceCollection services, IConfiguration configuration)
    {
        var config = configuration.GetSection("ReverseProxy");
        var builder = services.AddReverseProxy()
            .LoadFromConfig(config)
            .AddSwagger(config);

        builder.AddServiceDiscoveryDestinationResolver();
    }

    private static void AddOpenApiAndScalar(IServiceCollection services)
    {
        services.AddOpenApi();
        services.AddEndpointsApiExplorer();
    }
}
