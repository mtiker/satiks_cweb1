using App.DTO.v1;
namespace App.BLL.Contracts;

public interface IManufacturerService
{
    Task<IEnumerable<ManufacturerDto>> AllAsync();
    Task<ManufacturerDto?> FindAsync(Guid id);
    Task<ManufacturerDto> AddAsync(ManufacturerCreateDto dto);
    Task<ManufacturerDto> UpdateAsync(ManufacturerUpdateDto dto);
    Task<bool> RemoveAsync(Guid id);
}
