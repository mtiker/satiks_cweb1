using App.DTO.v1;
namespace App.BLL.Contracts;

public interface ILaboratoryService
{
    Task<IEnumerable<LaboratoryDto>> AllAsync();
    Task<LaboratoryDto?> FindAsync(Guid id);
    Task<LaboratoryDto> AddAsync(LaboratoryCreateDto dto);
    Task<LaboratoryDto> UpdateAsync(LaboratoryUpdateDto dto);
    Task<bool> RemoveAsync(Guid id);
}
