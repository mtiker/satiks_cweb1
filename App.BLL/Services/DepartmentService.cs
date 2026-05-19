using App.BLL.Contracts;
using App.BLL.Mappers;
using App.DAL.Contracts;
using App.DTO.v1;

namespace App.BLL.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IAppUOW _uow;
    public DepartmentService(IAppUOW uow) => _uow = uow;

    public async Task<IEnumerable<DepartmentDto>> AllAsync()
        => (await _uow.Departments.AllWithDetailsAsync()).Select(DepartmentBllDtoFactory.CreateDto);

    public async Task<DepartmentDto?> FindAsync(Guid id)
    {
        var e = await _uow.Departments.FindAsync(id);
        return e == null ? null : DepartmentBllDtoFactory.CreateDto(e);
    }

    public async Task<DepartmentDto> AddAsync(DepartmentCreateDto dto)
    {
        var entity = DepartmentBllDtoFactory.CreateEntity(dto);
        _uow.Departments.Add(entity);
        await _uow.SaveChangesAsync();
        var result = await _uow.Departments.FindAsync(entity.Id) ?? entity;
        return DepartmentBllDtoFactory.CreateDto(result);
    }

    public async Task<DepartmentDto> UpdateAsync(DepartmentUpdateDto dto)
    {
        var entity = await _uow.Departments.FindAsync(dto.Id)
                     ?? throw new KeyNotFoundException($"Department {dto.Id} not found");
        DepartmentBllDtoFactory.UpdateEntity(entity, dto);
        _uow.Departments.Update(entity);
        await _uow.SaveChangesAsync();
        return DepartmentBllDtoFactory.CreateDto(entity);
    }

    public async Task<bool> RemoveAsync(Guid id)
    {
        if (!await _uow.Departments.RemoveAsync(id)) return false;
        await _uow.SaveChangesAsync();
        return true;
    }
}