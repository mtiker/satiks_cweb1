using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.BLL.Contracts;
using App.BLL.Mappers;
using App.DAL.Contracts;
using App.Domain;
using App.DTO.v1;

namespace App.BLL.Services;

public class BookingService : IBookingService
{
    private readonly IAppUOW _uow;
    public BookingService(IAppUOW uow) => _uow = uow;

    public async Task<IEnumerable<BookingDto>> AllAsync(Guid? appUserId = null)
        => (await _uow.Bookings.AllWithDetailsAsync(appUserId)).Select(BookingBllDtoFactory.CreateDto);

    public async Task<BookingDto?> FindAsync(Guid id, Guid? appUserId = null)
    {
        var e = await _uow.Bookings.FindWithDetailsAsync(id, appUserId);
        return e == null ? null : BookingBllDtoFactory.CreateDto(e);
    }

    public async Task<BookingDto> AddAsync(BookingCreateDto dto, Guid appUserId)
    {
        if (dto.EndTime <= dto.StartTime)
            throw new InvalidOperationException("End time must be after start time.");

        var equipment = await _uow.Equipment.FindWithDetailsAsync(dto.EquipmentId)
                        ?? throw new KeyNotFoundException($"Equipment {dto.EquipmentId} not found");
        if (equipment.EquipmentCategory?.RequiresTraining == true)
        {
            var hasCert = await _uow.TrainingCertifications
                .UserHasApprovedCertificationAsync(appUserId, equipment.EquipmentCategoryId);
            if (!hasCert)
                throw new InvalidOperationException(
                    "This equipment requires a valid approved training certification.");
        }

        if (await _uow.Bookings.HasConflictingBookingAsync(dto.EquipmentId, dto.StartTime, dto.EndTime))
            throw new InvalidOperationException("Equipment is already booked for this time slot.");

        var entity = BookingBllDtoFactory.CreateEntity(dto, appUserId);
        _uow.Bookings.Add(entity);
        await _uow.SaveChangesAsync();
        var result = await _uow.Bookings.FindWithDetailsAsync(entity.Id) ?? entity;
        return BookingBllDtoFactory.CreateDto(result);
    }

    public async Task<BookingDto> UpdateAsync(BookingUpdateDto dto, Guid? appUserId = null)
    {
        var entity = await _uow.Bookings.FindAsync(dto.Id)
                     ?? throw new KeyNotFoundException($"Booking {dto.Id} not found");
        if (appUserId.HasValue && entity.AppUserId != appUserId.Value)
            throw new UnauthorizedAccessException("Access denied.");
        BookingBllDtoFactory.UpdateEntity(entity, dto);
        _uow.Bookings.Update(entity);
        await _uow.SaveChangesAsync();
        var result = await _uow.Bookings.FindWithDetailsAsync(entity.Id) ?? entity;
        return BookingBllDtoFactory.CreateDto(result);
    }

    public async Task<bool> RemoveAsync(Guid id, Guid? appUserId = null)
    {
        if (!await _uow.Bookings.RemoveAsync(id, appUserId)) return false;
        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<BookingDto> ConfirmAsync(Guid id)
    {
        var entity = await _uow.Bookings.FindAsync(id)
                     ?? throw new KeyNotFoundException($"Booking {id} not found");
        entity.BookingStatus = BookingStatus.Confirmed;
        _uow.Bookings.Update(entity);
        await _uow.SaveChangesAsync();
        var result = await _uow.Bookings.FindWithDetailsAsync(entity.Id) ?? entity;
        return BookingBllDtoFactory.CreateDto(result);
    }

    public async Task<BookingDto> CancelAsync(Guid id, Guid? appUserId = null)
    {
        var entity = await _uow.Bookings.FindAsync(id)
                     ?? throw new KeyNotFoundException($"Booking {id} not found");
        if (appUserId != null && entity.AppUserId != appUserId)
            throw new UnauthorizedAccessException("Access denied.");
        entity.BookingStatus = BookingStatus.Cancelled;
        _uow.Bookings.Update(entity);
        await _uow.SaveChangesAsync();
        var result = await _uow.Bookings.FindWithDetailsAsync(entity.Id) ?? entity;
        return BookingBllDtoFactory.CreateDto(result);
    }
}
