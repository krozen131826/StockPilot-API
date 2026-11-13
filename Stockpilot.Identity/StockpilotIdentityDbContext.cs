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
