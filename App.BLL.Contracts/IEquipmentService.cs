using App.DTO.v1;
namespace App.BLL.Contracts;

public interface IEquipmentService
{
    Task<IEnumerable<EquipmentDto>> AllAsync();
    Task<EquipmentDto?> FindAsync(Guid id);
    Task<EquipmentDto> AddAsync(EquipmentCreateDto dto);
    Task<EquipmentDto> UpdateAsync(EquipmentUpdateDto dto);
    Task<bool> RemoveAsync(Guid id);
    Task<IEnumerable<EquipmentDto>> GetAvailableAsync();
    Task<IEnumerable<EquipmentDto>> GetByLaboratoryAsync(Guid laboratoryId);
    Task<IEnumerable<EquipmentDto>> GetByCategoryAsync(Guid categoryId);
}
