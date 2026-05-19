using App.DTO.v1;
namespace App.BLL.Contracts;

public interface IBookingService
{
    Task<IEnumerable<BookingDto>> AllAsync(Guid? appUserId = null);
    Task<BookingDto?> FindAsync(Guid id, Guid? appUserId = null);
    Task<BookingDto> AddAsync(BookingCreateDto dto, Guid appUserId);
    Task<BookingDto> UpdateAsync(BookingUpdateDto dto, Guid? appUserId = null);
    Task<bool> RemoveAsync(Guid id, Guid? appUserId = null);
    Task<BookingDto> ConfirmAsync(Guid id);
    Task<BookingDto> CancelAsync(Guid id, Guid? appUserId = null);
}