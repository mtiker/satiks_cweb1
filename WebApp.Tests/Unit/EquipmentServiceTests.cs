using App.BLL.Services;
using App.DAL.Contracts;
using App.Domain;
using App.DTO.v1;
using Base.Domain;
using Moq;

namespace WebApp.Tests.Unit;

public class EquipmentServiceTests
{
    private readonly Mock<IAppUOW>              _uowMock  = new();
    private readonly Mock<IEquipmentRepository> _repoMock = new();
    private readonly EquipmentService           _service;

    public EquipmentServiceTests()
    {
        _uowMock.Setup(u => u.Equipment).Returns(_repoMock.Object);
        _uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _service = new EquipmentService(_uowMock.Object);
    }

    [Fact]
    public async Task FindAsync_UnknownId_ReturnsNull()
    {
        _repoMock
            .Setup(r => r.FindWithDetailsAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Equipment?)null);

        Assert.Null(await _service.FindAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task AllAsync_ReturnsMappedDtos()
    {
        var equipment = new List<Equipment>
        {
            new()
            {
                Id = Guid.NewGuid(), Name = "Scope 1",
                LaboratoryId        = Guid.NewGuid(),
                EquipmentCategoryId = Guid.NewGuid(),
                ManufacturerId      = Guid.NewGuid(),
            },
            new()
            {
                Id = Guid.NewGuid(), Name = "Scope 2",
                LaboratoryId        = Guid.NewGuid(),
                EquipmentCategoryId = Guid.NewGuid(),
                ManufacturerId      = Guid.NewGuid(),
            },
        };

        _repoMock
            .Setup(r => r.AllWithDetailsAsync())
            .ReturnsAsync(equipment);

        var result = (await _service.AllAsync()).ToList();
        Assert.Equal(2, result.Count);
        Assert.Equal("Scope 1", result[0].Name);
    }

    [Fact]
    public async Task RemoveAsync_NotFound_ReturnsFalse()
    {
        _repoMock
            .Setup(r => r.RemoveAsync(It.IsAny<Guid>(), null))
            .ReturnsAsync(false);

        Assert.False(await _service.RemoveAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateAsync_UnknownId_ThrowsKeyNotFound()
    {
        // DAL interface: FindAsync(Guid id, Guid? userId = null)
        _repoMock
            .Setup(r => r.FindWithDetailsAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Equipment?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.UpdateAsync(new EquipmentUpdateDto
            {
                Id                  = Guid.NewGuid(),
                Name                = "x",
                EquipmentCondition  = "Good",
                LaboratoryId        = Guid.NewGuid(),
                EquipmentCategoryId = Guid.NewGuid(),
                ManufacturerId      = Guid.NewGuid(),
            }));
    }

    [Fact]
    public async Task AddAsync_PersistsAndReturnsDto()
    {
        var dto = new EquipmentCreateDto
        {
            Name                = "New Scope",
            EquipmentCondition  = "Good",
            LaboratoryId        = Guid.NewGuid(),
            EquipmentCategoryId = Guid.NewGuid(),
            ManufacturerId      = Guid.NewGuid(),
        };
        Equipment? captured = null;
        _repoMock.Setup(r => r.Add(It.IsAny<Equipment>()))
                 .Callback<Equipment>(e => captured = e)
                 .Returns<Equipment>(e => e);
        _repoMock.Setup(r => r.FindWithDetailsAsync(It.IsAny<Guid>())).ReturnsAsync((Equipment?)null);

        await _service.AddAsync(dto);

        Assert.NotNull(captured);
        Assert.Equal("New Scope", captured!.Name);
        _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_Found_Succeeds()
    {
        var id = Guid.NewGuid();
        var equipment = new Equipment
        {
            Id                  = id,
            Name                = "Old Name",
            LaboratoryId        = Guid.NewGuid(),
            EquipmentCategoryId = Guid.NewGuid(),
            ManufacturerId      = Guid.NewGuid(),
            EquipmentCondition  = EquipmentCondition.Good,
        };
        var dto = new EquipmentUpdateDto
        {
            Id                  = id,
            Name                = "Updated",
            EquipmentCondition  = "Good",
            LaboratoryId        = equipment.LaboratoryId,
            EquipmentCategoryId = equipment.EquipmentCategoryId,
            ManufacturerId      = equipment.ManufacturerId,
        };
        _repoMock.Setup(r => r.FindAsync(It.IsAny<object[]>())).ReturnsAsync(equipment);
        _repoMock.Setup(r => r.FindWithDetailsAsync(id)).ReturnsAsync(equipment);
        _repoMock.Setup(r => r.Update(It.IsAny<Equipment>())).Returns(equipment);

        var result = await _service.UpdateAsync(dto);

        Assert.NotNull(result);
        _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_Found_ReturnsTrue()
    {
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.RemoveAsync(id, null)).ReturnsAsync(true);

        Assert.True(await _service.RemoveAsync(id));
        _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAvailableAsync_ReturnsMappedDtos()
    {
        var equipment = new List<Equipment>
        {
            new() { Id = Guid.NewGuid(), Name = "Available", LaboratoryId = Guid.NewGuid(), EquipmentCategoryId = Guid.NewGuid(), ManufacturerId = Guid.NewGuid(), IsAvailableForBooking = true },
        };
        _repoMock.Setup(r => r.GetAvailableEquipmentAsync()).ReturnsAsync(equipment);

        var result = (await _service.GetAvailableAsync()).ToList();

        Assert.Single(result);
    }

    [Fact]
    public async Task GetByLaboratoryAsync_ReturnsMappedDtos()
    {
        var labId = Guid.NewGuid();
        var equipment = new List<Equipment>
        {
            new() { Id = Guid.NewGuid(), Name = "Lab Eq", LaboratoryId = labId, EquipmentCategoryId = Guid.NewGuid(), ManufacturerId = Guid.NewGuid() },
        };
        _repoMock.Setup(r => r.GetByLaboratoryAsync(labId)).ReturnsAsync(equipment);

        var result = (await _service.GetByLaboratoryAsync(labId)).ToList();

        Assert.Single(result);
    }

    [Fact]
    public async Task GetByCategoryAsync_ReturnsMappedDtos()
    {
        var catId = Guid.NewGuid();
        var equipment = new List<Equipment>
        {
            new() { Id = Guid.NewGuid(), Name = "Cat Eq", LaboratoryId = Guid.NewGuid(), EquipmentCategoryId = catId, ManufacturerId = Guid.NewGuid() },
        };
        _repoMock.Setup(r => r.GetByCategoryAsync(catId)).ReturnsAsync(equipment);

        var result = (await _service.GetByCategoryAsync(catId)).ToList();

        Assert.Single(result);
    }
}
