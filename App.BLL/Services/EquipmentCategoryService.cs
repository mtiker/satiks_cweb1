using App.BLL.Contracts;
using App.BLL.Mappers;
using App.DAL.Contracts;
using App.DTO.v1;

namespace App.BLL.Services;

public class EquipmentCategoryService : IEquipmentCategoryService
{
    private readonly IAppUOW _uow;
    public EquipmentCategoryService(IAppUOW uow) => _uow = uow;

    public async Task<IEnumerable<EquipmentCategoryDto>> AllAsync()
        => (await _uow.EquipmentCategories.AllWithDetailsAsync()).Select(EquipmentCategoryBllDtoFactory.CreateDto);

    public async Task<EquipmentCategoryDto?> FindAsync(Guid id)
    {
        var e = await _uow.EquipmentCategories.FindAsync(id);
        return e == null ? null : EquipmentCategoryBllDtoFactory.CreateDto(e);
    }

    public async Task<EquipmentCategoryDto> AddAsync(EquipmentCategoryCreateDto dto)
    {
        var entity = EquipmentCategoryBllDtoFactory.CreateEntity(dto);
        _uow.EquipmentCategories.Add(entity);
        await _uow.SaveChangesAsync();
        var result = await _uow.EquipmentCategories.FindAsync(entity.Id) ?? entity;
        return EquipmentCategoryBllDtoFactory.CreateDto(result);
    }

    public async Task<EquipmentCategoryDto> UpdateAsync(EquipmentCategoryUpdateDto dto)
    {
        var entity = await _uow.EquipmentCategories.FindAsync(dto.Id)
                     ?? throw new KeyNotFoundException($"EquipmentCategory {dto.Id} not found");
        EquipmentCategoryBllDtoFactory.UpdateEntity(entity, dto);
        _uow.EquipmentCategories.Update(entity);
        await _uow.SaveChangesAsync();
        var result = await _uow.EquipmentCategories.FindAsync(entity.Id) ?? entity;
        return EquipmentCategoryBllDtoFactory.CreateDto(result);
    }

    public async Task<bool> RemoveAsync(Guid id)
    {
        if (!await _uow.EquipmentCategories.RemoveAsync(id)) return false;
        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<EquipmentCategoryDto>> GetRequiringTrainingAsync()
        => (await _uow.EquipmentCategories.GetRequiringTrainingAsync()).Select(EquipmentCategoryBllDtoFactory.CreateDto);
}