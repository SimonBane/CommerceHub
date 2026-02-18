using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CommerceHub.GatewayApi.Features.Me;

public static class MeEndpoints
{
    public static IEndpointRouteBuilder MapMeEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/api/me", GetMe)
            .RequireAuthorization()
            .WithName("GetMe")
            .WithTags("Me")
            .Produces(StatusCodes.Status401Unauthorized);

        return builder;
    }

    private static IResult GetMe(ClaimsPrincipal claimsPrincipal)
    {
        var claims = claimsPrincipal.Claims.ToDictionary(claim => claim.Type, claim => claim.Value);
        return Results.Ok(claims);
    }
}
