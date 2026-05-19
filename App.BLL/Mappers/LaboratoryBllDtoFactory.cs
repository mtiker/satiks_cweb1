using App.Domain;
using App.DTO.v1;
using Base.Domain;

namespace App.BLL.Mappers;

public static class LaboratoryBllDtoFactory
{
    public static LaboratoryDto CreateDto(Laboratory l) => new()
    {
        Id = l.Id,
        Name = l.Name.ToString(),
        NameEn = l.Name.TryGetValue("en", out var nameEn) ? nameEn : null,
        NameEt = l.Name.TryGetValue("et", out var nameEt) ? nameEt : null,
        Description = l.Description?.ToString(),
        DescriptionEn = l.Description != null && l.Description.TryGetValue("en", out var descEn) ? descEn : null,
        DescriptionEt = l.Description != null && l.Description.TryGetValue("et", out var descEt) ? descEt : null,
        MaxOccupancy = l.MaxOccupancy,
        DepartmentId = l.DepartmentId,
        DepartmentName = l.Department?.Name.ToString(),
        LocationId = l.LocationId,
        LocationBuildingName = l.Location?.BuildingName.ToString(),
        EquipmentCount = l.Equipment?.Count ?? 0,
    };

    public static Laboratory CreateEntity(LaboratoryCreateDto dto)
    {
        var entity = new Laboratory
        {
            Name = new LangStr(dto.Name, "en"),
            Description = dto.Description != null ? new LangStr(dto.Description, "en") : null,
            MaxOccupancy = dto.MaxOccupancy,
            DepartmentId = dto.DepartmentId,
            LocationId = dto.LocationId,
        };
        if (dto.NameEt != null)
            entity.Name.SetTranslation(dto.NameEt, "et");
        if (dto.DescriptionEt != null && entity.Description != null)
            entity.Description.SetTranslation(dto.DescriptionEt, "et");
        return entity;
    }

    public static void UpdateEntity(Laboratory entity, LaboratoryUpdateDto dto)
    {
        entity.Name.SetTranslation(dto.Name, "en");
        if (dto.NameEt != null)
            entity.Name.SetTranslation(dto.NameEt, "et");
        if (dto.Description != null)
        {
            entity.Description ??= new LangStr(dto.Description, "en");
            entity.Description.SetTranslation(dto.Description, "en");
        }
        else
        {
            entity.Description = null;
        }
        if (dto.DescriptionEt != null && entity.Description != null)
            entity.Description.SetTranslation(dto.DescriptionEt, "et");
        entity.MaxOccupancy = dto.MaxOccupancy;
        entity.DepartmentId = dto.DepartmentId;
        entity.LocationId = dto.LocationId;
    }
}
