using App.DTO.v1;

namespace App.BLL.Contracts;

public interface IEquipmentCategoryService
{
    Task<IEnumerable<EquipmentCategoryDto>> AllAsync();
    Task<EquipmentCategoryDto?> FindAsync(Guid id);
    Task<EquipmentCategoryDto> AddAsync(EquipmentCategoryCreateDto dto);
    Task<EquipmentCategoryDto> UpdateAsync(EquipmentCategoryUpdateDto dto);
    Task<bool> RemoveAsync(Guid id);
    Task<IEnumerable<EquipmentCategoryDto>> GetRequiringTrainingAsync();
}