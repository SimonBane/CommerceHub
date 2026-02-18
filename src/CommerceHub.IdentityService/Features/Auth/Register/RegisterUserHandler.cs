using CommerceHub.IdentityService.Domain;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Wolverine.Http;

namespace CommerceHub.IdentityService.Features.Auth.Register;

public static class RegisterUserHandler
{
    [WolverinePost("/auth/register")]
    public static async Task<IResult> Handle(
        RegisterUserCommand command,
        UserManager<ApplicationUser> userManager)
    {
        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            UserName = command.Email,
            Email = command.Email
        };

        var result = await userManager.CreateAsync(user, command.Password);
        if (!result.Succeeded)
        {
            return MapIdentityValidationProblem(result);
        }

        var addToRoleResult = await userManager.AddToRoleAsync(user, PlatformRoles.Customer);
        if (!addToRoleResult.Succeeded)
        {
            return MapIdentityValidationProblem(addToRoleResult);
        }

        var response = new RegisterUserResponse(user.Id, user.Email ?? string.Empty);
        return Results.Created($"/users/{user.Id}", response);
    }
    
    private static IResult MapIdentityValidationProblem(IdentityResult result)
    {
        var errors = result.Errors
            .GroupBy(e => e.Code)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.Description).ToArray());

        return Results.ValidationProblem(errors);
    }
}

