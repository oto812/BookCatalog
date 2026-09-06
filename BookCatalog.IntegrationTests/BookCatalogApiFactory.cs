using BookCatalog.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace BookCatalog.IntegrationTests;

// Starts a throwaway PostgreSQL container, then boots the real API against it.
// Created once per test class, disposed when the class finishes.
public class BookCatalogApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:17")
        .Build();

    // Runs once, before any test in the class.
    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        // Asking for Services builds the app, which runs ConfigureWebHost below.
        // The container must already be running, or there is no connection string to give it.
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BookCatalogDbContext>();
        await db.Database.MigrateAsync();
    }

    // Called by WebApplicationFactory while it builds the app.
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Not "Development", so the app does not run its own startup migration.
        // This class owns the schema.
        builder.UseEnvironment("Testing");

        // Point the app at the container instead of appsettings.json.
        builder.ConfigureAppConfiguration((_, config) =>
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:BookCatalog"] = _postgres.GetConnectionString()
            }));
        
    }
    

    // Empties every table. Each test calls this before it starts.
    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BookCatalogDbContext>();

        await db.Database.ExecuteSqlRawAsync(
            """TRUNCATE "Loans", "Books", "Users", "Authors" CASCADE;""");
    }

    // "new" because WebApplicationFactory already has a DisposeAsync.
    // Ours also stops the container.
    public new async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }
}
