using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CommerceHub.IdentityService.Configuration;
using CommerceHub.IdentityService.Domain;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Wolverine.Http;

namespace CommerceHub.IdentityService.Features.Auth.Login;

public static class LoginHandler
{
    [WolverinePost("/auth/login")]
    public static async Task<IResult> Handle(
        LoginCommand command,
        UserManager<ApplicationUser> userManager,
        IOptions<JwtConfigurationOptions> jwtOptions)
    {
        var user = await userManager.FindByEmailAsync(command.Email);
        if (user is null || !await userManager.CheckPasswordAsync(user, command.Password))
        {
            return Results.Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Invalid credentials",
                detail: "The email or password provided is incorrect.");
        }

        var roles = await userManager.GetRolesAsync(user);
        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Name, user.UserName ?? string.Empty),
            ..roles.Select(role => new Claim(ClaimTypes.Role, role))
        ];

        var jwtConfiguration = jwtOptions.Value;
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfiguration.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtConfiguration.Issuer,
            audience: jwtConfiguration.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

        var response = new LoginResponse(tokenValue, "Bearer", 3600);
        return Results.Ok(response);
    }
}
