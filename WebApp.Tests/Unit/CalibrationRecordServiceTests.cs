using App.BLL.Contracts;
using App.BLL.Services;
using App.DAL.Contracts;
using App.Domain;
using App.DTO.v1;
using Moq;

namespace WebApp.Tests.Unit;

public class CalibrationRecordServiceTests
{
    private readonly Mock<IAppUOW>                     _uowMock       = new();
    private readonly Mock<ICalibrationRecordRepository> _repoMock     = new();
    private readonly CalibrationRecordService          _service;

    public CalibrationRecordServiceTests()
    {
        _uowMock.Setup(u => u.CalibrationRecords)
                .Returns(_repoMock.Object);
        _uowMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);
        _service = new CalibrationRecordService(_uowMock.Object);
    }

    private static CalibrationRecord BuildCalibration(
        Guid id, Guid equipmentId, Guid? calibratedByUserId)
        => new()
        {
            Id               = id,
            EquipmentId      = equipmentId,
            CalibratedByUserId = calibratedByUserId,
            CalibrationDate  = DateTime.UtcNow.AddMonths(-1),
            NextCalibrationDue = DateTime.UtcNow.AddMonths(5),
            Passed           = true,
        };

    [Fact]
    public async Task AllAsync_WithUserId_ReturnsOnlyOwnedRecords()
    {
        var userId   = Guid.NewGuid();
        var eqId     = Guid.NewGuid();
        var rec1     = BuildCalibration(Guid.NewGuid(), eqId, userId);
        var rec2     = BuildCalibration(Guid.NewGuid(), eqId, userId);
        var rec3     = BuildCalibration(Guid.NewGuid(), eqId, Guid.NewGuid());

        _repoMock
            .Setup(r => r.AllWithDetailsAsync(userId))
            .ReturnsAsync(new[] { rec1, rec2 }.ToList());

        var result = (await _service.AllAsync(userId)).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, dto => Assert.Equal(userId, dto.CalibratedByUserId));
    }

    [Fact]
    public async Task FindAsync_NotOwnedByUser_ReturnsNull()
    {
        var recordId    = Guid.NewGuid();
        var userId      = Guid.NewGuid();

        _repoMock
            .Setup(r => r.FindWithDetailsAsync(recordId, userId))
            .ReturnsAsync((CalibrationRecord?)null);

        var result = await _service.FindAsync(recordId, userId);

        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_SetsCalibratedByUserIdFromParameter()
    {
        var userId      = Guid.NewGuid();
        var dto         = new CalibrationRecordCreateDto
        {
            CalibrationDate    = DateTime.UtcNow,
            NextCalibrationDue = DateTime.UtcNow.AddMonths(6),
            EquipmentId        = Guid.NewGuid(),
            Passed             = true,
        };

        CalibrationRecord? capturedEntity = null;
        _repoMock
            .Setup(r => r.Add(It.IsAny<CalibrationRecord>()))
            .Callback<CalibrationRecord>(e => capturedEntity = e)
            .Returns<CalibrationRecord>(e => e);

        _repoMock
            .Setup(r => r.FindWithDetailsAsync(It.IsAny<Guid>(), null))
            .ReturnsAsync((CalibrationRecord?)null);

        await _service.AddAsync(dto, userId);

        Assert.NotNull(capturedEntity);
        Assert.Equal(userId, capturedEntity!.CalibratedByUserId);
    }

    [Fact]
    public async Task RemoveAsync_NotOwner_ReturnsFalse()
    {
        var recordId    = Guid.NewGuid();
        var ownerId     = Guid.NewGuid();
        var requesterId = Guid.NewGuid(); // different user
        var record      = BuildCalibration(recordId, Guid.NewGuid(), ownerId);

        // Service calls FindAsync first to check ownership before calling RemoveAsync.
        _repoMock
            .Setup(r => r.FindAsync(recordId))
            .ReturnsAsync(record);

        var result = await _service.RemoveAsync(recordId, requesterId);

        Assert.False(result);
        _repoMock.Verify(r => r.RemoveAsync(It.IsAny<Guid>(), It.IsAny<Guid?>()), Times.Never);
    }

    [Fact]
    public async Task AllAsync_NoFilter_ReturnsAllRecords()
    {
        var records = new[]
        {
            BuildCalibration(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()),
            BuildCalibration(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()),
        };
        _repoMock.Setup(r => r.AllWithDetailsAsync(null)).ReturnsAsync(records.ToList());

        var result = (await _service.AllAsync()).ToList();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task FindAsync_NoFilter_ReturnsRecord()
    {
        var id     = Guid.NewGuid();
        var record = BuildCalibration(id, Guid.NewGuid(), Guid.NewGuid());
        _repoMock.Setup(r => r.FindWithDetailsAsync(id, null)).ReturnsAsync(record);

        var result = await _service.FindAsync(id);

        Assert.NotNull(result);
        Assert.Equal(id, result!.Id);
    }

    [Fact]
    public async Task AddAsync_AdminOverload_DoesNotSetCalibratedByUserId()
    {
        var dto = new CalibrationRecordCreateDto
        {
            CalibrationDate    = DateTime.UtcNow,
            NextCalibrationDue = DateTime.UtcNow.AddMonths(6),
            EquipmentId        = Guid.NewGuid(),
            Passed             = true,
        };
        CalibrationRecord? captured = null;
        _repoMock.Setup(r => r.Add(It.IsAny<CalibrationRecord>()))
                 .Callback<CalibrationRecord>(e => captured = e)
                 .Returns<CalibrationRecord>(e => e);
        _repoMock.Setup(r => r.FindWithDetailsAsync(It.IsAny<Guid>(), null))
                 .ReturnsAsync((CalibrationRecord?)null);

        await _service.AddAsync(dto);

        Assert.NotNull(captured);
        _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_AdminOverload_NotFound_ThrowsKeyNotFound()
    {
        _repoMock.Setup(r => r.FindAsync(It.IsAny<Guid>())).ReturnsAsync((CalibrationRecord?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.UpdateAsync(new CalibrationRecordUpdateDto
            {
                Id = Guid.NewGuid(), CalibrationDate = DateTime.UtcNow,
                NextCalibrationDue = DateTime.UtcNow.AddMonths(6), EquipmentId = Guid.NewGuid(),
            }));
    }

    [Fact]
    public async Task UpdateAsync_AdminOverload_Found_Succeeds()
    {
        var id     = Guid.NewGuid();
        var record = BuildCalibration(id, Guid.NewGuid(), null);
        var dto = new CalibrationRecordUpdateDto
        {
            Id = id, CalibrationDate = DateTime.UtcNow,
            NextCalibrationDue = DateTime.UtcNow.AddMonths(6), EquipmentId = record.EquipmentId,
        };
        _repoMock.Setup(r => r.FindAsync(id)).ReturnsAsync(record);
        _repoMock.Setup(r => r.FindWithDetailsAsync(id, null)).ReturnsAsync(record);
        _repoMock.Setup(r => r.Update(It.IsAny<CalibrationRecord>())).Returns(record);

        var result = await _service.UpdateAsync(dto);

        Assert.NotNull(result);
        _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_UserOverload_WrongUser_ThrowsUnauthorized()
    {
        var id      = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var record  = BuildCalibration(id, Guid.NewGuid(), ownerId);
        _repoMock.Setup(r => r.FindAsync(id)).ReturnsAsync(record);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.UpdateAsync(
                new CalibrationRecordUpdateDto
                {
                    Id = id, CalibrationDate = DateTime.UtcNow,
                    NextCalibrationDue = DateTime.UtcNow.AddMonths(6), EquipmentId = Guid.NewGuid(),
                },
                Guid.NewGuid())); // wrong user
    }

    [Fact]
    public async Task RemoveAsync_AdminOverload_NotFound_ReturnsFalse()
    {
        _repoMock.Setup(r => r.RemoveAsync(It.IsAny<Guid>(), null)).ReturnsAsync(false);

        Assert.False(await _service.RemoveAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task RemoveAsync_AdminOverload_Found_ReturnsTrue()
    {
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.RemoveAsync(id, null)).ReturnsAsync(true);

        Assert.True(await _service.RemoveAsync(id));
        _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_UserOverload_Owner_ReturnsTrue()
    {
        var id      = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var record  = BuildCalibration(id, Guid.NewGuid(), ownerId);
        _repoMock.Setup(r => r.FindAsync(id)).ReturnsAsync(record);
        _repoMock.Setup(r => r.RemoveAsync(id, null)).ReturnsAsync(true);

        Assert.True(await _service.RemoveAsync(id, ownerId));
    }

    [Fact]
    public async Task GetByEquipmentAsync_ReturnsMatchingRecords()
    {
        var eqId = Guid.NewGuid();
        var records = new[]
        {
            BuildCalibration(Guid.NewGuid(), eqId, null),
            BuildCalibration(Guid.NewGuid(), eqId, null),
        };
        _repoMock.Setup(r => r.GetByEquipmentAsync(eqId)).ReturnsAsync(records.ToList());

        var result = (await _service.GetByEquipmentAsync(eqId)).ToList();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetLatestByEquipmentAsync_NoRecord_ReturnsNull()
    {
        _repoMock.Setup(r => r.GetLatestByEquipmentAsync(It.IsAny<Guid>()))
                 .ReturnsAsync((CalibrationRecord?)null);

        Assert.Null(await _service.GetLatestByEquipmentAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetLatestByEquipmentAsync_Found_ReturnsDto()
    {
        var eqId   = Guid.NewGuid();
        var record = BuildCalibration(Guid.NewGuid(), eqId, null);
        _repoMock.Setup(r => r.GetLatestByEquipmentAsync(eqId)).ReturnsAsync(record);

        var result = await _service.GetLatestByEquipmentAsync(eqId);

        Assert.NotNull(result);
        Assert.Equal(eqId, result!.EquipmentId);
    }

    [Fact]
    public async Task GetOverdueAsync_ReturnsOverdueRecords()
    {
        var records = new[]
        {
            BuildCalibration(Guid.NewGuid(), Guid.NewGuid(), null),
        };
        _repoMock.Setup(r => r.GetOverdueAsync(null)).ReturnsAsync(records.ToList());

        var result = (await _service.GetOverdueAsync()).ToList();

        Assert.Single(result);
    }
}
