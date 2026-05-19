using App.Domain;
using App.DTO.v1;
using Base.Domain;

namespace App.BLL.Mappers;

public static class EquipmentBllDtoFactory
{
    public static EquipmentDto CreateDto(Equipment e) => new()
    {
        Id = e.Id,
        Name = e.Name.ToString(),
        NameEn = e.Name.TryGetValue("en", out var nameEn) ? nameEn : null,
        NameEt = e.Name.TryGetValue("et", out var nameEt) ? nameEt : null,
        SerialNumber = e.SerialNumber,
        ModelName = e.ModelName,
        Description = e.Description?.ToString(),
        DescriptionEn = e.Description != null && e.Description.TryGetValue("en", out var descEn) ? descEn : null,
        DescriptionEt = e.Description != null && e.Description.TryGetValue("et", out var descEt) ? descEt : null,
        PurchaseDate = e.PurchaseDate,
        WarrantyExpiry = e.WarrantyExpiry,
        PurchasePrice = e.PurchasePrice,
        IsAvailableForBooking = e.IsAvailableForBooking,
        EquipmentCondition = e.EquipmentCondition.ToString(),
        LaboratoryId = e.LaboratoryId,
        LaboratoryName = e.Laboratory?.Name.ToString(),
        EquipmentCategoryId = e.EquipmentCategoryId,
        CategoryName = e.EquipmentCategory?.Name.ToString(),
        ManufacturerId = e.ManufacturerId,
        ManufacturerName = e.Manufacturer?.Name.ToString(),
        RequiresTraining = e.EquipmentCategory?.RequiresTraining ?? false,
    };

    public static Equipment CreateEntity(EquipmentCreateDto dto)
    {
        var entity = new Equipment
        {
            Name = new LangStr(dto.Name, "en"),
            SerialNumber = dto.SerialNumber,
            ModelName = dto.ModelName,
            Description = dto.Description != null ? new LangStr(dto.Description, "en") : null,
            PurchaseDate = dto.PurchaseDate,
            WarrantyExpiry = dto.WarrantyExpiry,
            PurchasePrice = dto.PurchasePrice,
            IsAvailableForBooking = dto.IsAvailableForBooking,
            EquipmentCondition = Enum.Parse<EquipmentCondition>(dto.EquipmentCondition),
            LaboratoryId = dto.LaboratoryId,
            EquipmentCategoryId = dto.EquipmentCategoryId,
            ManufacturerId = dto.ManufacturerId,
        };
        if (dto.NameEt != null)
            entity.Name.SetTranslation(dto.NameEt, "et");
        if (dto.DescriptionEt != null && entity.Description != null)
            entity.Description.SetTranslation(dto.DescriptionEt, "et");
        return entity;
    }

    public static void UpdateEntity(Equipment entity, EquipmentUpdateDto dto)
    {
        entity.Name.SetTranslation(dto.Name, "en");
        if (dto.NameEt != null)
            entity.Name.SetTranslation(dto.NameEt, "et");
        entity.SerialNumber = dto.SerialNumber;
        entity.ModelName = dto.ModelName;
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
        entity.PurchaseDate = dto.PurchaseDate;
        entity.WarrantyExpiry = dto.WarrantyExpiry;
        entity.PurchasePrice = dto.PurchasePrice;
        entity.IsAvailableForBooking = dto.IsAvailableForBooking;
        entity.EquipmentCondition = Enum.Parse<EquipmentCondition>(dto.EquipmentCondition);
        entity.LaboratoryId = dto.LaboratoryId;
        entity.EquipmentCategoryId = dto.EquipmentCategoryId;
        entity.ManufacturerId = dto.ManufacturerId;
    }
}
