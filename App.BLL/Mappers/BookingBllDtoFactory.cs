using App.Domain;
using App.DTO.v1;

namespace App.BLL.Mappers;

public static class BookingBllDtoFactory
{
    public static BookingDto CreateDto(Booking b) => new()
    {
        Id = b.Id, StartTime = b.StartTime, EndTime = b.EndTime,
        Purpose = b.Purpose, Notes = b.Notes,
        BookingStatus = b.BookingStatus.ToString(),
        EquipmentId = b.EquipmentId, EquipmentName = b.Equipment?.Name,
        AppUserId = b.AppUserId, UserFullName = b.AppUser?.FullName,
    };

    public static Booking CreateEntity(BookingCreateDto dto, Guid appUserId) => new()
    {
        EquipmentId = dto.EquipmentId, AppUserId = appUserId,
        StartTime = dto.StartTime, EndTime = dto.EndTime,
        Purpose = dto.Purpose, Notes = dto.Notes,
        BookingStatus = BookingStatus.Pending,
    };

    public static void UpdateEntity(Booking entity, BookingUpdateDto dto)
    {
        entity.StartTime = dto.StartTime; entity.EndTime = dto.EndTime;
        entity.Purpose = dto.Purpose; entity.Notes = dto.Notes;
        entity.BookingStatus = Enum.Parse<BookingStatus>(dto.BookingStatus);
    }
}
