using App.Domain;
using App.DTO.v1;
using Base.Domain;

namespace App.BLL.Mappers;

public static class EquipmentCategoryBllDtoFactory
{
    public static EquipmentCategoryDto CreateDto(EquipmentCategory c) => new()
    {
        Id = c.Id,
        Name = c.Name.ToString(),
        NameEn = c.Name.TryGetValue("en", out var nameEn) ? nameEn : null,
        NameEt = c.Name.TryGetValue("et", out var nameEt) ? nameEt : null,
        Description = c.Description,
        RequiresTraining = c.RequiresTraining,
        EquipmentCount = c.Equipment?.Count ?? 0,
    };

    public static EquipmentCategory CreateEntity(EquipmentCategoryCreateDto dto)
    {
        var entity = new EquipmentCategory
        {
            Name = new LangStr(dto.Name, "en"),
            Description = dto.Description,
            RequiresTraining = dto.RequiresTraining,
        };
        if (dto.NameEt != null)
            entity.Name.SetTranslation(dto.NameEt, "et");
        return entity;
    }

    public static void UpdateEntity(EquipmentCategory entity, EquipmentCategoryUpdateDto dto)
    {
        entity.Name.SetTranslation(dto.Name, "en");
        if (dto.NameEt != null)
            entity.Name.SetTranslation(dto.NameEt, "et");
        entity.Description = dto.Description;
        entity.RequiresTraining = dto.RequiresTraining;
    }
}