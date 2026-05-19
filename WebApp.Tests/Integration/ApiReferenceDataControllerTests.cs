using App.DTO.v1;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;

namespace WebApp.Tests.Integration;

/// <summary>
/// Integration tests for the read-only (anonymous) GET endpoints on the reference-data
/// API controllers: Departments, EquipmentCategories, Laboratories, Locations, Manufacturers.
/// Each controller exposes at least a GET-all and GET-by-id endpoint that can be hit
/// without authentication.
/// </summary>
public class ApiReferenceDataControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public ApiReferenceDataControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client  = factory.CreateClient(
            new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
    }

    // ── Departments ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Departments_GetAll_Returns200WithList()
    {
        var resp = await _client.GetAsync("/api/v1/departments");
        resp.EnsureSuccessStatusCode();

        var dtos = await resp.Content.ReadFromJsonAsync<IEnumerable<DepartmentDto>>();
        Assert.NotNull(dtos);
    }

    [Fact]
    public async Task Departments_GetById_NotFound_Returns404()
    {
        var resp = await _client.GetAsync($"/api/v1/departments/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task Departments_GetById_Found_Returns200()
    {
        var allResp = await _client.GetAsync("/api/v1/departments");
        allResp.EnsureSuccessStatusCode();
        var all = (await allResp.Content.ReadFromJsonAsync<IEnumerable<DepartmentDto>>())!.ToList();

        if (!all.Any()) return; // no seed data in this factory instance

        var resp = await _client.GetAsync($"/api/v1/departments/{all[0].Id}");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    // ── EquipmentCategories ───────────────────────────────────────────────────

    [Fact]
    public async Task EquipmentCategories_GetAll_Returns200WithList()
    {
        var resp = await _client.GetAsync("/api/v1/equipmentcategories");
        resp.EnsureSuccessStatusCode();

        var dtos = await resp.Content.ReadFromJsonAsync<IEnumerable<EquipmentCategoryDto>>();
        Assert.NotNull(dtos);
        Assert.NotEmpty(dtos!); // DataSeeder seeds one category
    }

    [Fact]
    public async Task EquipmentCategories_GetById_Found_Returns200()
    {
        var catId = _factory.Seed!.TestEquipmentCategoryId;

        var resp = await _client.GetAsync($"/api/v1/equipmentcategories/{catId}");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task EquipmentCategories_GetById_NotFound_Returns404()
    {
        var resp = await _client.GetAsync($"/api/v1/equipmentcategories/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task EquipmentCategories_GetRequiringTraining_Returns200()
    {
        var resp = await _client.GetAsync("/api/v1/equipmentcategories/requiring-training");
        resp.EnsureSuccessStatusCode();
        var dtos = await resp.Content.ReadFromJsonAsync<IEnumerable<EquipmentCategoryDto>>();
        Assert.NotNull(dtos);
    }

    // ── Laboratories ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Laboratories_GetAll_Returns200WithList()
    {
        var resp = await _client.GetAsync("/api/v1/laboratories");
        resp.EnsureSuccessStatusCode();
        var dtos = await resp.Content.ReadFromJsonAsync<IEnumerable<LaboratoryDto>>();
        Assert.NotNull(dtos);
        Assert.NotEmpty(dtos!);
    }

    [Fact]
    public async Task Laboratories_GetById_NotFound_Returns404()
    {
        var resp = await _client.GetAsync($"/api/v1/laboratories/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task Laboratories_GetById_Found_Returns200()
    {
        var all = (await _client.GetFromJsonAsync<IEnumerable<LaboratoryDto>>("/api/v1/laboratories"))!.ToList();
        if (!all.Any()) return;

        var resp = await _client.GetAsync($"/api/v1/laboratories/{all[0].Id}");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    // ── Locations ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Locations_GetAll_Returns200WithList()
    {
        var resp = await _client.GetAsync("/api/v1/locations");
        resp.EnsureSuccessStatusCode();
        var dtos = await resp.Content.ReadFromJsonAsync<IEnumerable<LocationDto>>();
        Assert.NotNull(dtos);
        Assert.NotEmpty(dtos!);
    }

    [Fact]
    public async Task Locations_GetById_NotFound_Returns404()
    {
        var resp = await _client.GetAsync($"/api/v1/locations/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task Locations_GetById_Found_Returns200()
    {
        var all = (await _client.GetFromJsonAsync<IEnumerable<LocationDto>>("/api/v1/locations"))!.ToList();
        if (!all.Any()) return;

        var resp = await _client.GetAsync($"/api/v1/locations/{all[0].Id}");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    // ── Manufacturers ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Manufacturers_GetAll_Returns200WithList()
    {
        var resp = await _client.GetAsync("/api/v1/manufacturers");
        resp.EnsureSuccessStatusCode();
        var dtos = await resp.Content.ReadFromJsonAsync<IEnumerable<ManufacturerDto>>();
        Assert.NotNull(dtos);
        Assert.NotEmpty(dtos!);
    }

    [Fact]
    public async Task Manufacturers_GetById_NotFound_Returns404()
    {
        var resp = await _client.GetAsync($"/api/v1/manufacturers/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task Manufacturers_GetById_Found_Returns200()
    {
        var all = (await _client.GetFromJsonAsync<IEnumerable<ManufacturerDto>>("/api/v1/manufacturers"))!.ToList();
        if (!all.Any()) return;

        var resp = await _client.GetAsync($"/api/v1/manufacturers/{all[0].Id}");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }
}
