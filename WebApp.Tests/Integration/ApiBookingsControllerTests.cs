using System.Net;
using System.Net.Http.Json;
using App.DTO.v1;
using Microsoft.AspNetCore.Mvc.Testing;

namespace WebApp.Tests.Integration;

public class ApiBookingsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ApiBookingsControllerTests(CustomWebApplicationFactory factory)
        => _client = factory.CreateClient(
            new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    [Fact]
    public async Task Get_Bookings_NoAuth_Returns401()
        => Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await _client.GetAsync("/api/v1/bookings")).StatusCode);

    [Fact]
    public async Task Get_BookingsAdminAll_NoAuth_Returns401()
        => Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await _client.GetAsync("/api/v1/bookings/admin/all")).StatusCode);

    [Fact]
    public async Task Post_Booking_NoAuth_Returns401()
    {
        // Guid.Empty signals this test is about auth, not DTO validity.
        var response = await _client.PostAsJsonAsync("/api/v1/bookings",
            new BookingCreateDto
            {
                EquipmentId = Guid.Empty,
                StartTime   = DateTime.UtcNow.AddDays(1),
                EndTime     = DateTime.UtcNow.AddDays(1).AddHours(2),
            });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
