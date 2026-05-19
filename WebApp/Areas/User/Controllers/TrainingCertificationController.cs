using System.Security.Claims;
using App.BLL.Contracts;
using App.DTO.v1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.Models.TrainingCertification;

namespace WebApp.Areas.User.Controllers;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
[Area("User")]
[Authorize]
public class TrainingCertificationController(
    ITrainingCertificationService certService,
    IEquipmentCategoryService categoryService) : Controller
{
    private readonly ITrainingCertificationService _certService = certService;
    private readonly IEquipmentCategoryService _categoryService = categoryService;

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task<IActionResult> Index()
    {
        var certs = await _certService.AllAsync(CurrentUserId);
        return View(certs);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var dto = await _certService.FindAsync(id, CurrentUserId);
        if (dto == null) return NotFound();
        return View(dto);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var vm = new TrainingCertFormViewModel
        {
            CategorySelectList = new SelectList(
                await _categoryService.AllAsync(), "Id", "Name"),
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TrainingCertFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.CategorySelectList = new SelectList(
                await _categoryService.AllAsync(), "Id", "Name", vm.EquipmentCategoryId);
            return View(vm);
        }

        var dto = new TrainingCertificationCreateDto
        {
            CertifiedDate        = vm.CertifiedDate,
            ExpiryDate           = vm.ExpiryDate,
            CertificateReference = vm.CertificateReference,
            EquipmentCategoryId  = vm.EquipmentCategoryId,
        };

        await _certService.AddAsync(dto, CurrentUserId);
        TempData["Success"] = "Certification submitted for review.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var dto = await _certService.FindAsync(id, CurrentUserId);
        if (dto == null) return NotFound();

        if (dto.Status != "Pending")
        {
            TempData["Error"] = "Only Pending certifications can be edited.";
            return RedirectToAction(nameof(Index));
        }

        var vm = new TrainingCertFormViewModel
        {
            Id                   = dto.Id,
            CertifiedDate        = dto.CertifiedDate,
            ExpiryDate           = dto.ExpiryDate,
            CertificateReference = dto.CertificateReference,
            EquipmentCategoryId  = dto.EquipmentCategoryId,
            CategorySelectList   = new SelectList(
                await _categoryService.AllAsync(), "Id", "Name", dto.EquipmentCategoryId),
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, TrainingCertFormViewModel vm)
    {
        if (id != vm.Id) return BadRequest();

        if (!ModelState.IsValid)
        {
            vm.CategorySelectList = new SelectList(
                await _categoryService.AllAsync(), "Id", "Name", vm.EquipmentCategoryId);
            return View(vm);
        }

        try
        {
            await _certService.UpdateAsync(
                new TrainingCertificationUserUpdateDto
                {
                    Id                   = vm.Id,
                    CertifiedDate        = vm.CertifiedDate,
                    ExpiryDate           = vm.ExpiryDate,
                    CertificateReference = vm.CertificateReference,
                },
                CurrentUserId);

            TempData["Success"] = "Certification updated.";
            return RedirectToAction(nameof(Index));
        }
        catch (UnauthorizedAccessException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Delete(Guid id)
    {
        var dto = await _certService.FindAsync(id, CurrentUserId);
        if (dto == null) return NotFound();
        return View(dto);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        await _certService.RemoveAsync(id, CurrentUserId);
        TempData["Success"] = "Certification removed.";
        return RedirectToAction(nameof(Index));
    }
}
