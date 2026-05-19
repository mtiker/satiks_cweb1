using App.DAL.EF;
using App.DTO.v1.Identity;
using App.Domain;
using App.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;

namespace WebApp.Tests.Integration;

/// <summary>
/// Admin area smoke tests for the /Admin/Booking MVC controller.
///
/// The Admin area requires the "admin" role on the cookie-based auth scheme.
/// A local helper promotes a freshly-registered user to admin by writing directly
/// to the shared SQLite database via a new AppDbContext — no external helper files
/// or fixtures are needed.
/// </summary>
public class AdminBookingControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public AdminBookingControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client  = factory.CreateClient(
            new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
    }

    /// <summary>
    /// Register a new user via the Identity API, then promote them to the "admin" role
    /// by writing directly to the shared SQLite database via a new AppDbContext.
    /// </summary>
    private async Task<(Guid userId, string email, string password)> RegisterAdminUserAsync()
    {
        const string email    = "admin@test.ee";
        const string password = "Admin!Pass23";

        // 1. Register using the full RegisterInfo DTO (FirstName & LastName are
        //    required fields in the SQLite AspNetUsers schema via EF Core's default mapping).
        var registerResp = await _client.PostAsJsonAsync(
            "/api/v1/identity/account/register?expiresInSeconds=999999",
            new RegisterInfo
            {
                Email     = email,
                Password  = password,
                FirstName = "Admin",
                LastName  = "Test",
            });
        registerResp.EnsureSuccessStatusCode();

        // 2. Promote to admin by writing directly to SQLite via a fresh AppDbContext.
        //    CustomWebApplicationFactory exposes _dbName (private); we can reach it via
        //    reflection to build the same in-memory connection string.
        var connStrField = typeof(CustomWebApplicationFactory)
            .GetField("_dbName",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic);
        var dbName  = (string?)connStrField?.GetValue(_factory)
                      ?? throw new InvalidOperationException("Cannot access factory _dbName.");
        var connStr = $"DataSource=file:{dbName}?mode=memory&cache=shared";

        using var seedCtx = new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connStr)
                .ConfigureWarnings(w =>
                    w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics
                        .RelationalEventId.PendingModelChangesWarning))
                .Options);

        // Ensure the role row exists in the DB (save it before querying its Id)
        if (!await seedCtx.Roles.AnyAsync(r => r.Name == "admin"))
        {
            seedCtx.Roles.Add(new AppRole { Name = "admin", NormalizedName = "ADMIN" });
            await seedCtx.SaveChangesAsync();
        }

        var adminRoleId = (await seedCtx.Roles.FirstAsync(r => r.Name == "admin")).Id;

        var user = await seedCtx.Users
            .FirstOrDefaultAsync(u => u.NormalizedEmail == email.ToUpper())
            ?? throw new InvalidOperationException($"User {email} not found after registration.");

        if (!await seedCtx.UserRoles.AnyAsync(ur => ur.UserId == user.Id))
            seedCtx.UserRoles.Add(
                new Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>
                {
                    UserId = user.Id,
                    RoleId = adminRoleId,
                });

        await seedCtx.SaveChangesAsync();

        return (user.Id, email, password);
    }

    /// <summary>
    /// Log in as the admin user via the MVC cookie form action and return a client
    /// that carries the resulting auth cookie.
    /// </summary>
    private async Task<HttpClient> LoginAsAdminAsync()
    {
        var (_, email, password) = await RegisterAdminUserAsync();

        // Use a factory-created client so that:
        //   (a) the test-server handler routes requests to the in-process server, and
        //   (b) an internal CookieContainerHandler persists cookies across requests.
        // Using a plain `new HttpClient()` here would throw InvalidOperationException
        // on a relative URL because no BaseAddress is set and it has no server handler.
        var loginClient = _factory.CreateClient(
            new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        // Step 1: GET the login form to harvest the anti-forgery cookie and token.
        using var formResp = await loginClient.GetAsync("/User");
        formResp.EnsureSuccessStatusCode();

        var html = await formResp.Content.ReadAsStringAsync();

        var token = System.Text.RegularExpressions.Regex
            .Match(html,
                @"name=""__RequestVerificationToken""[^>]*value=""([^""]+)""",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase)
            .Groups[1].Value;

        if (string.IsNullOrEmpty(token))
            throw new InvalidOperationException("Anti-forgery token not found on the login form.");

        // Step 2: POST credentials. loginClient's cookie container automatically sends
        // the anti-forgery cookie that was set during the GET above.
        using var loginResp = await loginClient.PostAsync("/User",
            new System.Net.Http.FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("Email",     email),
                new KeyValuePair<string, string>("Password",  password),
                new KeyValuePair<string, string>(
                    "__RequestVerificationToken", token),
                new KeyValuePair<string, string>("RememberMe", "false"),
            }));

        // Successful login ⇒ 302 Found redirect; validation failure ⇒ 200 on login page.
        Assert.True(
            loginResp.StatusCode is HttpStatusCode.Found or HttpStatusCode.OK,
            $"Login POST returned unexpected {loginResp.StatusCode}");

        // loginClient now holds the auth cookie — use it for the protected GET.
        return loginClient;
    }

    // ── tests ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Index_Unauthenticated_RedirectsToLogin()
    {
        // Cookie auth redirects unauthenticated MVC requests to the configured LoginPath.
        var resp = await _client.GetAsync("/Admin/Booking");
        Assert.Equal(HttpStatusCode.Found, resp.StatusCode);
        Assert.Contains(
            "/Account/Login?ReturnUrl=",
            resp.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Index_AdminRole_Returns200()
    {
        using var adminClient = await LoginAsAdminAsync();
        using var resp        = await adminClient.GetAsync("/Admin/Booking");

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }
}
