using App.BLL.Services;
using App.DAL.Contracts;
using App.Domain;
using App.DTO.v1;
using Base.Domain;
using Moq;

namespace WebApp.Tests.Unit;

public class BookingServiceTests
{
    private readonly Mock<IAppUOW>              _uowMock           = new();
    private readonly Mock<IBookingRepository>   _bookingRepoMock   = new();
    private readonly Mock<IEquipmentRepository> _equipmentRepoMock = new();
    private readonly BookingService             _service;

    public BookingServiceTests()
    {
        _uowMock.Setup(u => u.Bookings).Returns(_bookingRepoMock.Object);
        // BookingService.AddAsync loads equipment before any booking check —
        // this mock must be present or AddAsync throws NullReferenceException
        // instead of the expected InvalidOperationException.
        _uowMock.Setup(u => u.Equipment).Returns(_equipmentRepoMock.Object);
        _uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _service = new BookingService(_uowMock.Object);
    }

    /// <summary>
    /// Returns a fully-populated Equipment entity with a resolved
    /// EquipmentCategory navigation property.
    /// </summary>
    private static Equipment BuildEquipment(Guid equipmentId, bool requiresTraining = false) =>
        new()
        {
            Id                    = equipmentId,
            Name                  = "Test Equipment",
            LaboratoryId          = Guid.NewGuid(),
            ManufacturerId        = Guid.NewGuid(),
            EquipmentCondition    = EquipmentCondition.Good,
            IsAvailableForBooking = true,
            EquipmentCategory = new EquipmentCategory
            {
                Id               = Guid.NewGuid(),
                Name             = new LangStr("Category", "en"),
                RequiresTraining = requiresTraining,
            },
        };

