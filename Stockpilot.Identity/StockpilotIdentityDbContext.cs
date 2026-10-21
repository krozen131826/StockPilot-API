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
        // Always configure the built-in Identity entities first.
        base.OnModelCreating(modelBuilder);

        var passwordHasher = new PasswordHasher<ApplicationUser>();

        var adminUser = new ApplicationUser
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            UserName = "Admin",
            NormalizedUserName = "ADMIN",
            Email = "rozenkimrobert@gmail.com",
            NormalizedEmail = "ROZENKIMROBERT@GMAIL.COM",
            EmailConfirmed = true,
            LockoutEnabled = false,

            SecurityStamp = "f9b2a1d4-3c8e-4f7a-9b62-8d4e6c1a5b90",
            ConcurrencyStamp = "e8c1b5a3-7d4f-4b2a-9c6e-1f3a8d5b7c20"
        };

        adminUser.PasswordHash =
            passwordHasher.HashPassword(adminUser, "ReplaceWithAStrongPassword");

        modelBuilder.Entity<ApplicationUser>().HasData(adminUser);

        var roles = new List<IdentityRole<Guid>>
    {
        new()
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Name = "Administrator",
            NormalizedName = "ADMINISTRATOR"
        },
        new()
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Name = "Staff",
            NormalizedName = "STAFF"
        }
    };

        modelBuilder.Entity<IdentityRole<Guid>>().HasData(roles);

        var userRoles = new List<IdentityUserRole<Guid>>
    {
        new()
        {
            UserId = adminUser.Id,
            RoleId = roles[0].Id
        }
    };

        modelBuilder.Entity<IdentityUserRole<Guid>>().HasData(userRoles);

        var roleClaims = new List<IdentityRoleClaim<Guid>>
    {
        new()
        {
            Id = 1,
            RoleId = roles[0].Id,
            ClaimType = "Permission",
            ClaimValue = "SuperAdmin"
        }
    };

        // Keep business entities out of the Identity model.
        modelBuilder.Entity<ApplicationUser>()
            .Ignore(u => u.CreatedPurchaseOrders);

        modelBuilder.Entity<ApplicationUser>()
            .Ignore(u => u.StockMovements);

        modelBuilder.Entity<IdentityRoleClaim<Guid>>().HasData(roleClaims);
    }

}
