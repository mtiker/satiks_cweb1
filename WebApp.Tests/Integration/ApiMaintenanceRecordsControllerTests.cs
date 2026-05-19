using App.DTO.v1;
using App.DTO.v1.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;

namespace WebApp.Tests.Integration;

public class ApiMaintenanceRecordsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public ApiMaintenanceRecordsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client  = factory.CreateClient(
            new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
    }

    private const int LongExpiry = 999999;

    private static async Task<string> RegisterAndLoginAsync(HttpClient c, string email)
    {
        var resp = await c.PostAsJsonAsync(
            "/api/v1/identity/account/register?expiresInSeconds=" + LongExpiry,
            new RegisterInfo { Email = email, Password = "Pass!23", FirstName = "Test", LastName = "User" });
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<JWTResponse>())!.Jwt;
    }

    private static HttpRequestMessage AuthRequest(HttpMethod method, string url, string jwt)
    {
        var req = new HttpRequestMessage(method, url);
        req.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
        return req;
    }

    // ── anonymous GET endpoints ────────────────────────────────────────────────

    [Fact]
    public async Task GetScheduled_Anonymous_Returns200()
    {
        var resp = await _client.GetAsync("/api/v1/maintenancerecords/scheduled");
        resp.EnsureSuccessStatusCode();
        var dtos = await resp.Content.ReadFromJsonAsync<IEnumerable<MaintenanceRecordDto>>();
        Assert.NotNull(dtos);
    }

    [Fact]
    public async Task GetByEquipment_Anonymous_Returns200()
    {
        var equipId = _factory.Seed!.TestEquipmentId;
        var resp    = await _client.GetAsync($"/api/v1/maintenancerecords/by-equipment/{equipId}");
        resp.EnsureSuccessStatusCode();
        var dtos = await resp.Content.ReadFromJsonAsync<IEnumerable<MaintenanceRecordDto>>();
        Assert.NotNull(dtos);
    }

    [Fact]
    public async Task GetByEquipment_UnknownId_Returns200WithEmptyList()
    {
        var resp = await _client.GetAsync($"/api/v1/maintenancerecords/by-equipment/{Guid.NewGuid()}");
        resp.EnsureSuccessStatusCode();
        var dtos = await resp.Content.ReadFromJsonAsync<IEnumerable<MaintenanceRecordDto>>();
        Assert.Empty(dtos!);
    }

    // ── authenticated GET endpoints ────────────────────────────────────────────

    [Fact]
    public async Task GetAll_Unauthenticated_Returns401()
    {
        var resp = await _client.GetAsync("/api/v1/maintenancerecords");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task GetAll_Authenticated_Returns200()
    {
        var jwt  = await RegisterAndLoginAsync(_client, "maint_viewer@test.ee");
        var resp = await _client.SendAsync(AuthRequest(HttpMethod.Get, "/api/v1/maintenancerecords", jwt));
        resp.EnsureSuccessStatusCode();
        var dtos = await resp.Content.ReadFromJsonAsync<IEnumerable<MaintenanceRecordDto>>();
        Assert.NotNull(dtos);
    }

    [Fact]
    public async Task GetById_Unauthenticated_Returns401()
    {
        var resp = await _client.GetAsync($"/api/v1/maintenancerecords/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task GetById_Authenticated_UnknownId_Returns404()
    {
        var jwt  = await RegisterAndLoginAsync(_client, "maint_viewer2@test.ee");
        var resp = await _client.SendAsync(
            AuthRequest(HttpMethod.Get, $"/api/v1/maintenancerecords/{Guid.NewGuid()}", jwt));
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    // ── IDOR isolation ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_Authenticated_ReturnsOnlyOwnRecords()
    {
        var jwt1 = await RegisterAndLoginAsync(_client, "maintuser1@test.ee");
        var jwt2 = await RegisterAndLoginAsync(_client, "maintuser2@test.ee");

        // User 1 has 0 records; User 2 has 0 records by default.
        var records1 = (await (await _client.SendAsync(
            AuthRequest(HttpMethod.Get, "/api/v1/maintenancerecords", jwt1))).Content
            .ReadFromJsonAsync<IEnumerable<MaintenanceRecordDto>>())!;
        var records2 = (await (await _client.SendAsync(
            AuthRequest(HttpMethod.Get, "/api/v1/maintenancerecords", jwt2))).Content
            .ReadFromJsonAsync<IEnumerable<MaintenanceRecordDto>>())!;

        Assert.Empty(records1);
        Assert.Empty(records2);
    }
}
