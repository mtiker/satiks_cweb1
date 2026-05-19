using App.BLL.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Areas.User.Controllers;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
[Area("User")]
public class EquipmentController(IEquipmentService equipmentService) : Controller
{
    private readonly IEquipmentService _equipmentService = equipmentService;

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var items = await _equipmentService.AllAsync();
        return View(items);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(Guid id)
    {
        var item = await _equipmentService.FindAsync(id);
        if (item == null) return NotFound();
        return View(item);
    }
}
