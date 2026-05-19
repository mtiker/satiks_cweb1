using App.Domain;
using App.DTO.v1;
using Base.Domain;

namespace App.BLL.Mappers;

public static class ManufacturerBllDtoFactory
{
    public static ManufacturerDto CreateDto(Manufacturer m) => new()
    {
        Id = m.Id,
        Name = m.Name.ToString(),
        NameEn = m.Name.TryGetValue("en", out var nameEn) ? nameEn : null,
        NameEt = m.Name.TryGetValue("et", out var nameEt) ? nameEt : null,
        Country = m.Country,
        Website = m.Website,
        SupportPhone = m.SupportPhone,
        SupportEmail = m.SupportEmail,
    };

    public static Manufacturer CreateEntity(ManufacturerCreateDto dto)
    {
        var entity = new Manufacturer
        {
            Name = new LangStr(dto.Name, "en"),
            Country = dto.Country,
            Website = dto.Website,
            SupportPhone = dto.SupportPhone,
            SupportEmail = dto.SupportEmail,
        };
        if (dto.NameEt != null)
            entity.Name.SetTranslation(dto.NameEt, "et");
        return entity;
    }

    public static void UpdateEntity(Manufacturer entity, ManufacturerUpdateDto dto)
    {
        entity.Name.SetTranslation(dto.Name, "en");
        if (dto.NameEt != null)
            entity.Name.SetTranslation(dto.NameEt, "et");
        entity.Country = dto.Country;
        entity.Website = dto.Website;
        entity.SupportPhone = dto.SupportPhone;
        entity.SupportEmail = dto.SupportEmail;
    }
}
