using App.BLL.Services;
using App.DAL.Contracts;
using App.Domain;
using App.DTO.v1;
using Moq;

namespace WebApp.Tests.Unit;

public class MaintenanceRecordServiceTests
{
    private readonly Mock<IAppUOW>                      _uowMock  = new();
    private readonly Mock<IMaintenanceRecordRepository> _repoMock = new();
    private readonly MaintenanceRecordService           _service;

    public MaintenanceRecordServiceTests()
    {
        _uowMock.Setup(u => u.MaintenanceRecords).Returns(_repoMock.Object);
        _uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _service = new MaintenanceRecordService(_uowMock.Object);
    }

    private static MaintenanceRecord BuildRecord(Guid id, Guid equipmentId, Guid? performedByUserId) =>
        new()
        {
            Id                = id,
            EquipmentId       = equipmentId,
            PerformedByUserId = performedByUserId,
            ScheduledDate     = DateTime.UtcNow.AddDays(-1),
            Description       = "Test maintenance",
            IsScheduled       = true,
        };

    // ── AllAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task AllAsync_WithUserId_DelegatesToRepo()
    {
        var userId = Guid.NewGuid();
        var eqId   = Guid.NewGuid();
        var rec    = BuildRecord(Guid.NewGuid(), eqId, userId);

        _repoMock.Setup(r => r.AllWithDetailsAsync(userId))
                 .ReturnsAsync(new[] { rec }.ToList());

        var result = (await _service.AllAsync(userId)).ToList();

        Assert.Single(result);
        Assert.Equal(userId, result[0].PerformedByUserId);
    }

