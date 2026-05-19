using App.Domain;
using App.DTO.v1;
using Base.Domain;

namespace App.BLL.Mappers;

public static class LocationBllDtoFactory
{
    public static LocationDto CreateDto(Location l) => new()
    {
        Id = l.Id,
        BuildingName = l.BuildingName.ToString(),
        BuildingNameEn = l.BuildingName.TryGetValue("en", out var buildingNameEn) ? buildingNameEn : null,
        BuildingNameEt = l.BuildingName.TryGetValue("et", out var buildingNameEt) ? buildingNameEt : null,
        Floor = l.Floor,
        RoomNumber = l.RoomNumber,
        Address = l.Address,
    };

    public static Location CreateEntity(LocationCreateDto dto)
    {
        var entity = new Location
        {
            BuildingName = new LangStr(dto.BuildingName, "en"),
            Floor = dto.Floor,
            RoomNumber = dto.RoomNumber,
            Address = dto.Address,
        };
        if (dto.BuildingNameEt != null)
            entity.BuildingName.SetTranslation(dto.BuildingNameEt, "et");
        return entity;
    }

    public static void UpdateEntity(Location entity, LocationUpdateDto dto)
    {
        entity.BuildingName.SetTranslation(dto.BuildingName, "en");
        if (dto.BuildingNameEt != null)
            entity.BuildingName.SetTranslation(dto.BuildingNameEt, "et");
        entity.Floor = dto.Floor;
        entity.RoomNumber = dto.RoomNumber;
        entity.Address = dto.Address;
    }
}