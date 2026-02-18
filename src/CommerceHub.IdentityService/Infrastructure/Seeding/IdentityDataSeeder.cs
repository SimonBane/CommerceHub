using CommerceHub.IdentityService.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CommerceHub.IdentityService.Infrastructure.Seeding;

public sealed class IdentityDataSeeder(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole<Guid>> roleManager)
{
    public async Task SeedAsync(CancellationToken ct = default)
    {
        await EnsureRolesAsync(ct);
        await SeedUsersAsync(ct);
    }

    private async Task EnsureRolesAsync(CancellationToken ct)
    {
        if (await roleManager.Roles.AnyAsync(ct))
            return;
        
        foreach (var roleName in PlatformRoles.All)
        {
            if (await roleManager.RoleExistsAsync(roleName))
                continue;

            await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
        }
    }

    private async Task SeedUsersAsync(CancellationToken ct)
    {
        if (await userManager.Users.AnyAsync(ct))
            return;
        
        const string seedPassword = "SeedUser1!";

        var faker = new Bogus.Faker<ApplicationUser>()
            .RuleFor(u => u.Id, _ => Guid.CreateVersion7())
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.UserName, f => f.Internet.UserName())
            .RuleFor(u => u.EmailConfirmed, _ => true);

        var customers = faker.Generate(20);
        foreach (var user in customers)
        {
            if (await userManager.FindByEmailAsync(user.Email!) != null)
                continue;

            var result = await userManager.CreateAsync(user, seedPassword);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(user, PlatformRoles.Customer);
        }

        var supportAgents = faker.Generate(5);
        foreach (var user in supportAgents)
        {
            if (await userManager.FindByEmailAsync(user.Email!) != null)
                continue;

            var result = await userManager.CreateAsync(user, seedPassword);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(user, PlatformRoles.SupportAgent);
        }
        
        var admin = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            Email = "admin@commercehub.com",
            UserName = "admin",
            EmailConfirmed = true
        };

        if (await userManager.FindByEmailAsync(admin.Email!) == null)
        {
            var result = await userManager.CreateAsync(admin, seedPassword);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, PlatformRoles.Admin);
        }
    }
}
