using System.Diagnostics.CodeAnalysis;
using App.BLL.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Areas.Admin.ViewModels.Home;

namespace WebApp.Areas.Admin.Controllers;

[ExcludeFromCodeCoverage]
[Area("Admin")]
[Authorize(Roles = "admin")]
public class HomeController : Controller
{
    private readonly IEquipmentService _equipmentService;
    private readonly IBookingService _bookingService;

    public HomeController(IEquipmentService equipmentService, IBookingService bookingService)
    {
        _equipmentService = equipmentService;
        _bookingService = bookingService;
    }

    public async Task<IActionResult> Index()
    {
        var equipment = await _equipmentService.AllAsync();
        var bookings = await _bookingService.AllAsync();

        return View(new AdminDashboardViewModel
        {
            TotalEquipment = equipment.Count(),
            TotalBookings = bookings.Count(),
            PendingBookings = bookings.Count(b => b.BookingStatus == "Pending"),
            ActiveBookings = bookings.Count(b => b.BookingStatus == "Confirmed"),
        });
    }
}