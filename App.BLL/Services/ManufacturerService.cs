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

public class ManufacturerService : IManufacturerService
{
    private readonly IAppUOW _uow;
    public ManufacturerService(IAppUOW uow) => _uow = uow;

    public async Task<IEnumerable<ManufacturerDto>> AllAsync()
        => (await _uow.Manufacturers.AllAsync()).Select(ManufacturerBllDtoFactory.CreateDto);

    public async Task<ManufacturerDto?> FindAsync(Guid id)
    {
        var e = await _uow.Manufacturers.FindAsync(id);
        return e == null ? null : ManufacturerBllDtoFactory.CreateDto(e);
    }

    public async Task<ManufacturerDto> AddAsync(ManufacturerCreateDto dto)
    {
        var entity = ManufacturerBllDtoFactory.CreateEntity(dto);
        _uow.Manufacturers.Add(entity);
        await _uow.SaveChangesAsync();
        var result = await _uow.Manufacturers.FindAsync(entity.Id) ?? entity;
        return ManufacturerBllDtoFactory.CreateDto(result);
    }

    public async Task<ManufacturerDto> UpdateAsync(ManufacturerUpdateDto dto)
    {
        var entity = await _uow.Manufacturers.FindAsync(dto.Id)
                     ?? throw new KeyNotFoundException($"Manufacturer {dto.Id} not found");
        ManufacturerBllDtoFactory.UpdateEntity(entity, dto);
        _uow.Manufacturers.Update(entity);
        await _uow.SaveChangesAsync();
        return ManufacturerBllDtoFactory.CreateDto(entity);
    }

    public async Task<bool> RemoveAsync(Guid id)
    {
        if (!await _uow.Manufacturers.RemoveAsync(id)) return false;
        await _uow.SaveChangesAsync();
        return true;
    }
}
