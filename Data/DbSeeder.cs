using CinemaDashboard.Constants;
using CinemaDashboard.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CinemaDashboard.Data;

/// <summary>
/// Runs once at startup: applies pending migrations, creates the roles and the first Admin account.
/// Admin credentials come from the "AdminUser" section of appsettings.json.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;

        var db = sp.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();

        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in AppRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var config = sp.GetRequiredService<IConfiguration>();
        var email = config["AdminUser:Email"];
        var password = config["AdminUser:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) return;

        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        if (await userManager.FindByEmailAsync(email) is not null) return;

        var admin = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = config["AdminUser:FullName"] ?? "Administrator",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(admin, password);
        if (!result.Succeeded)
            throw new InvalidOperationException("Could not create the admin user: " +
                string.Join(" ", result.Errors.Select(e => e.Description)));

        await userManager.AddToRoleAsync(admin, AppRoles.Admin);
    }
}
