using System.Security.Claims;
using App.BLL.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.ViewModels.Technician;

namespace WebApp.Areas.Technician.Controllers;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
[Area("Technician")]
[Authorize(Roles = "technician,admin")]
public class TechnicianHomeController(
    IMaintenanceRecordService maintenanceService,
    ICalibrationRecordService calibrationService) : Controller
{
    private readonly IMaintenanceRecordService _maintenanceService = maintenanceService;
    private readonly ICalibrationRecordService _calibrationService = calibrationService;

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task<IActionResult> Index()
    {
        var allMaintenance = await _maintenanceService.AllAsync();
        var allCalibration = await _calibrationService.AllAsync();
        var overdue        = await _calibrationService.GetOverdueAsync(DateTime.UtcNow);
        var scheduled      = await _maintenanceService.GetScheduledAsync();

        var vm = new TechnicianDashboardViewModel
        {
            ScheduledMaintenanceCount = scheduled.Count(),
            OverdueCalibrationCount   = overdue.Count(),
            RecentMaintenance         = allMaintenance
                .OrderByDescending(m => m.ScheduledDate)
                .Take(5)
                .ToList(),
            RecentCalibrations        = allCalibration
                .OrderByDescending(c => c.CalibrationDate)
                .Take(5)
                .ToList(),
        };

        return View(vm);
    }
}
