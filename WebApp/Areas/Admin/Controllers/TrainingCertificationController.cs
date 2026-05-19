using System.Security.Claims;
using App.BLL.Contracts;
using App.DTO.v1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.Areas.Admin.ViewModels.TrainingCertification;
using App.Domain.Identity;
using Microsoft.AspNetCore.Identity;

namespace WebApp.Areas.Admin.Controllers;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
[Area("Admin")]
[Authorize(Roles = "admin")]
public class TrainingCertificationController(
    ITrainingCertificationService trainingService,
    IEquipmentCategoryService categoryService,
    UserManager<AppUser> userManager) : Controller
{
    private readonly ITrainingCertificationService _trainingService = trainingService;
    private readonly IEquipmentCategoryService _categoryService = categoryService;
    private readonly UserManager<AppUser> _userManager = userManager;

    private static readonly string[] ValidStatuses = ["Approved", "Rejected", "Revoked"];

    public async Task<IActionResult> Index(
        string? search,
        string? status,
        string? userId,
        string? equipmentCategoryId,
        int page = 1,
        int pageSize = 20)
    {
        var all = await _trainingService.AllAsync();

        if (!string.IsNullOrWhiteSpace(search))
            all = all.Where(c =>
                (c.AppUserFullName != null && c.AppUserFullName.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                (c.EquipmentCategoryName != null && c.EquipmentCategoryName.Contains(search, StringComparison.OrdinalIgnoreCase)));

        if (!string.IsNullOrWhiteSpace(status))
            all = all.Where(c => c.Status == status);

        if (!string.IsNullOrWhiteSpace(userId))
            all = all.Where(c => c.AppUserId.ToString() == userId);

        if (!string.IsNullOrWhiteSpace(equipmentCategoryId))
            all = all.Where(c => c.EquipmentCategoryId.ToString() == equipmentCategoryId);

        var ordered = all.OrderByDescending(c => c.CertifiedDate).ToList();
        var totalCount = ordered.Count;
        var items = ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CertificationListItem
            {
                Id                    = c.Id,
                Status                = c.Status,
                AppUserId             = c.AppUserId,
                AppUserFullName       = c.AppUserFullName ?? string.Empty,
                EquipmentCategoryId   = c.EquipmentCategoryId,
                EquipmentCategoryName = c.EquipmentCategoryName ?? string.Empty,
                CertifiedDate         = c.CertifiedDate,
                ExpiryDate            = c.ExpiryDate,
                CertificateReference  = c.CertificateReference,
            })
            .ToList();

        var vm = new TrainingCertIndexViewModel
        {
            Items       = items,
            TotalCount  = totalCount,
            Page        = page,
            PageSize    = pageSize,
            FilterState = TrainingCertIndexViewModel.BuildFilters(
                TrainingCertStatusOptions.AsSelectList(),
                _userManager.Users.Select(u => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = u.Id.ToString(), Text = u.Email }),
                (await _categoryService.AllAsync()).Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = c.Id.ToString(), Text = c.Name }),
                status, userId, equipmentCategoryId, search)
        };

        return View(vm);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var dto = await _trainingService.FindAsync(id);
        if (dto == null) return NotFound();
        return View(dto);
    }

    public async Task<IActionResult> UpdateStatus(Guid id)
    {
        var dto = await _trainingService.FindAsync(id);
        if (dto == null) return NotFound();

        var vm = new TrainingCertUpdateStatusViewModel
        {
            Id = dto.Id,
            Status = dto.Status,
            ValidationComment = dto.ValidationComment,
            StatusSelectList = new SelectList(ValidStatuses, dto.Status),
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(TrainingCertUpdateStatusViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.StatusSelectList = new SelectList(ValidStatuses, vm.Status);
            return View(vm);
        }

        var adminUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            await _trainingService.UpdateStatusAsync(
                new TrainingCertificationAdminUpdateDto
                {
                    Id = vm.Id,
                    Status = vm.Status,
                    ValidationComment = vm.ValidationComment,
                },
                adminUserId);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            vm.StatusSelectList = new SelectList(ValidStatuses, vm.Status);
            return View(vm);
        }

        TempData["Success"] = "Certification status updated.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        var dto = await _trainingService.FindAsync(id);
        if (dto == null) return NotFound();
        return View(dto);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        try { await _trainingService.RemoveAsync(id); }
        catch (KeyNotFoundException) { return NotFound(); }

        TempData["Success"] = "Certification deleted.";
        return RedirectToAction(nameof(Index));
    }
[HttpGet]
    public async Task<IActionResult> Create()
    {
        var vm = new TrainingCertCreateEditViewModel
        {
            CategorySelectList = new SelectList(
                await _categoryService.AllAsync(), "Id", "Name"),
            UserSelectList = new SelectList(
                _userManager.Users.ToList(), "Id", "Email"),
            CertifiedDate = DateTime.Today,
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TrainingCertCreateEditViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.CategorySelectList = new SelectList(
                await _categoryService.AllAsync(), "Id", "Name", vm.EquipmentCategoryId);
            vm.UserSelectList = new SelectList(
                _userManager.Users.ToList(), "Id", "Email", vm.AppUserId);
            return View(vm);
        }

        var dto = new App.DTO.v1.TrainingCertificationCreateDto
        {
            CertifiedDate = vm.CertifiedDate,
            ExpiryDate = vm.ExpiryDate,
            CertificateReference = vm.CertificateReference,
            EquipmentCategoryId = vm.EquipmentCategoryId,
        };
        await _trainingService.AddAsync(dto, vm.AppUserId);
        TempData["Success"] = "Certification created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var dto = await _trainingService.FindAsync(id);
        if (dto == null) return NotFound();

        var vm = new TrainingCertCreateEditViewModel
        {
            Id = dto.Id,
            AppUserId = dto.AppUserId,
            EquipmentCategoryId = dto.EquipmentCategoryId,
            CertifiedDate = dto.CertifiedDate,
            ExpiryDate = dto.ExpiryDate,
            CertificateReference = dto.CertificateReference,
            CategorySelectList = new SelectList(
                await _categoryService.AllAsync(), "Id", "Name", dto.EquipmentCategoryId),
            UserSelectList = new SelectList(
                _userManager.Users.ToList(), "Id", "Email", dto.AppUserId),
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(TrainingCertCreateEditViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.CategorySelectList = new SelectList(
                await _categoryService.AllAsync(), "Id", "Name", vm.EquipmentCategoryId);
            vm.UserSelectList = new SelectList(
                _userManager.Users.ToList(), "Id", "Email", vm.AppUserId);
            return View(vm);
        }

        var dto = new App.DTO.v1.TrainingCertificationUserUpdateDto
        {
            Id = vm.Id,
            CertifiedDate = vm.CertifiedDate,
            ExpiryDate = vm.ExpiryDate,
            CertificateReference = vm.CertificateReference,
        };
        await _trainingService.UpdateAsync(dto, vm.AppUserId);
        TempData["Success"] = "Certification updated.";
        return RedirectToAction(nameof(Index));
    }
}