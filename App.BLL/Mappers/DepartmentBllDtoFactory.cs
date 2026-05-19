using App.Domain;
using App.DTO.v1;
using Base.Domain;

namespace App.BLL.Mappers;

public static class DepartmentBllDtoFactory
{
    public static DepartmentDto CreateDto(Department d) => new()
    {
        Id = d.Id,
        Name = d.Name.ToString(),
        NameEn = d.Name.TryGetValue("en", out var nameEn) ? nameEn : null,
        NameEt = d.Name.TryGetValue("et", out var nameEt) ? nameEt : null,
        Description = d.Description,
        CostCenter = d.CostCenter,
        LaboratoryCount = d.Laboratories?.Count ?? 0,
    };

    public static Department CreateEntity(DepartmentCreateDto dto)
    {
        var entity = new Department
        {
            Name = new LangStr(dto.Name, "en"),
            Description = dto.Description,
            CostCenter = dto.CostCenter,
        };
        if (dto.NameEt != null)
            entity.Name.SetTranslation(dto.NameEt, "et");
        return entity;
    }

    public static void UpdateEntity(Department entity, DepartmentUpdateDto dto)
    {
        entity.Name.SetTranslation(dto.Name, "en");
        if (dto.NameEt != null)
            entity.Name.SetTranslation(dto.NameEt, "et");
        entity.Description = dto.Description;
        entity.CostCenter = dto.CostCenter;
    }
}