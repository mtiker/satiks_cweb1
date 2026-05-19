using App.DTO.v1;
using App.DTO.v1.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;

namespace WebApp.Tests.Integration;

public class ApiTrainingCertificationsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public ApiTrainingCertificationsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client  = factory.CreateClient(
            new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
    }

    // ── helpers ────────────────────────────────────────────────────────────────

    private const int LongExpiry = 999999;

    private static async Task<string> RegisterAndLoginAsync(HttpClient c, string email, string password)
    {
        var registerResp = await c.PostAsJsonAsync(
            "/api/v1/identity/account/register?expiresInSeconds=" + LongExpiry,
            new RegisterInfo
            {
                Email    = email,
                Password = password,
                FirstName = "First",
                LastName  = "Last",
            });
        registerResp.EnsureSuccessStatusCode();

        return (await registerResp.Content.ReadFromJsonAsync<JWTResponse>())!.Jwt;
    }

    private static HttpRequestMessage AuthGet(HttpClient c, string url, string jwt)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
        return req;
    }

    private static HttpRequestMessage AuthDelete(HttpClient c, string url, string jwt)
    {
        var req = new HttpRequestMessage(HttpMethod.Delete, url);
        req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
        return req;
    }

    private static async Task<IEnumerable<TrainingCertificationDto>> GetAllCertificationsAsync(HttpClient c, string jwt)
    {
        var req = AuthGet(c, "/api/v1/trainingcertifications", jwt);
        using var resp = await c.SendAsync(req);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<IEnumerable<TrainingCertificationDto>>() ?? Array.Empty<TrainingCertificationDto>();
    }

    private async Task<TrainingCertificationDto> CreateCertificationAsync(HttpClient c, string jwt)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, "/api/v1/trainingcertifications")
        {
            Content = JsonContent.Create(new TrainingCertificationCreateDto
            {
                CertifiedDate       = DateTime.UtcNow.Date,
                EquipmentCategoryId = _factory.Seed!.TestEquipmentCategoryId,
            }),
        };
        req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
        using var resp = await c.SendAsync(req);
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<TrainingCertificationDto>())!;
    }

    // ── tests ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_Authenticated_ReturnsOnlyOwnRecords()
    {
        var jwt1 = await RegisterAndLoginAsync(_client, "user1@test.ee", "Pass!23");
        var jwt2 = await RegisterAndLoginAsync(_client, "user2@test.ee", "Pass!23");

        var certId1a = (await CreateCertificationAsync(_client, jwt1)).Id;
        var certId1b = (await CreateCertificationAsync(_client, jwt1)).Id;
        _ = await CreateCertificationAsync(_client, jwt2);

        var ownRecords = await GetAllCertificationsAsync(_client, jwt1);
        Assert.Equal(2, ownRecords.Count());
    }

    [Fact]
    public async Task Get_OtherUsersRecord_Returns404()
    {
        var jwt1 = await RegisterAndLoginAsync(_client, "certowner@test.ee", "Pass!23");
        var jwt2 = await RegisterAndLoginAsync(_client, "otheruser@test.ee",  "Pass!23");

        var created = await CreateCertificationAsync(_client, jwt1);
        var recordId = created.Id;

        var resp = await _client.SendAsync(AuthGet(_client, $"/api/v1/trainingcertifications/{recordId}", jwt2));
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task Delete_OtherUsersRecord_Returns404()
    {
        var jwt1 = await RegisterAndLoginAsync(_client, "delowner@test.ee", "Pass!23");
        var jwt2 = await RegisterAndLoginAsync(_client, "otherdeleter@test.ee", "Pass!23");

        var created = await CreateCertificationAsync(_client, jwt1);
        var recordId = created.Id;

        var resp = await _client.SendAsync(
            AuthDelete(_client, $"/api/v1/trainingcertifications/{recordId}", jwt2));
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task Get_Unauthenticated_Returns401()
        => Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await _client.GetAsync("/api/v1/trainingcertifications")).StatusCode);
}
