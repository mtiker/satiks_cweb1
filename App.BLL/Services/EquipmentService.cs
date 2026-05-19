using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.BLL.Contracts;
using App.BLL.Mappers;
using App.DAL.Contracts;
using App.DTO.v1;

namespace App.BLL.Services;

public class EquipmentService : IEquipmentService
{
    private readonly IAppUOW _uow;
    public EquipmentService(IAppUOW uow) => _uow = uow;

    public async Task<IEnumerable<EquipmentDto>> AllAsync()
        => (await _uow.Equipment.AllWithDetailsAsync()).Select(EquipmentBllDtoFactory.CreateDto);

    public async Task<EquipmentDto?> FindAsync(Guid id)
    {
        var e = await _uow.Equipment.FindWithDetailsAsync(id);
        return e == null ? null : EquipmentBllDtoFactory.CreateDto(e);
    }

    public async Task<EquipmentDto> AddAsync(EquipmentCreateDto dto)
    {
        var entity = EquipmentBllDtoFactory.CreateEntity(dto);
        _uow.Equipment.Add(entity);
        await _uow.SaveChangesAsync();
        var result = await _uow.Equipment.FindWithDetailsAsync(entity.Id) ?? entity;
        return EquipmentBllDtoFactory.CreateDto(result);
    }

    public async Task<EquipmentDto> UpdateAsync(EquipmentUpdateDto dto)
    {
        var entity = await _uow.Equipment.FindAsync(dto.Id)
                     ?? throw new KeyNotFoundException($"Equipment {dto.Id} not found");
        EquipmentBllDtoFactory.UpdateEntity(entity, dto);
        _uow.Equipment.Update(entity);
        await _uow.SaveChangesAsync();
        var result = await _uow.Equipment.FindWithDetailsAsync(entity.Id) ?? entity;
        return EquipmentBllDtoFactory.CreateDto(result);
    }

    public async Task<bool> RemoveAsync(Guid id)
    {
        if (!await _uow.Equipment.RemoveAsync(id)) return false;
        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<EquipmentDto>> GetAvailableAsync()
        => (await _uow.Equipment.GetAvailableEquipmentAsync()).Select(EquipmentBllDtoFactory.CreateDto);

    public async Task<IEnumerable<EquipmentDto>> GetByLaboratoryAsync(Guid laboratoryId)
        => (await _uow.Equipment.GetByLaboratoryAsync(laboratoryId)).Select(EquipmentBllDtoFactory.CreateDto);

    public async Task<IEnumerable<EquipmentDto>> GetByCategoryAsync(Guid categoryId)
        => (await _uow.Equipment.GetByCategoryAsync(categoryId)).Select(EquipmentBllDtoFactory.CreateDto);
}
