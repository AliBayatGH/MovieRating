using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MovieRating.API.Extensions;
using MovieRating.API.Middleware;
using MovieRating.Application.Services;
using MovieRating.Application.Validators;
using MovieRating.Domain.Repositories;
using MovieRating.Infrastructure.Interceptors;
using MovieRating.Infrastructure.Persistence;
using MovieRating.Infrastructure.Repositories;
using Serilog;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add logging first
builder.AddLoggingConfiguration();

try
{
    // Add services to the container
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddApiVersioningConfig();

    // Add Rate Limiting
    builder.Services.AddRateLimitingConfig(builder.Configuration);

    // Add DB Context
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });
    });

    // Add Services
    builder.Services.AddScoped<QueryLoggingInterceptor>();
    builder.Services.AddScoped<IMovieRepository, MovieRepository>();
    builder.Services.AddScoped<IMovieService, MovieService>();
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

    //// Add Validators
    builder.Services.AddValidatorsFromAssemblyContaining<CreateMovieDtoValidator>();

    // Add Authentication
    if (!builder.Environment.IsEnvironment("Test"))
    {
        builder.AddJwtAuthentication();
    }

    // Add Response Caching and Health Checks
    builder.Services.AddResponseCaching();
    builder.Services.AddHealthChecks()
        .AddCheck("SQL Server", () =>
        {
            using var scope = builder.Services.BuildServiceProvider().CreateScope();
            try
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                context.Database.CanConnect();
                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(exception: ex);
            }
        });

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    else
    {
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseResponseCaching();
    app.UseRateLimiter();

    // Add security headers
    app.Use(async (context, next) =>
    {
        context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Add("X-Frame-Options", "DENY");
        context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
        await next();
    });

    // Add performance logging
    app.UseMiddleware<PerformanceLoggingMiddleware>();

    // Add exception handling
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    app.UseAuthentication();
    app.UseAuthorization();

    // Map health check endpoint
    app.MapHealthChecks("/health");
    app.MapControllers();

    // Apply migrations in development
    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Make the implicit Program class public so test projects can access it
public partial class Program { }