using AquaPass.Models;
using Microsoft.EntityFrameworkCore;

namespace AquaPass.Data;

public static class DbInitializer
{
    public static async Task SeedAdminAsync(AppDbContext context, IConfiguration config)
    {
        // Переконуємося, що в базі є хоча б один адміністратор
        var adminExists = await context.Staffs.AnyAsync(u => u.Role == "Admin");

        if (adminExists)
        {
            return;
        }

        var adminEmail = config["DefaultAdmin:Email"] ?? "admin@aquapass.com";
        var adminPassword = config["DefaultAdmin:Password"] ?? "Password";
        var adminName = config["DefaultAdmin:FullName"] ?? "Головний Адміністратор";

        var adminUser = new Staff
        {
            Id = Guid.NewGuid(),
            FullName = adminName,
            Email = adminEmail.Trim().ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
            Role = "Admin",
            CreatedAt = DateTime.UtcNow
        };

        context.Staffs.Add(adminUser);
        await context.SaveChangesAsync();
    }
}