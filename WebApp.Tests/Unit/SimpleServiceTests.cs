using App.BLL.Services;
using App.DAL.Contracts;
using App.Domain;
using App.DTO.v1;
using Base.Domain;
using Moq;

namespace WebApp.Tests.Unit;

// ── DepartmentService ────────────────────────────────────────────────────────

public class DepartmentServiceTests
{
    private readonly Mock<IAppUOW>              _uow  = new();
    private readonly Mock<IDepartmentRepository> _repo = new();
    private readonly DepartmentService          _svc;

    public DepartmentServiceTests()
    {
        _uow.Setup(u => u.Departments).Returns(_repo.Object);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _svc = new DepartmentService(_uow.Object);
    }

    private static Department Build(Guid id, string name = "Test Dept") =>
        new() { Id = id, Name = new LangStr(name, "en") };

    [Fact]
    public async Task AllAsync_ReturnsDtoList()
    {
        _repo.Setup(r => r.AllWithDetailsAsync())
             .ReturnsAsync(new[] { Build(Guid.NewGuid()) }.ToList());

        var result = (await _svc.AllAsync()).ToList();

        Assert.Single(result);
    }

    [Fact]
    public async Task FindAsync_NotFound_ReturnsNull()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<object[]>()))
             .ReturnsAsync((Department?)null);

        Assert.Null(await _svc.FindAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task FindAsync_Found_ReturnsDto()
    {
        var id   = Guid.NewGuid();
        var dept = Build(id);
        _repo.Setup(r => r.FindAsync(It.IsAny<object[]>())).ReturnsAsync(dept);

        var result = await _svc.FindAsync(id);

        Assert.NotNull(result);
        Assert.Equal(id, result!.Id);
    }

    [Fact]
    public async Task AddAsync_PersistsAndReturnsDto()
    {
        var dto = new DepartmentCreateDto { Name = "New Dept" };
        Department? captured = null;

        _repo.Setup(r => r.Add(It.IsAny<Department>()))
             .Callback<Department>(e => captured = e)
             .Returns<Department>(e => e);
        _repo.Setup(r => r.FindAsync(It.IsAny<object[]>()))
             .ReturnsAsync((Department?)null);

        var result = await _svc.AddAsync(dto);

        Assert.NotNull(captured);
        _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ThrowsKeyNotFound()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<object[]>())).ReturnsAsync((Department?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _svc.UpdateAsync(new DepartmentUpdateDto { Id = Guid.NewGuid(), Name = "x" }));
    }

    [Fact]
    public async Task UpdateAsync_Found_SavesAndReturnsDto()
    {
        var id   = Guid.NewGuid();
        var dept = Build(id);
        _repo.Setup(r => r.FindAsync(It.IsAny<object[]>())).ReturnsAsync(dept);
        _repo.Setup(r => r.Update(It.IsAny<Department>())).Returns(dept);

        var result = await _svc.UpdateAsync(new DepartmentUpdateDto { Id = id, Name = "Updated" });

        Assert.NotNull(result);
        _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_NotFound_ReturnsFalse()
    {
        _repo.Setup(r => r.RemoveAsync(It.IsAny<Guid>(), null)).ReturnsAsync(false);

        Assert.False(await _svc.RemoveAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task RemoveAsync_Found_ReturnsTrue()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.RemoveAsync(id, null)).ReturnsAsync(true);

        Assert.True(await _svc.RemoveAsync(id));
        _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
}

// ── LocationService ───────────────────────────────────────────────────────────

public class LocationServiceTests
{
    private readonly Mock<IAppUOW>            _uow  = new();
    private readonly Mock<ILocationRepository> _repo = new();
    private readonly LocationService          _svc;

    public LocationServiceTests()
    {
        _uow.Setup(u => u.Locations).Returns(_repo.Object);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _svc = new LocationService(_uow.Object);
    }

    private static Location Build(Guid id) =>
        new() { Id = id, BuildingName = new LangStr("Building A", "en") };

    [Fact]
    public async Task AllAsync_ReturnsMappedDtos()
    {
        _repo.Setup(r => r.AllAsync()).ReturnsAsync(new[] { Build(Guid.NewGuid()) }.ToList());

        var result = (await _svc.AllAsync()).ToList();

        Assert.Single(result);
    }

    [Fact]
    public async Task FindAsync_NotFound_ReturnsNull()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<object[]>())).ReturnsAsync((Location?)null);

        Assert.Null(await _svc.FindAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task FindAsync_Found_ReturnsDto()
    {
        var id  = Guid.NewGuid();
        var loc = Build(id);
        _repo.Setup(r => r.FindAsync(It.IsAny<object[]>())).ReturnsAsync(loc);

        var result = await _svc.FindAsync(id);

        Assert.NotNull(result);
        Assert.Equal(id, result!.Id);
    }

    [Fact]
    public async Task AddAsync_PersistsEntity()
    {
        var dto = new LocationCreateDto { BuildingName = "New Building" };
        Location? captured = null;

        _repo.Setup(r => r.Add(It.IsAny<Location>()))
             .Callback<Location>(e => captured = e)
             .Returns<Location>(e => e);
        _repo.Setup(r => r.FindAsync(It.IsAny<object[]>())).ReturnsAsync((Location?)null);

        await _svc.AddAsync(dto);

        Assert.NotNull(captured);
        _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ThrowsKeyNotFound()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<object[]>())).ReturnsAsync((Location?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _svc.UpdateAsync(new LocationUpdateDto { Id = Guid.NewGuid(), BuildingName = "x" }));
    }

    [Fact]
    public async Task UpdateAsync_Found_SavesAndReturnsDto()
    {
        var id  = Guid.NewGuid();
        var loc = Build(id);
        _repo.Setup(r => r.FindAsync(It.IsAny<object[]>())).ReturnsAsync(loc);
        _repo.Setup(r => r.Update(It.IsAny<Location>())).Returns(loc);

        var result = await _svc.UpdateAsync(new LocationUpdateDto { Id = id, BuildingName = "Updated" });

        Assert.NotNull(result);
        _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_NotFound_ReturnsFalse()
    {
        _repo.Setup(r => r.RemoveAsync(It.IsAny<Guid>(), null)).ReturnsAsync(false);

        Assert.False(await _svc.RemoveAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task RemoveAsync_Found_ReturnsTrue()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.RemoveAsync(id, null)).ReturnsAsync(true);

        Assert.True(await _svc.RemoveAsync(id));
        _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
}

// ── LaboratoryService ─────────────────────────────────────────────────────────

public class LaboratoryServiceTests
{
    private readonly Mock<IAppUOW>              _uow  = new();
    private readonly Mock<ILaboratoryRepository> _repo = new();
    private readonly LaboratoryService          _svc;

    public LaboratoryServiceTests()
    {
        _uow.Setup(u => u.Laboratories).Returns(_repo.Object);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _svc = new LaboratoryService(_uow.Object);
    }

    private static Laboratory Build(Guid id) =>
        new() { Id = id, Name = "Lab 1" };

    [Fact]
    public async Task AllAsync_ReturnsMappedDtos()
    {
        _repo.Setup(r => r.AllWithDetailsAsync()).ReturnsAsync(new[] { Build(Guid.NewGuid()) }.ToList());

        Assert.Single(await _svc.AllAsync());
    }

    [Fact]
    public async Task FindAsync_NotFound_ReturnsNull()
    {
        _repo.Setup(r => r.FindWithDetailsAsync(It.IsAny<Guid>())).ReturnsAsync((Laboratory?)null);

        Assert.Null(await _svc.FindAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task FindAsync_Found_ReturnsDto()
    {
        var id  = Guid.NewGuid();
        var lab = Build(id);
        _repo.Setup(r => r.FindWithDetailsAsync(id)).ReturnsAsync(lab);

        var result = await _svc.FindAsync(id);

        Assert.NotNull(result);
        Assert.Equal(id, result!.Id);
    }

    [Fact]
    public async Task AddAsync_PersistsEntity()
    {
        var dto = new LaboratoryCreateDto { Name = "New Lab", DepartmentId = Guid.NewGuid(), LocationId = Guid.NewGuid() };
        Laboratory? captured = null;

        _repo.Setup(r => r.Add(It.IsAny<Laboratory>()))
             .Callback<Laboratory>(e => captured = e)
             .Returns<Laboratory>(e => e);
        _repo.Setup(r => r.FindWithDetailsAsync(It.IsAny<Guid>())).ReturnsAsync((Laboratory?)null);

        await _svc.AddAsync(dto);

        Assert.NotNull(captured);
        _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ThrowsKeyNotFound()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<object[]>())).ReturnsAsync((Laboratory?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _svc.UpdateAsync(new LaboratoryUpdateDto { Id = Guid.NewGuid(), Name = "x", DepartmentId = Guid.NewGuid(), LocationId = Guid.NewGuid() }));
    }

    [Fact]
    public async Task UpdateAsync_Found_Succeeds()
    {
        var id  = Guid.NewGuid();
        var lab = Build(id);
        _repo.Setup(r => r.FindAsync(It.IsAny<object[]>())).ReturnsAsync(lab);
        _repo.Setup(r => r.FindWithDetailsAsync(id)).ReturnsAsync(lab);
        _repo.Setup(r => r.Update(It.IsAny<Laboratory>())).Returns(lab);

        var result = await _svc.UpdateAsync(new LaboratoryUpdateDto { Id = id, Name = "Updated", DepartmentId = Guid.NewGuid(), LocationId = Guid.NewGuid() });

        Assert.NotNull(result);
        _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_NotFound_ReturnsFalse()
    {
        _repo.Setup(r => r.RemoveAsync(It.IsAny<Guid>(), null)).ReturnsAsync(false);

        Assert.False(await _svc.RemoveAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task RemoveAsync_Found_ReturnsTrue()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.RemoveAsync(id, null)).ReturnsAsync(true);

        Assert.True(await _svc.RemoveAsync(id));
    }
}

// ── ManufacturerService ───────────────────────────────────────────────────────

public class ManufacturerServiceTests
{
    private readonly Mock<IAppUOW>               _uow  = new();
    private readonly Mock<IManufacturerRepository> _repo = new();
    private readonly ManufacturerService         _svc;

    public ManufacturerServiceTests()
    {
        _uow.Setup(u => u.Manufacturers).Returns(_repo.Object);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _svc = new ManufacturerService(_uow.Object);
    }

    private static Manufacturer Build(Guid id, string name = "Acme") =>
        new() { Id = id, Name = name };

    [Fact]
    public async Task AllAsync_ReturnsMappedDtos()
    {
        _repo.Setup(r => r.AllAsync()).ReturnsAsync(new[] { Build(Guid.NewGuid()) }.ToList());

        Assert.Single(await _svc.AllAsync());
    }

    [Fact]
    public async Task FindAsync_NotFound_ReturnsNull()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<object[]>())).ReturnsAsync((Manufacturer?)null);

        Assert.Null(await _svc.FindAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task FindAsync_Found_ReturnsDto()
    {
        var id  = Guid.NewGuid();
        var mfg = Build(id, "Keysight");
        _repo.Setup(r => r.FindAsync(It.IsAny<object[]>())).ReturnsAsync(mfg);

        var result = await _svc.FindAsync(id);

        Assert.NotNull(result);
        Assert.Equal("Keysight", result!.Name);
    }

    [Fact]
    public async Task AddAsync_PersistsEntity()
    {
        var dto = new ManufacturerCreateDto { Name = "New Mfg" };
        Manufacturer? captured = null;

        _repo.Setup(r => r.Add(It.IsAny<Manufacturer>()))
             .Callback<Manufacturer>(e => captured = e)
             .Returns<Manufacturer>(e => e);
        _repo.Setup(r => r.FindAsync(It.IsAny<object[]>())).ReturnsAsync((Manufacturer?)null);

        await _svc.AddAsync(dto);

        Assert.NotNull(captured);
        Assert.Equal("New Mfg", captured!.Name);
        _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ThrowsKeyNotFound()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<object[]>())).ReturnsAsync((Manufacturer?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _svc.UpdateAsync(new ManufacturerUpdateDto { Id = Guid.NewGuid(), Name = "x" }));
    }

    [Fact]
    public async Task UpdateAsync_Found_Succeeds()
    {
        var id  = Guid.NewGuid();
        var mfg = Build(id);
        _repo.Setup(r => r.FindAsync(It.IsAny<object[]>())).ReturnsAsync(mfg);
        _repo.Setup(r => r.Update(It.IsAny<Manufacturer>())).Returns(mfg);

        var result = await _svc.UpdateAsync(new ManufacturerUpdateDto { Id = id, Name = "Updated" });

        Assert.NotNull(result);
        _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_NotFound_ReturnsFalse()
    {
        _repo.Setup(r => r.RemoveAsync(It.IsAny<Guid>(), null)).ReturnsAsync(false);

        Assert.False(await _svc.RemoveAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task RemoveAsync_Found_ReturnsTrue()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.RemoveAsync(id, null)).ReturnsAsync(true);

        Assert.True(await _svc.RemoveAsync(id));
        _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
}

// ── EquipmentCategoryService ──────────────────────────────────────────────────

public class EquipmentCategoryServiceTests
{
    private readonly Mock<IAppUOW>                    _uow  = new();
    private readonly Mock<IEquipmentCategoryRepository> _repo = new();
    private readonly EquipmentCategoryService         _svc;

    public EquipmentCategoryServiceTests()
    {
        _uow.Setup(u => u.EquipmentCategories).Returns(_repo.Object);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _svc = new EquipmentCategoryService(_uow.Object);
    }

    private static EquipmentCategory Build(Guid id, bool requiresTraining = false) =>
        new() { Id = id, Name = new LangStr("Category", "en"), RequiresTraining = requiresTraining };

    [Fact]
    public async Task AllAsync_ReturnsMappedDtos()
    {
        _repo.Setup(r => r.AllWithDetailsAsync()).ReturnsAsync(new[] { Build(Guid.NewGuid()) }.ToList());

        Assert.Single(await _svc.AllAsync());
    }

    [Fact]
    public async Task FindAsync_NotFound_ReturnsNull()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<object[]>())).ReturnsAsync((EquipmentCategory?)null);

        Assert.Null(await _svc.FindAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task FindAsync_Found_ReturnsDto()
    {
        var id  = Guid.NewGuid();
        var cat = Build(id, requiresTraining: true);
        _repo.Setup(r => r.FindAsync(It.IsAny<object[]>())).ReturnsAsync(cat);

        var result = await _svc.FindAsync(id);

        Assert.NotNull(result);
        Assert.Equal(id, result!.Id);
        Assert.True(result.RequiresTraining);
    }

    [Fact]
    public async Task AddAsync_PersistsEntity()
    {
        var dto = new EquipmentCategoryCreateDto { Name = "New Cat", RequiresTraining = true };
        EquipmentCategory? captured = null;

        _repo.Setup(r => r.Add(It.IsAny<EquipmentCategory>()))
             .Callback<EquipmentCategory>(e => captured = e)
             .Returns<EquipmentCategory>(e => e);
        _repo.Setup(r => r.FindAsync(It.IsAny<object[]>())).ReturnsAsync((EquipmentCategory?)null);

        await _svc.AddAsync(dto);

        Assert.NotNull(captured);
        _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ThrowsKeyNotFound()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<object[]>())).ReturnsAsync((EquipmentCategory?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _svc.UpdateAsync(new EquipmentCategoryUpdateDto { Id = Guid.NewGuid(), Name = "x" }));
    }

    [Fact]
    public async Task UpdateAsync_Found_Succeeds()
    {
        var id  = Guid.NewGuid();
        var cat = Build(id);
        _repo.Setup(r => r.FindAsync(It.IsAny<object[]>())).ReturnsAsync(cat);
        _repo.Setup(r => r.FindAsync(It.IsAny<object[]>())).ReturnsAsync(cat);
        _repo.Setup(r => r.Update(It.IsAny<EquipmentCategory>())).Returns(cat);

        var result = await _svc.UpdateAsync(new EquipmentCategoryUpdateDto { Id = id, Name = "Updated" });

        Assert.NotNull(result);
        _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_NotFound_ReturnsFalse()
    {
        _repo.Setup(r => r.RemoveAsync(It.IsAny<Guid>(), null)).ReturnsAsync(false);

        Assert.False(await _svc.RemoveAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task RemoveAsync_Found_ReturnsTrue()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.RemoveAsync(id, null)).ReturnsAsync(true);

        Assert.True(await _svc.RemoveAsync(id));
        _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetRequiringTrainingAsync_ReturnsOnlyTrainingCategories()
    {
        var cat = Build(Guid.NewGuid(), requiresTraining: true);
        _repo.Setup(r => r.GetRequiringTrainingAsync()).ReturnsAsync(new[] { cat }.ToList());

        var result = (await _svc.GetRequiringTrainingAsync()).ToList();

        Assert.Single(result);
        Assert.True(result[0].RequiresTraining);
    }
}
