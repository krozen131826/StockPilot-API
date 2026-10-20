using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Stockpilot.Identity.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Stockpilot.Identity;

internal class StockpilotIdentityDbContext(DbContextOptions<StockpilotIdentityDbContext> opt, IConfiguration configuration) : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(opt)
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseNpgsql(configuration.GetConnectionString("IdentityConnection"))
            .ConfigureWarnings(warnings =>
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Create a password hasher
        var passwordHasher = new PasswordHasher<ApplicationUser>();

        // Seed Admin user
        var adminUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "Admin",
            NormalizedUserName = "ADMIN",
            Email = "rozenkimrobert@gmail.com",
            NormalizedEmail = "ROZENKIMROBERT@GMAIL.COM",
            EmailConfirmed = true,
            LockoutEnabled = false,
        };
        adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "password123");

        // Seed users
        modelBuilder.Entity<ApplicationUser>().HasData(adminUser);

        // Seed roles
        var roles = new List<IdentityRole>
        {
            new IdentityRole { Id = Guid.NewGuid().ToString(), Name = "Administrator", NormalizedName = "Administrator" },
            new IdentityRole { Id = Guid.NewGuid().ToString(), Name = "Staff", NormalizedName = "Staff" },

        };
        modelBuilder.Entity<IdentityRole>().HasData(roles);

        // Assign roles to users
        var userRoles = new List<IdentityUserRole<string>>
        {
            // Admin assigned to Admin role
            new IdentityUserRole<string> { UserId = adminUser.Id.ToString(), RoleId = roles.First(r => r.Name == "Admin").Id },

        };
        modelBuilder.Entity<IdentityUserRole<string>>().HasData(userRoles);

        // Optional: Add role claims
        var roleClaims = new List<IdentityRoleClaim<string>>
        {
            new IdentityRoleClaim<string> { Id = 1, RoleId = roles.First(r => r.Name == "Admin").Id, ClaimType = "Permission", ClaimValue = "SuperAdmin" },
        };

        modelBuilder.Entity<IdentityRoleClaim<string>>().HasData(roleClaims);

        base.OnModelCreating(modelBuilder);
    }
}
