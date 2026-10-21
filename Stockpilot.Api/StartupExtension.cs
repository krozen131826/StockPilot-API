using Microsoft.AspNetCore.Diagnostics;
using Scalar.AspNetCore;
using Stockpilot.Application;
using Stockpilot.Identity;
using Stockpilot.Persistence;

namespace Stockpilot.Api;

public static class StartupExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {

        builder.Services.AddOpenApi();

        builder.Services.AddControllers();

        // Register IHttpContextAccessor so it can be injected (used by DbContext)
        builder.Services.AddHttpContextAccessor();

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




        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();

            app.MapScalarApiReference();
        }

        // Register custom exception middleware early in the pipeline
        app.UseExceptionHandler();

        app.UseHttpsRedirection();

        app.UseCors("Open");

        app.MapControllers();


        return app;
    }

}