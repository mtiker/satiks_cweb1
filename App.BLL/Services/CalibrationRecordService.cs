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

public class CalibrationRecordService : ICalibrationRecordService
{
    private readonly IAppUOW _uow;
    public CalibrationRecordService(IAppUOW uow) => _uow = uow;

    public async Task<IEnumerable<CalibrationRecordDto>> AllAsync(Guid? calibratedByUserId = null)
        => (await _uow.CalibrationRecords.AllWithDetailsAsync(calibratedByUserId)).Select(CalibrationRecordBllDtoFactory.CreateDto);

    public async Task<CalibrationRecordDto?> FindAsync(Guid id, Guid? calibratedByUserId = null)
    {
        var e = await _uow.CalibrationRecords.FindWithDetailsAsync(id, calibratedByUserId);
        return e == null ? null : CalibrationRecordBllDtoFactory.CreateDto(e);
    }

    public async Task<CalibrationRecordDto> AddAsync(CalibrationRecordCreateDto dto)
    {
        var entity = CalibrationRecordBllDtoFactory.CreateEntity(dto);
        _uow.CalibrationRecords.Add(entity);
        await _uow.SaveChangesAsync();
        var result = await _uow.CalibrationRecords.FindWithDetailsAsync(entity.Id) ?? entity;
        return CalibrationRecordBllDtoFactory.CreateDto(result);
    }

    public async Task<CalibrationRecordDto> UpdateAsync(CalibrationRecordUpdateDto dto)
    {
        var entity = await _uow.CalibrationRecords.FindAsync(dto.Id)
                     ?? throw new KeyNotFoundException($"CalibrationRecord {dto.Id} not found");
        CalibrationRecordBllDtoFactory.UpdateEntity(entity, dto);
        _uow.CalibrationRecords.Update(entity);
        await _uow.SaveChangesAsync();
        var result = await _uow.CalibrationRecords.FindWithDetailsAsync(entity.Id) ?? entity;
        return CalibrationRecordBllDtoFactory.CreateDto(result);
    }

    public async Task<bool> RemoveAsync(Guid id)
    {
        if (!await _uow.CalibrationRecords.RemoveAsync(id)) return false;
        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<CalibrationRecordDto> AddAsync(CalibrationRecordCreateDto dto, Guid userId)
    {
        var entity = CalibrationRecordBllDtoFactory.CreateEntity(dto);
        entity.CalibratedByUserId = userId;
        _uow.CalibrationRecords.Add(entity);
        await _uow.SaveChangesAsync();
        var result = await _uow.CalibrationRecords.FindWithDetailsAsync(entity.Id) ?? entity;
        return CalibrationRecordBllDtoFactory.CreateDto(result);
    }

    public async Task<CalibrationRecordDto> UpdateAsync(CalibrationRecordUpdateDto dto, Guid userId)
    {
        var entity = await _uow.CalibrationRecords.FindAsync(dto.Id)
                     ?? throw new KeyNotFoundException($"CalibrationRecord {dto.Id} not found");
        if (entity.CalibratedByUserId != userId)
            throw new UnauthorizedAccessException("Access denied.");
        CalibrationRecordBllDtoFactory.UpdateEntity(entity, dto);
        _uow.CalibrationRecords.Update(entity);
        await _uow.SaveChangesAsync();
        var result = await _uow.CalibrationRecords.FindWithDetailsAsync(entity.Id) ?? entity;
        return CalibrationRecordBllDtoFactory.CreateDto(result);
    }

    public async Task<bool> RemoveAsync(Guid id, Guid userId)
    {
        var entity = await _uow.CalibrationRecords.FindAsync(id);
        if (entity == null || entity.CalibratedByUserId != userId) return false;
        if (!await _uow.CalibrationRecords.RemoveAsync(id)) return false;
        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<CalibrationRecordDto>> GetByEquipmentAsync(Guid equipmentId)
        => (await _uow.CalibrationRecords.GetByEquipmentAsync(equipmentId)).Select(CalibrationRecordBllDtoFactory.CreateDto);

    public async Task<CalibrationRecordDto?> GetLatestByEquipmentAsync(Guid equipmentId)
    {
        var r = await _uow.CalibrationRecords.GetLatestByEquipmentAsync(equipmentId);
        return r == null ? null : CalibrationRecordBllDtoFactory.CreateDto(r);
    }

    public async Task<IEnumerable<CalibrationRecordDto>> GetOverdueAsync(DateTime? asOf = null)
        => (await _uow.CalibrationRecords.GetOverdueAsync(asOf)).Select(CalibrationRecordBllDtoFactory.CreateDto);
}
