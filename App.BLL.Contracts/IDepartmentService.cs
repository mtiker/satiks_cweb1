using App.DTO.v1;

namespace App.BLL.Contracts;

public interface IDepartmentService
{
    Task<IEnumerable<DepartmentDto>> AllAsync();
    Task<DepartmentDto?> FindAsync(Guid id);
    Task<DepartmentDto> AddAsync(DepartmentCreateDto dto);
    Task<DepartmentDto> UpdateAsync(DepartmentUpdateDto dto);
    Task<bool> RemoveAsync(Guid id);
}