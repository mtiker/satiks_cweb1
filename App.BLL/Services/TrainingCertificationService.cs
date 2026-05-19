using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.BLL.Contracts;
using App.BLL.Mappers;
using App.DAL.Contracts;
using App.Domain;
using App.DTO.v1;

namespace App.BLL.Services;

public class TrainingCertificationService : ITrainingCertificationService
{
    private readonly IAppUOW _uow;
    public TrainingCertificationService(IAppUOW uow) => _uow = uow;

    public async Task<IEnumerable<TrainingCertificationDto>> AllAsync(Guid? appUserId = null)
        => (await _uow.TrainingCertifications.AllWithDetailsAsync(appUserId)).Select(TrainingCertificationBllDtoFactory.CreateDto);

    public async Task<TrainingCertificationDto?> FindAsync(Guid id, Guid? appUserId = null)
    {
        var e = await _uow.TrainingCertifications.FindWithDetailsAsync(id, appUserId);
        return e == null ? null : TrainingCertificationBllDtoFactory.CreateDto(e);
    }

    public async Task<TrainingCertificationDto> AddAsync(TrainingCertificationCreateDto dto, Guid appUserId)
    {
        var entity = TrainingCertificationBllDtoFactory.CreateEntity(dto, appUserId);
        _uow.TrainingCertifications.Add(entity);
        await _uow.SaveChangesAsync();
        var result = await _uow.TrainingCertifications.FindWithDetailsAsync(entity.Id) ?? entity;
        return TrainingCertificationBllDtoFactory.CreateDto(result);
    }

    public async Task<TrainingCertificationDto> UpdateAsync(TrainingCertificationUserUpdateDto dto, Guid appUserId)
    {
        var entity = await _uow.TrainingCertifications.FindAsync(dto.Id)
                     ?? throw new KeyNotFoundException($"TrainingCertification {dto.Id} not found");
        if (entity.AppUserId != appUserId)
            throw new UnauthorizedAccessException("Access denied.");
        if (entity.Status != TrainingCertificationStatus.Pending)
            throw new InvalidOperationException("Only pending certifications can be edited by the user.");
        TrainingCertificationBllDtoFactory.UpdateEntity(entity, dto);
        _uow.TrainingCertifications.Update(entity);
        await _uow.SaveChangesAsync();
        var result = await _uow.TrainingCertifications.FindWithDetailsAsync(entity.Id) ?? entity;
        return TrainingCertificationBllDtoFactory.CreateDto(result);
    }

    public async Task<bool> RemoveAsync(Guid id, Guid? appUserId = null)
    {
        if (!await _uow.TrainingCertifications.RemoveAsync(id, appUserId)) return false;
        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<TrainingCertificationDto> UpdateStatusAsync(TrainingCertificationAdminUpdateDto dto, Guid adminUserId)
    {
        var entity = await _uow.TrainingCertifications.FindAsync(dto.Id)
                     ?? throw new KeyNotFoundException($"TrainingCertification {dto.Id} not found");
        TrainingCertificationBllDtoFactory.UpdateEntityStatus(entity, dto, adminUserId);
        _uow.TrainingCertifications.Update(entity);
        await _uow.SaveChangesAsync();
        var result = await _uow.TrainingCertifications.FindWithDetailsAsync(entity.Id) ?? entity;
        return TrainingCertificationBllDtoFactory.CreateDto(result);
    }

    public async Task<IEnumerable<TrainingCertificationDto>> GetByStatusAsync(string status)
    {
        var parsed = Enum.Parse<TrainingCertificationStatus>(status);
        return (await _uow.TrainingCertifications.GetByStatusAsync(parsed)).Select(TrainingCertificationBllDtoFactory.CreateDto);
    }

    public async Task<bool> UserHasApprovedCertificationAsync(Guid appUserId, Guid categoryId)
        => await _uow.TrainingCertifications.UserHasApprovedCertificationAsync(appUserId, categoryId);
}
