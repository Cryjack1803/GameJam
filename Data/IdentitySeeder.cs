using Microsoft.AspNetCore.Identity;
using Tarea_01.Models;

namespace Tarea_01.Data;

public static class IdentitySeeder
{
    private static readonly string[] Roles = ["Administrador", "Cliente", "Cajero"];

    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        const string adminEmail = "admin@minimarket.com";
        const string adminPassword = "Admin123*";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Cristopher"
            };

            var createResult = await userManager.CreateAsync(adminUser, adminPassword);

            if (!createResult.Succeeded)
            {
                var message = string.Join("; ", createResult.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"No se pudo crear el usuario administrador inicial: {message}");
            }
        }

        if (!await userManager.IsInRoleAsync(adminUser, "Administrador"))
        {
            await userManager.AddToRoleAsync(adminUser, "Administrador");
        }
    }
}