using App.DAL.EF;
using App.DTO.v1;
using App.DTO.v1.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;

namespace WebApp.Tests.Integration;

/// <summary>
/// CalibrationRecords POST endpoints require the caller to carry the "technician" or "admin"
/// role; normal registered users don't have it.  Calibration records for these tests are
/// therefore seeded directly into the shared SQLite database through the factory's service
/// scope so that we only need authenticated (but not role-bearing) JWTs when calling GET.
/// </summary>
public class ApiCalibrationRecordsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public ApiCalibrationRecordsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client  = factory.CreateClient(
            new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        // CalibrationRecordsController POST endpoints require the "technician" or "admin"
        // role, which freshly-registered users do not have.  To make the GET isolation tests
        // meaningful we seed CalibrationRecords that are USABLE by the authenticated user by
        // writing directly to the shared SQLite database using a fresh AppDbContext.
        // CustomWebApplicationFactory stores the SQLite connection string as an instance field;
        // we access it via reflection because it is private.
        var connStrField = typeof(CustomWebApplicationFactory)
            .GetField("_dbName",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic);
        var dbName  = (string?)connStrField?.GetValue(factory)
                      ?? throw new InvalidOperationException("Cannot access _dbName.");
        var connStr = $"DataSource=file:{dbName}?mode=memory&cache=shared";

        using var seedCtx = new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connStr)
                .ConfigureWarnings(w =>
                    w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics
                        .RelationalEventId.PendingModelChangesWarning))
                .Options);

        var ownerId = factory.Seed!.TestUserId;
        var equipId = factory.Seed!.TestEquipmentId;

        seedCtx.CalibrationRecords.AddRange(new[]
        {
            new App.Domain.CalibrationRecord
            {
                EquipmentId        = equipId,
                CalibratedByUserId = ownerId,   // owned by the factory user — will be queried by user
                CalibrationDate    = DateTime.UtcNow.AddMonths(-2),
                NextCalibrationDue = DateTime.UtcNow.AddMonths(4),
                CertificateNumber  = "CAL-SEED-001",
                Passed             = true,
            },
            new App.Domain.CalibrationRecord
            {
                EquipmentId        = equipId,
                CalibratedByUserId = factory.Seed!.SecondUserId,  // belongs to somebody else
                CalibrationDate    = DateTime.UtcNow.AddMonths(-3),
                NextCalibrationDue = DateTime.UtcNow.AddMonths(3),
                CertificateNumber  = "CAL-SEED-002",
                Passed             = true,
            },
        });
        seedCtx.SaveChanges();
    }

    // ── helpers ────────────────────────────────────────────────────────────────

    private const int LongExpiry = 999999;

    private static async Task<string> RegisterAndLoginAsync(HttpClient c, string email, string password)
    {
        var registerResp = await c.PostAsJsonAsync(
            "/api/v1/identity/account/register?expiresInSeconds=" + LongExpiry,
            new App.DTO.v1.Identity.RegisterInfo
            {
                Email    = email,
                Password = password,
                FirstName = "Test",
                LastName  = "User",
            });
        registerResp.EnsureSuccessStatusCode();

        return (await registerResp.Content.ReadFromJsonAsync<App.DTO.v1.Identity.JWTResponse>())!.Jwt;
    }

    private static HttpRequestMessage AuthGet(HttpClient c, string url, string jwt)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
        return req;
    }

    private static async Task<IEnumerable<App.DTO.v1.CalibrationRecordDto>>
        GetAllAsync(HttpClient c, string jwt)
    {
        using var resp = await c.SendAsync(AuthGet(c, "/api/v1/calibrationrecords", jwt));
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<IEnumerable<App.DTO.v1.CalibrationRecordDto>>()
               ?? Array.Empty<App.DTO.v1.CalibrationRecordDto>();
    }

    // ── tests ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_Authenticated_ReturnsOnlyOwnCalibrations()
    {
        var jwt = await RegisterAndLoginAsync(_client, "calib_viewer@test.ee", "Pass!23");

        // calib_viewer@test.ee owns 0 calibration records; the 2 seeded ones belong to
        // Seed.TestUserId. If the endpoint leaks other users' records this assertion fails.
        var all = await GetAllAsync(_client, jwt);

        Assert.Empty(all);
    }

    [Fact]
    public async Task Get_OtherUsersCalibration_Returns404()
    {
        var jwt    = await RegisterAndLoginAsync(_client, "calib_viewer2@test.ee", "Pass!23");
        var other  = Guid.NewGuid();           // not our user

        // Unknown id → always 404 (not found period, ownership is irrelevant here)
        var resp = await _client.SendAsync(
            AuthGet(_client, $"/api/v1/calibrationrecords/{Guid.NewGuid()}", jwt));
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task GetAll_Unauthenticated_Returns401()
        => Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await _client.GetAsync("/api/v1/calibrationrecords")).StatusCode);
}
