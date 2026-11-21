using MadaFinishing.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MadaFinishing.Web.Data;

public static class ManagerSetup
{
    public static async Task RunAsync(IServiceProvider services, IConfiguration configuration)
    {
        var email = configuration["InitialManager:Email"]?.Trim();
        var password = configuration["InitialManager:Password"];
        var displayName = configuration["InitialManager:DisplayName"]?.Trim();
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(displayName) || displayName.Length > 120)
        {
            throw new InvalidOperationException("Set InitialManager:Email and InitialManager:DisplayName (up to 120 characters).");
        }

        var database = services.GetRequiredService<MadaFinishingDbContext>();
        var users = services.GetRequiredService<UserManager<StaffUser>>();
        var roles = services.GetRequiredService<RoleManager<IdentityRole>>();
        await database.Database.MigrateAsync();

        var manager = await users.FindByEmailAsync(email);
        if (manager is null)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new InvalidOperationException("Set InitialManager:Password to create the manager account.");
            }

            manager = new StaffUser { UserName = email, Email = email, DisplayName = displayName };
            RequireSuccess(await users.CreateAsync(manager, password));
        }

        if (!await roles.RoleExistsAsync(StaffUser.ManagerRole))
        {
            RequireSuccess(await roles.CreateAsync(new IdentityRole(StaffUser.ManagerRole)));
        }

        if (!await users.IsInRoleAsync(manager, StaffUser.ManagerRole))
        {
            RequireSuccess(await users.AddToRoleAsync(manager, StaffUser.ManagerRole));
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
