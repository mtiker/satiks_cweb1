using Microsoft.AspNetCore.Mvc.Testing;

namespace WebApp.Tests.Integration;

public class HomeControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HomeControllerTests(CustomWebApplicationFactory factory)
        => _client = factory.CreateClient(
            new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    [Fact]
    public async Task Get_AccessDenied_ReturnsSuccess()
        => (await _client.GetAsync("/Home/AccessDenied")).EnsureSuccessStatusCode();

    [Fact]
    public async Task Get_Swagger_ReturnsSuccess()
        => (await _client.GetAsync("/swagger/index.html")).EnsureSuccessStatusCode();
}
