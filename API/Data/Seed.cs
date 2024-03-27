using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using static API.Helpers.Constants;

namespace API.Data;

public class Seed
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public static async Task ClearConnections(DataContext context)
    {
        context.Connections.RemoveRange(context.Connections);

        await context.SaveChangesAsync();
    }

    public static async Task SeedUsers(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
    {
        if (await userManager.Users.AnyAsync())
            return;

        var userData = await File.ReadAllTextAsync("Data/UserSeedData.json");
        var users = JsonSerializer.Deserialize<List<AppUser>>(userData, JsonOptions);

        var roles = new List<AppRole>
        {
            new() { Name = Roles.Member },
            new() { Name = Roles.Admin },
            new() { Name = Roles.Moderator },
        };

        foreach (var role in roles)
            await roleManager.CreateAsync(role);

        foreach (var user in users!)
        {
            user.UserName = user.UserName!.ToLower();
            user.Created = DateTime.SpecifyKind(user.Created, DateTimeKind.Utc);
            user.LastActive = DateTime.SpecifyKind(user.LastActive, DateTimeKind.Utc);

            await userManager.CreateAsync(user, "Pa$$w0rd");
            await userManager.AddToRoleAsync(user, Roles.Member);
        }

        var admin = new AppUser
        {
            UserName = "admin",
            KnownAs = "",
            Gender = "",
            City = "",
            Country = "",
        };

        await userManager.CreateAsync(admin, "admin");
        await userManager.AddToRolesAsync(admin, [Roles.Admin, Roles.Moderator]);
    }
}
