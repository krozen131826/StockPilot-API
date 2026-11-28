using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Scalar.AspNetCore;
using Stockpilot.Application;
using Stockpilot.Application.Contracts;
using Stockpilot.Identity;
using Stockpilot.Identity.Models;
using Stockpilot.Persistence;

namespace Stockpilot.Api;

public static class StartupExtensions
{
    private const string AdministratorRole = "Administrator";
    private static readonly Guid ExistingAdministratorId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    public static async Task<WebApplication> ConfigureServices(this WebApplicationBuilder builder)
    {

        builder.Services.AddOpenApi();

        builder.Services.AddControllers();

        // Register IHttpContextAccessor so it can be injected (used by DbContext)
        builder.Services.AddHttpContextAccessor();


        builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Register default exception handler used by middleware
        builder.Services.AddExceptionHandler<ExceptionHandlerMiddleware>();

        builder.Services.AddIdentityServices(builder.Configuration)
               .AddPersistenceService(builder.Configuration)
               .AddApplicationServices();

        builder.Services.AddProblemDetails();
        builder.Services.AddControllers();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("Open", builder =>
                builder.AllowAnyOrigin()
                       .AllowAnyHeader()
                       .AllowAnyMethod()
            );
        });

        var app = builder.Build();

        if (!app.Environment.IsEnvironment("Testing"))
        {
            await app.ProvisionAdministratorAsync(builder.Configuration);
        }




        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            
        }
        app.MapOpenApi();
        app.MapScalarApiReference();

        // Register custom exception middleware early in the pipeline
        app.UseExceptionHandler();

        app.UseHttpsRedirection();

        app.UseCors("Open");

        app.UseAuthentication();

        app.UseAuthorization();

        app.MapControllers();


        return app;
    }

    private static async Task ProvisionAdministratorAsync(
        this WebApplication app,
        IConfiguration configuration)
    {
        using var scope = app.Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var administratorRole = await roleManager.FindByNameAsync(AdministratorRole);
        if (administratorRole is null)
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole<Guid>(AdministratorRole));
            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Administrator role provisioning failed: {string.Join("; ", roleResult.Errors.Select(error => error.Description))}");
            }
        }

        var administrator = (await userManager.GetUsersInRoleAsync(AdministratorRole))
            .FirstOrDefault();

        if (administrator is not null)
        {
            return;
        }

        administrator = await userManager.FindByIdAsync(ExistingAdministratorId.ToString());
        if (administrator is not null)
        {
            var recoveryRoleResult = await userManager.AddToRoleAsync(administrator, AdministratorRole);
            if (!recoveryRoleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Administrator role assignment failed: {string.Join("; ", recoveryRoleResult.Errors.Select(error => error.Description))}");
            }

            return;
        }

        var adminUsername = configuration["Identity:AdminUsername"];
        var adminEmail = configuration["Identity:AdminEmail"];
        var adminPassword = configuration["Identity:AdminPassword"];
        if (string.IsNullOrWhiteSpace(adminUsername) ||
            string.IsNullOrWhiteSpace(adminEmail) ||
            string.IsNullOrWhiteSpace(adminPassword))
        {
            throw new InvalidOperationException(
                "Administrator provisioning requires Identity:AdminUsername, Identity:AdminEmail, and Identity:AdminPassword configuration when no administrator account exists.");
        }

        var usernameOwner = await userManager.FindByNameAsync(adminUsername);
        if (usernameOwner is not null)
        {
            throw new InvalidOperationException(
                $"Administrator provisioning cannot continue because the configured username '{adminUsername}' is already in use.");
        }

        var emailOwner = await userManager.FindByEmailAsync(adminEmail);
        if (emailOwner is not null)
        {
            throw new InvalidOperationException(
                $"Administrator provisioning cannot continue because the configured email '{adminEmail}' is already in use.");
        }

        administrator = new ApplicationUser
        {
            Id = ExistingAdministratorId,
            UserName = adminUsername,
            Email = adminEmail,
            EmailConfirmed = true,
            IsActive = true
        };

        var createResult = await userManager.CreateAsync(administrator, adminPassword);
        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Administrator account provisioning failed: {string.Join("; ", createResult.Errors.Select(error => error.Description))}");
        }

        var administratorRoleResult = await userManager.AddToRoleAsync(administrator, AdministratorRole);
        if (!administratorRoleResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Administrator role assignment failed: {string.Join("; ", administratorRoleResult.Errors.Select(error => error.Description))}");
        }
    }

}