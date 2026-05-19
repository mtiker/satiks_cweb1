using App.BLL.Contracts;
using App.BLL.Mappers;
using App.DAL.Contracts;
using App.DTO.v1;

namespace App.BLL.Services;

public class LocationService : ILocationService
{
    private readonly IAppUOW _uow;
    public LocationService(IAppUOW uow) => _uow = uow;

    public async Task<IEnumerable<LocationDto>> AllAsync()
        => (await _uow.Locations.AllAsync()).Select(LocationBllDtoFactory.CreateDto);

    public async Task<LocationDto?> FindAsync(Guid id)
    {
        var e = await _uow.Locations.FindAsync(id);
        return e == null ? null : LocationBllDtoFactory.CreateDto(e);
    }

    public async Task<LocationDto> AddAsync(LocationCreateDto dto)
    {
        var entity = LocationBllDtoFactory.CreateEntity(dto);
        _uow.Locations.Add(entity);
        await _uow.SaveChangesAsync();
        var result = await _uow.Locations.FindAsync(entity.Id) ?? entity;
        return LocationBllDtoFactory.CreateDto(result);
    }

    public async Task<LocationDto> UpdateAsync(LocationUpdateDto dto)
    {
        var entity = await _uow.Locations.FindAsync(dto.Id)
                     ?? throw new KeyNotFoundException($"Location {dto.Id} not found");
        LocationBllDtoFactory.UpdateEntity(entity, dto);
        _uow.Locations.Update(entity);
        await _uow.SaveChangesAsync();
        return LocationBllDtoFactory.CreateDto(entity);
    }

    public async Task<bool> RemoveAsync(Guid id)
    {
        if (!await _uow.Locations.RemoveAsync(id)) return false;
        await _uow.SaveChangesAsync();
        return true;
    }
}