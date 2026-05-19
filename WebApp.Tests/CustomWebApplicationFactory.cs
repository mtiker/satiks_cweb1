using App.DAL.EF;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebApp.Tests.Helpers;

namespace WebApp.Tests;

/// <summary>
/// Replaces the Postgres DbContext with an isolated SQLite in-memory database.
/// One factory instance = one database = one test class (via IClassFixture).
///
/// Schema creation and seeding happen inside ConfigureServices by building
/// a temporary ServiceProvider scope — NOT via builder.Configure(), which
/// would replace the entire middleware pipeline.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"test-{Guid.NewGuid():N}";
    private SqliteConnection? _keepAliveConnection;

    /// <summary>
    /// Seeded entity IDs — populated after the first CreateClient() call.
    /// Stored as instance state, never static.
    /// </summary>
    public SeedResult? Seed { get; private set; }

    private string ConnectionString =>
        $"DataSource=file:{_dbName}?mode=memory&cache=shared";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("DataInitialization:DropDatabase",    "false");
        builder.UseSetting("DataInitialization:MigrateDatabase", "false");
        builder.UseSetting("DataInitialization:SeedIdentity",    "false");
        builder.UseSetting("DataInitialization:SeedData",        "false");

        builder.ConfigureServices(services =>
        {
            // ── 1. Remove the Postgres DbContextOptions registration ──────────
            // DbContextOptions<AppDbContext> is the correct descriptor to target
            // in EF Core 10. IDbContextOptionsConfiguration<T> is unreliable.
            var optionsDescriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (optionsDescriptor != null)
                services.Remove(optionsDescriptor);

            // Also remove IDbContextOptionsConfiguration and other EF services
            // that may have been registered by AddIdentity
            var efConfigurationDescriptors = services
                .Where(d => d.ServiceType?.Name?.Contains("DbContextOptions") ?? false)
                .ToList();
            foreach (var descriptor in efConfigurationDescriptors)
            {
                services.Remove(descriptor);
            }

            // ── 2. Keep one connection open for the factory lifetime ──────────
            // SQLite in-memory databases are destroyed when the last connection
            // closes. Holding this connection open preserves the database across
            // all DbContext instances created during tests.
            _keepAliveConnection ??= new SqliteConnection(ConnectionString);
            _keepAliveConnection.Open();

            // ── 3. Register SQLite DbContext ──────────────────────────────────
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(ConnectionString);
                // EnsureCreated is used instead of Migrate because EF migrations
                // target Postgres-specific features (jsonb, etc.) and cannot run
                // on SQLite.
                options.ConfigureWarnings(w =>
                    w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics
                        .RelationalEventId.PendingModelChangesWarning));
            });

            // ── 4. Create schema and seed ─────────────────────────────────────
            // Done here inside ConfigureServices by building a temporary scope.
            // Do NOT use builder.Configure() — that replaces the entire
            // middleware pipeline, breaking routing, auth, and Swagger.
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db     = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var logger = scope.ServiceProvider
                .GetRequiredService<ILogger<CustomWebApplicationFactory>>();

            db.Database.EnsureCreated();

            try
            {
                Seed = DataSeeder.SeedData(db);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Test seeding error: {Msg}", ex.Message);
            }
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _keepAliveConnection?.Dispose();
            _keepAliveConnection = null;
        }
        base.Dispose(disposing);
    }
}
