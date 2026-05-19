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

public class MaintenanceRecordService : IMaintenanceRecordService
{
    private readonly IAppUOW _uow;
    public MaintenanceRecordService(IAppUOW uow) => _uow = uow;

    public async Task<IEnumerable<MaintenanceRecordDto>> AllAsync(Guid? performedByUserId = null)
        => (await _uow.MaintenanceRecords.AllWithDetailsAsync(performedByUserId)).Select(MaintenanceRecordBllDtoFactory.CreateDto);

    public async Task<MaintenanceRecordDto?> FindAsync(Guid id, Guid? performedByUserId = null)
    {
        var e = await _uow.MaintenanceRecords.FindWithDetailsAsync(id, performedByUserId);
        return e == null ? null : MaintenanceRecordBllDtoFactory.CreateDto(e);
    }

    public async Task<MaintenanceRecordDto> AddAsync(MaintenanceRecordCreateDto dto)
    {
        var entity = MaintenanceRecordBllDtoFactory.CreateEntity(dto);
        _uow.MaintenanceRecords.Add(entity);
        await _uow.SaveChangesAsync();
        var result = await _uow.MaintenanceRecords.FindWithDetailsAsync(entity.Id) ?? entity;
        return MaintenanceRecordBllDtoFactory.CreateDto(result);
    }

    public async Task<MaintenanceRecordDto> UpdateAsync(MaintenanceRecordUpdateDto dto)
    {
        var entity = await _uow.MaintenanceRecords.FindAsync(dto.Id)
                     ?? throw new KeyNotFoundException($"MaintenanceRecord {dto.Id} not found");
        MaintenanceRecordBllDtoFactory.UpdateEntity(entity, dto);
        _uow.MaintenanceRecords.Update(entity);
        await _uow.SaveChangesAsync();
        var result = await _uow.MaintenanceRecords.FindWithDetailsAsync(entity.Id) ?? entity;
        return MaintenanceRecordBllDtoFactory.CreateDto(result);
    }

    public async Task<bool> RemoveAsync(Guid id)
    {
        if (!await _uow.MaintenanceRecords.RemoveAsync(id)) return false;
        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<MaintenanceRecordDto> AddAsync(MaintenanceRecordCreateDto dto, Guid userId)
    {
        var entity = MaintenanceRecordBllDtoFactory.CreateEntity(dto);
        entity.PerformedByUserId = userId;
        _uow.MaintenanceRecords.Add(entity);
        await _uow.SaveChangesAsync();
        var result = await _uow.MaintenanceRecords.FindWithDetailsAsync(entity.Id) ?? entity;
        return MaintenanceRecordBllDtoFactory.CreateDto(result);
    }

    public async Task<MaintenanceRecordDto> UpdateAsync(MaintenanceRecordUpdateDto dto, Guid userId)
    {
        var entity = await _uow.MaintenanceRecords.FindAsync(dto.Id)
                     ?? throw new KeyNotFoundException($"MaintenanceRecord {dto.Id} not found");
        if (entity.PerformedByUserId != userId)
            throw new UnauthorizedAccessException("Access denied.");
        MaintenanceRecordBllDtoFactory.UpdateEntity(entity, dto);
        _uow.MaintenanceRecords.Update(entity);
        await _uow.SaveChangesAsync();
        var result = await _uow.MaintenanceRecords.FindWithDetailsAsync(entity.Id) ?? entity;
        return MaintenanceRecordBllDtoFactory.CreateDto(result);
    }

    public async Task<bool> RemoveAsync(Guid id, Guid userId)
    {
        var entity = await _uow.MaintenanceRecords.FindAsync(id);
        if (entity == null || entity.PerformedByUserId != userId) return false;
        if (!await _uow.MaintenanceRecords.RemoveAsync(id)) return false;
        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<MaintenanceRecordDto>> GetByEquipmentAsync(Guid equipmentId)
        => (await _uow.MaintenanceRecords.GetByEquipmentAsync(equipmentId)).Select(MaintenanceRecordBllDtoFactory.CreateDto);

    public async Task<IEnumerable<MaintenanceRecordDto>> GetScheduledAsync()
        => (await _uow.MaintenanceRecords.GetScheduledAsync()).Select(MaintenanceRecordBllDtoFactory.CreateDto);
}
