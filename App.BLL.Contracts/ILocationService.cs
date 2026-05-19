using App.DTO.v1;

namespace App.BLL.Contracts;

public interface ILocationService
{
    Task<IEnumerable<LocationDto>> AllAsync();
    Task<LocationDto?> FindAsync(Guid id);
    Task<LocationDto> AddAsync(LocationCreateDto dto);
    Task<LocationDto> UpdateAsync(LocationUpdateDto dto);
    Task<bool> RemoveAsync(Guid id);
}