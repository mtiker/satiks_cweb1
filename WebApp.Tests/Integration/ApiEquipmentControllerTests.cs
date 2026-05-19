using System.Net;
using System.Net.Http.Json;
using App.DTO.v1;
using Microsoft.AspNetCore.Mvc.Testing;

namespace WebApp.Tests.Integration;

public class ApiEquipmentControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ApiEquipmentControllerTests(CustomWebApplicationFactory factory)
        => _client = factory.CreateClient(
            new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    [Fact]
    public async Task Get_Equipment_ReturnsOkWithList()
    {
        var response = await _client.GetAsync("/api/v1/equipment");
        response.EnsureSuccessStatusCode();
        var items = await response.Content.ReadFromJsonAsync<IEnumerable<EquipmentDto>>();
        Assert.NotNull(items);
    }

    [Fact]
    public async Task Get_AvailableEquipment_ReturnsOk()
        => (await _client.GetAsync("/api/v1/equipment/available"))
            .EnsureSuccessStatusCode();

    [Fact]
    public async Task Get_SingleEquipment_Seeded_ReturnsOk()
    {
        var all   = await _client.GetFromJsonAsync<IEnumerable<EquipmentDto>>("/api/v1/equipment");
        var first = all?.FirstOrDefault();
        Assert.NotNull(first); // DataSeeder must have produced at least one item
        var response = await _client.GetAsync($"/api/v1/equipment/{first.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_SingleEquipment_Unknown_Returns404()
        => Assert.Equal(
            HttpStatusCode.NotFound,
            (await _client.GetAsync($"/api/v1/equipment/{Guid.NewGuid()}")).StatusCode);

    [Fact]
    public async Task Post_Equipment_WithoutAuth_Returns401()
    {
        // Auth middleware rejects before model binding — DTO content is irrelevant.
        var response = await _client.PostAsJsonAsync("/api/v1/equipment",
            new EquipmentCreateDto { Name = "x", EquipmentCondition = "Good" });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
