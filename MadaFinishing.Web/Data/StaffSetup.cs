using MadaFinishing.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MadaFinishing.Web.Data;

public static class StaffSetup
{
    public static async Task RunAsync(IServiceProvider services, IConfiguration configuration, string roleName)
    {
        var settings = configuration.GetSection($"Initial{roleName}");
        var email = settings["Email"]?.Trim();
        var password = settings["Password"];
        var displayName = settings["DisplayName"]?.Trim();
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(displayName) || displayName.Length > 120)
        {
            throw new InvalidOperationException($"Set {settings.Path}:Email and {settings.Path}:DisplayName (up to 120 characters).");
        }

        var database = services.GetRequiredService<MadaFinishingDbContext>();
        var users = services.GetRequiredService<UserManager<StaffUser>>();
        var roles = services.GetRequiredService<RoleManager<IdentityRole>>();
        await database.Database.MigrateAsync();

        var user = await users.FindByEmailAsync(email);
        if (user is null)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new InvalidOperationException($"Set {settings.Path}:Password to create the account.");
            }

            user = new StaffUser { UserName = email, Email = email, DisplayName = displayName };
            RequireSuccess(await users.CreateAsync(user, password));
        }

        var otherRole = roleName == StaffUser.ManagerRole ? StaffUser.CoordinatorRole : StaffUser.ManagerRole;
        if (await users.IsInRoleAsync(user, otherRole))
        {
            throw new InvalidOperationException($"Choose an account without the {otherRole} role.");
        }

        if (!await roles.RoleExistsAsync(roleName))
        {
            RequireSuccess(await roles.CreateAsync(new IdentityRole(roleName)));
        }

        if (!await users.IsInRoleAsync(user, roleName))
        {
            RequireSuccess(await users.AddToRoleAsync(user, roleName));
        }
    }

    private static void RequireSuccess(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join(" ", result.Errors.Select(error => error.Description)));
        }
    }
}