    [Fact]
    public async Task AddAsync_EndBeforeStart_ThrowsInvalidOperation()
    {
        var equipmentId = Guid.NewGuid();

        _equipmentRepoMock
            .Setup(r => r.FindWithDetailsAsync(equipmentId))
            .ReturnsAsync(BuildEquipment(equipmentId));

        var dto = new BookingCreateDto
        {
            EquipmentId = equipmentId,
            StartTime   = DateTime.UtcNow.AddHours(3),
            EndTime     = DateTime.UtcNow.AddHours(1), // end before start
        };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AddAsync(dto, Guid.NewGuid()));
    }

    [Fact]
    public async Task AddAsync_ConflictingBooking_ThrowsInvalidOperation()
    {
        var equipmentId = Guid.NewGuid();

        _equipmentRepoMock
            .Setup(r => r.FindWithDetailsAsync(equipmentId))
            .ReturnsAsync(BuildEquipment(equipmentId));

        _bookingRepoMock
            .Setup(r => r.HasConflictingBookingAsync(
                equipmentId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), null))
            .ReturnsAsync(true);

        var dto = new BookingCreateDto
        {
            EquipmentId = equipmentId,
            StartTime   = DateTime.UtcNow.AddHours(1),
            EndTime     = DateTime.UtcNow.AddHours(3),
        };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AddAsync(dto, Guid.NewGuid()));
    }

    [Fact]
    public async Task CancelAsync_OtherUsersBooking_ThrowsUnauthorized()
    {
        var bookingId   = Guid.NewGuid();
        var ownerId     = Guid.NewGuid();
        var requesterId = Guid.NewGuid(); // different user

        var booking = new Booking
        {
            Id            = bookingId,
            AppUserId     = ownerId,
            BookingStatus = BookingStatus.Pending,
            StartTime     = DateTime.UtcNow.AddDays(1),
            EndTime       = DateTime.UtcNow.AddDays(1).AddHours(2),
        };

        // CancelAsync calls FindAsync(id), not FindWithDetailsAsync
        _bookingRepoMock
            .Setup(r => r.FindAsync(bookingId))
            .ReturnsAsync(booking);
        // Then it calls FindWithDetailsAsync after saving
        _bookingRepoMock
            .Setup(r => r.FindWithDetailsAsync(bookingId, null))
            .ReturnsAsync(booking);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.CancelAsync(bookingId, requesterId));
    }

    [Fact]
    public async Task CancelAsync_AdminNullUserId_SkipsOwnershipCheck()
    {
        var bookingId = Guid.NewGuid();

        var booking = new Booking
        {
            Id            = bookingId,
            AppUserId     = Guid.NewGuid(), // some owner
            BookingStatus = BookingStatus.Pending,
            StartTime     = DateTime.UtcNow.AddDays(1),
            EndTime       = DateTime.UtcNow.AddDays(1).AddHours(2),
        };

        // CancelAsync calls FindAsync(id), not FindWithDetailsAsync
        _bookingRepoMock
            .Setup(r => r.FindAsync(bookingId))
            .ReturnsAsync(booking);
        // Then it calls FindWithDetailsAsync after saving
        _bookingRepoMock
            .Setup(r => r.FindWithDetailsAsync(bookingId, null))
            .ReturnsAsync(booking);

        // null appUserId = admin — must not throw, must save.
        await _service.CancelAsync(bookingId, appUserId: null);

        _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_NotFound_ReturnsFalse()
    {
        _bookingRepoMock
            .Setup(r => r.RemoveAsync(It.IsAny<Guid>(), null))
            .ReturnsAsync(false);

        Assert.False(await _service.RemoveAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task RemoveAsync_Found_ReturnsTrue()
    {
        var id = Guid.NewGuid();
        _bookingRepoMock.Setup(r => r.RemoveAsync(id, null)).ReturnsAsync(true);

        Assert.True(await _service.RemoveAsync(id));
        _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AllAsync_WithUserId_DelegatesToRepo()
    {
        var userId = Guid.NewGuid();
        _bookingRepoMock.Setup(r => r.AllWithDetailsAsync(userId))
                        .ReturnsAsync(new List<Booking>());

        var result = await _service.AllAsync(userId);

        Assert.Empty(result);
    }

    [Fact]
    public async Task FindAsync_NotFound_ReturnsNull()
    {
        _bookingRepoMock.Setup(r => r.FindWithDetailsAsync(It.IsAny<Guid>(), null))
                        .ReturnsAsync((Booking?)null);

        Assert.Null(await _service.FindAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task ConfirmAsync_NotFound_ThrowsKeyNotFound()
    {
        _bookingRepoMock.Setup(r => r.FindAsync(It.IsAny<Guid>()))
                        .ReturnsAsync((Booking?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.ConfirmAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task ConfirmAsync_SetsStatusToConfirmed()
    {
        var id = Guid.NewGuid();
        var booking = new Booking
        {
            Id            = id,
            AppUserId     = Guid.NewGuid(),
            BookingStatus = BookingStatus.Pending,
            StartTime     = DateTime.UtcNow.AddDays(1),
            EndTime       = DateTime.UtcNow.AddDays(1).AddHours(2),
        };
        _bookingRepoMock.Setup(r => r.FindAsync(id)).ReturnsAsync(booking);
        _bookingRepoMock.Setup(r => r.FindWithDetailsAsync(id, null)).ReturnsAsync(booking);
        _bookingRepoMock.Setup(r => r.Update(It.IsAny<Booking>())).Returns(booking);

        var result = await _service.ConfirmAsync(id);

        Assert.Equal(BookingStatus.Confirmed, booking.BookingStatus);
        _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ThrowsKeyNotFound()
    {
        _bookingRepoMock.Setup(r => r.FindAsync(It.IsAny<Guid>())).ReturnsAsync((Booking?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.UpdateAsync(new BookingUpdateDto
            {
                Id = Guid.NewGuid(),
                StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(1),
            }));
    }

    [Fact]
    public async Task UpdateAsync_WrongUser_ThrowsUnauthorized()
    {
        var id      = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var booking = new Booking
        {
            Id            = id,
            AppUserId     = ownerId,
            BookingStatus = BookingStatus.Pending,
            StartTime     = DateTime.UtcNow.AddDays(1),
            EndTime       = DateTime.UtcNow.AddDays(1).AddHours(2),
        };
        _bookingRepoMock.Setup(r => r.FindAsync(id)).ReturnsAsync(booking);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.UpdateAsync(
                new BookingUpdateDto { Id = id, StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(1) },
                Guid.NewGuid())); // different user
    }

    [Fact]
    public async Task AddAsync_TrainingRequired_NoCert_ThrowsInvalidOperation()
    {
        var equipmentId = Guid.NewGuid();
        var userId      = Guid.NewGuid();
        var certRepoMock = new Mock<ITrainingCertificationRepository>();
        _uowMock.Setup(u => u.TrainingCertifications).Returns(certRepoMock.Object);

        _equipmentRepoMock.Setup(r => r.FindWithDetailsAsync(equipmentId))
                          .ReturnsAsync(BuildEquipment(equipmentId, requiresTraining: true));
        certRepoMock.Setup(r => r.UserHasApprovedCertificationAsync(userId, It.IsAny<Guid>()))
                    .ReturnsAsync(false);

        var dto = new BookingCreateDto
        {
            EquipmentId = equipmentId,
            StartTime   = DateTime.UtcNow.AddHours(1),
            EndTime     = DateTime.UtcNow.AddHours(3),
        };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AddAsync(dto, userId));
    }
}
