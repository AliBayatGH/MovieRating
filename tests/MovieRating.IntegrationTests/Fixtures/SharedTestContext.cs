using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MovieRating.Infrastructure.Persistence;
using Xunit;

namespace MovieRating.IntegrationTests.Fixtures;

public class SharedTestContext : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string _databaseName = $"TestDb_{Guid.NewGuid()}";
    private IServiceScope? _scope;
    private ApplicationDbContext? _context;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            // Add in-memory database with consistent name
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
            });

            // Configure authentication for testing
            services.AddAuthentication(defaultScheme: "TestScheme")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    "TestScheme", options => { });
        });
    }

    public ApplicationDbContext GetDbContext()
    {
        _scope ??= Services.CreateScope();
        _context ??= _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return _context;
    }

    public HttpClient CreateAuthenticatedClient()
    {
        return CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    public async Task InitializeAsync()
    {
        // Create a new scope and context for the test run
        _scope = Services.CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await _context.Database.EnsureCreatedAsync();
    }

    public new async Task DisposeAsync()
    {
        if (_context != null)
        {
            await _context.Database.EnsureDeletedAsync();
            await _context.DisposeAsync();
        }

        if (_scope != null)
        {
            _scope.Dispose();
        }

        await base.DisposeAsync();
    }
}