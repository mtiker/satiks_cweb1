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

public class LaboratoryService : ILaboratoryService
{
    private readonly IAppUOW _uow;
    public LaboratoryService(IAppUOW uow) => _uow = uow;

    public async Task<IEnumerable<LaboratoryDto>> AllAsync()
        => (await _uow.Laboratories.AllWithDetailsAsync()).Select(LaboratoryBllDtoFactory.CreateDto);

    public async Task<LaboratoryDto?> FindAsync(Guid id)
    {
        var e = await _uow.Laboratories.FindWithDetailsAsync(id);
        return e == null ? null : LaboratoryBllDtoFactory.CreateDto(e);
    }

    public async Task<LaboratoryDto> AddAsync(LaboratoryCreateDto dto)
    {
        var entity = LaboratoryBllDtoFactory.CreateEntity(dto);
        _uow.Laboratories.Add(entity);
        await _uow.SaveChangesAsync();
        var result = await _uow.Laboratories.FindWithDetailsAsync(entity.Id) ?? entity;
        return LaboratoryBllDtoFactory.CreateDto(result);
    }

    public async Task<LaboratoryDto> UpdateAsync(LaboratoryUpdateDto dto)
    {
        var entity = await _uow.Laboratories.FindAsync(dto.Id)
                     ?? throw new KeyNotFoundException($"Laboratory {dto.Id} not found");
        LaboratoryBllDtoFactory.UpdateEntity(entity, dto);
        _uow.Laboratories.Update(entity);
        await _uow.SaveChangesAsync();
        var result = await _uow.Laboratories.FindWithDetailsAsync(entity.Id) ?? entity;
        return LaboratoryBllDtoFactory.CreateDto(result);
    }

    public async Task<bool> RemoveAsync(Guid id)
    {
        if (!await _uow.Laboratories.RemoveAsync(id)) return false;
        await _uow.SaveChangesAsync();
        return true;
    }
}