    [Fact]
    public async Task AllAsync_WithoutUserId_ReturnsAllRecords()
    {
        var rec1 = BuildRecord(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var rec2 = BuildRecord(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        _repoMock.Setup(r => r.AllWithDetailsAsync(null))
                 .ReturnsAsync(new[] { rec1, rec2 }.ToList());

        var result = (await _service.AllAsync()).ToList();

        Assert.Equal(2, result.Count);
    }

    // ── FindAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task FindAsync_WrongUser_ReturnsNull()
    {
        var recordId = Guid.NewGuid();
        var userId   = Guid.NewGuid();

        _repoMock.Setup(r => r.FindWithDetailsAsync(recordId, userId))
                 .ReturnsAsync((MaintenanceRecord?)null);

        Assert.Null(await _service.FindAsync(recordId, userId));
    }

    [Fact]
    public async Task FindAsync_AdminNoFilter_ReturnsRecord()
    {
        var recordId  = Guid.NewGuid();
        var record    = BuildRecord(recordId, Guid.NewGuid(), Guid.NewGuid());

        _repoMock.Setup(r => r.FindWithDetailsAsync(recordId, null))
                 .ReturnsAsync(record);

        var result = await _service.FindAsync(recordId);

        Assert.NotNull(result);
        Assert.Equal(recordId, result!.Id);
    }

    // ── AddAsync (admin overload, no userId) ────────────────────────────────

    [Fact]
    public async Task AddAsync_AdminOverload_AddsEntity()
    {
        var equipmentId = Guid.NewGuid();
        var dto = new MaintenanceRecordCreateDto
        {
            ScheduledDate = DateTime.UtcNow,
            Description   = "Routine check",
            EquipmentId   = equipmentId,
        };

        MaintenanceRecord? captured = null;
        _repoMock.Setup(r => r.Add(It.IsAny<MaintenanceRecord>()))
                 .Callback<MaintenanceRecord>(e => captured = e)
                 .Returns<MaintenanceRecord>(e => e);
        _repoMock.Setup(r => r.FindWithDetailsAsync(It.IsAny<Guid>(), null))
                 .ReturnsAsync((MaintenanceRecord?)null);

        await _service.AddAsync(dto);

        Assert.NotNull(captured);
        Assert.Equal(equipmentId, captured!.EquipmentId);
        _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    // ── AddAsync (user overload, sets PerformedByUserId) ────────────────────

    [Fact]
    public async Task AddAsync_UserOverload_SetsPerformedByUserId()
    {
        var userId = Guid.NewGuid();
        var dto = new MaintenanceRecordCreateDto
        {
            ScheduledDate = DateTime.UtcNow,
            Description   = "User maintenance",
            EquipmentId   = Guid.NewGuid(),
        };

        MaintenanceRecord? captured = null;
        _repoMock.Setup(r => r.Add(It.IsAny<MaintenanceRecord>()))
                 .Callback<MaintenanceRecord>(e => captured = e)
                 .Returns<MaintenanceRecord>(e => e);
        _repoMock.Setup(r => r.FindWithDetailsAsync(It.IsAny<Guid>(), null))
                 .ReturnsAsync((MaintenanceRecord?)null);

        await _service.AddAsync(dto, userId);

        Assert.NotNull(captured);
        Assert.Equal(userId, captured!.PerformedByUserId);
    }

    // ── UpdateAsync (admin overload) ─────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_AdminOverload_UpdatesEntity()
    {
        var recordId = Guid.NewGuid();
        var record   = BuildRecord(recordId, Guid.NewGuid(), null);
        var dto = new MaintenanceRecordUpdateDto
        {
            Id            = recordId,
            ScheduledDate = DateTime.UtcNow,
            Description   = "Updated description",
            EquipmentId   = record.EquipmentId,
        };

        _repoMock.Setup(r => r.FindAsync(recordId)).ReturnsAsync(record);
        _repoMock.Setup(r => r.FindWithDetailsAsync(recordId, null)).ReturnsAsync(record);
        _repoMock.Setup(r => r.Update(It.IsAny<MaintenanceRecord>())).Returns(record);

        var result = await _service.UpdateAsync(dto);

        Assert.NotNull(result);
        _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_AdminOverload_NotFound_ThrowsKeyNotFound()
    {
        _repoMock.Setup(r => r.FindAsync(It.IsAny<Guid>())).ReturnsAsync((MaintenanceRecord?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.UpdateAsync(new MaintenanceRecordUpdateDto
            {
                Id = Guid.NewGuid(), ScheduledDate = DateTime.UtcNow,
                Description = "x", EquipmentId = Guid.NewGuid(),
            }));
    }

    // ── UpdateAsync (user overload, ownership check) ──────────────────────

    [Fact]
    public async Task UpdateAsync_UserOverload_WrongUser_ThrowsUnauthorized()
    {
        var recordId  = Guid.NewGuid();
        var ownerId   = Guid.NewGuid();
        var requestId = Guid.NewGuid();
        var record    = BuildRecord(recordId, Guid.NewGuid(), ownerId);

        _repoMock.Setup(r => r.FindAsync(recordId)).ReturnsAsync(record);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.UpdateAsync(
                new MaintenanceRecordUpdateDto
                {
                    Id = recordId, ScheduledDate = DateTime.UtcNow,
                    Description = "x", EquipmentId = Guid.NewGuid(),
                },
                requestId));
    }

    [Fact]
    public async Task UpdateAsync_UserOverload_Owner_Succeeds()
    {
        var recordId = Guid.NewGuid();
        var ownerId  = Guid.NewGuid();
        var record   = BuildRecord(recordId, Guid.NewGuid(), ownerId);
        var dto = new MaintenanceRecordUpdateDto
        {
            Id = recordId, ScheduledDate = DateTime.UtcNow,
            Description = "Updated", EquipmentId = record.EquipmentId,
        };

        _repoMock.Setup(r => r.FindAsync(recordId)).ReturnsAsync(record);
        _repoMock.Setup(r => r.FindWithDetailsAsync(recordId, null)).ReturnsAsync(record);
        _repoMock.Setup(r => r.Update(It.IsAny<MaintenanceRecord>())).Returns(record);

        var result = await _service.UpdateAsync(dto, ownerId);

        Assert.NotNull(result);
        _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    // ── RemoveAsync (admin overload) ──────────────────────────────────────

    [Fact]
    public async Task RemoveAsync_AdminOverload_NotFound_ReturnsFalse()
    {
        _repoMock.Setup(r => r.RemoveAsync(It.IsAny<Guid>(), null)).ReturnsAsync(false);

        Assert.False(await _service.RemoveAsync(Guid.NewGuid()));
        _uowMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task RemoveAsync_AdminOverload_Found_ReturnsTrue()
    {
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.RemoveAsync(id, null)).ReturnsAsync(true);

        Assert.True(await _service.RemoveAsync(id));
        _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    // ── RemoveAsync (user overload, ownership check) ──────────────────────

    [Fact]
    public async Task RemoveAsync_UserOverload_NotOwner_ReturnsFalse()
    {
        var recordId    = Guid.NewGuid();
        var ownerId     = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        var record      = BuildRecord(recordId, Guid.NewGuid(), ownerId);

        _repoMock.Setup(r => r.FindAsync(recordId)).ReturnsAsync(record);

        Assert.False(await _service.RemoveAsync(recordId, requesterId));
        _repoMock.Verify(r => r.RemoveAsync(It.IsAny<Guid>(), It.IsAny<Guid?>()), Times.Never);
    }

    [Fact]
    public async Task RemoveAsync_UserOverload_Owner_ReturnsTrue()
    {
        var recordId = Guid.NewGuid();
        var ownerId  = Guid.NewGuid();
        var record   = BuildRecord(recordId, Guid.NewGuid(), ownerId);

        _repoMock.Setup(r => r.FindAsync(recordId)).ReturnsAsync(record);
        _repoMock.Setup(r => r.RemoveAsync(recordId, null)).ReturnsAsync(true);

        Assert.True(await _service.RemoveAsync(recordId, ownerId));
        _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    // ── GetByEquipmentAsync ───────────────────────────────────────────────

    [Fact]
    public async Task GetByEquipmentAsync_ReturnsMatchingRecords()
    {
        var equipmentId = Guid.NewGuid();
        var records = new[]
        {
            BuildRecord(Guid.NewGuid(), equipmentId, null),
            BuildRecord(Guid.NewGuid(), equipmentId, null),
        };

        _repoMock.Setup(r => r.GetByEquipmentAsync(equipmentId))
                 .ReturnsAsync(records.ToList());

        var result = (await _service.GetByEquipmentAsync(equipmentId)).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, dto => Assert.Equal(equipmentId, dto.EquipmentId));
    }

    // ── GetScheduledAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task GetScheduledAsync_ReturnsScheduledRecords()
    {
        var records = new[]
        {
            BuildRecord(Guid.NewGuid(), Guid.NewGuid(), null),
        };

        _repoMock.Setup(r => r.GetScheduledAsync()).ReturnsAsync(records.ToList());

        var result = (await _service.GetScheduledAsync()).ToList();

        Assert.Single(result);
    }
}
