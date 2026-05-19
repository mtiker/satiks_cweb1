using App.BLL.Contracts;
using App.BLL.Services;
using App.DAL.Contracts;
using App.Domain;
using App.Domain.Identity;
using App.DTO.v1;
using Base.Domain;
using Moq;

namespace WebApp.Tests.Unit;

public class TrainingCertificationServiceTests
{
    private readonly Mock<IAppUOW>                       _uowMock            = new();
    private readonly Mock<ITrainingCertificationRepository> _certRepoMock      = new();
    private readonly TrainingCertificationService        _service;

    public TrainingCertificationServiceTests()
    {
        _uowMock.Setup(u => u.TrainingCertifications)
                .Returns(_certRepoMock.Object);
        _uowMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);
        _service = new TrainingCertificationService(_uowMock.Object);
    }

    private static TrainingCertification BuildCert(
        Guid id, Guid appUserId, TrainingCertificationStatus status = TrainingCertificationStatus.Pending)
        => new()
        {
            Id              = id,
            AppUserId       = appUserId,
            Status          = status,
            CertifiedDate   = DateTime.UtcNow,
            EquipmentCategory = new EquipmentCategory
            {
                Id                = Guid.NewGuid(),
                Name              = new LangStr("Cat", "en"),
                RequiresTraining  = false,
            },
            AppUser = new AppUser
            {
                Id        = appUserId,
                FirstName = "John",
                LastName  = "Doe",
            },
        };

    [Fact]
    public async Task AllAsync_ReturnsOnlyCurrentUsersRecords()
    {
        var userId1   = Guid.NewGuid();
        var userId2   = Guid.NewGuid();
        var certId1   = Guid.NewGuid();
        var certId2   = Guid.NewGuid();
        var certId3   = Guid.NewGuid();

        var allCerts = new[]
        {
            BuildCert(certId1, userId1),
            BuildCert(certId2, userId1),
            BuildCert(certId3, userId2),
        };

        _certRepoMock
            .Setup(r => r.AllWithDetailsAsync(userId1))
            .ReturnsAsync(allCerts.Where(c => c.AppUserId == userId1).ToList());

        var result = (await _service.AllAsync(userId1)).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, dto => Assert.Equal(userId1, dto.AppUserId));
    }

    [Fact]
    public async Task FindAsync_WrongUser_ReturnsNull()
    {
        var userId1         = Guid.NewGuid();
        var userId2         = Guid.NewGuid();
        var certId          = Guid.NewGuid();
        var certForUser1    = BuildCert(certId, userId1);

        _certRepoMock
            .Setup(r => r.FindWithDetailsAsync(certId, userId2))
            .ReturnsAsync((TrainingCertification?)null);

        var result = await _service.FindAsync(certId, userId2);

        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_SetsAppUserIdFromParameter()
    {
        var userId          = Guid.NewGuid();
        var dto             = new TrainingCertificationCreateDto
        {
            CertifiedDate         = DateTime.UtcNow,
            EquipmentCategoryId   = Guid.NewGuid(),
        };

        // Capture what entity is passed to repo.Add() and return it from FindWithDetailsAsync
        TrainingCertification? capturedEntity = null;
        _certRepoMock
            .Setup(r => r.Add(It.IsAny<TrainingCertification>()))
            .Callback<TrainingCertification>(e => capturedEntity = e)
            .Returns<TrainingCertification>(e => e);

        _certRepoMock
            .Setup(r => r.FindWithDetailsAsync(It.IsAny<Guid>(), null))
            .ReturnsAsync((TrainingCertification?)null);

        await _service.AddAsync(dto, userId);

        Assert.NotNull(capturedEntity);
        Assert.Equal(userId, capturedEntity!.AppUserId);
    }

    [Fact]
    public async Task RemoveAsync_WrongUser_ReturnsFalse()
    {
        var recordId   = Guid.NewGuid();
        var userId1    = Guid.NewGuid();
        var userId2    = Guid.NewGuid();

        var record    = BuildCert(recordId, userId1);

        _certRepoMock
            .Setup(r => r.RemoveAsync(recordId, userId2))
            .ReturnsAsync(false);

        var result = await _service.RemoveAsync(recordId, userId2);

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateStatusAsync_ValidTransition_Succeeds()
    {
        var recordId       = Guid.NewGuid();
        var adminUserId    = Guid.NewGuid();
        var cert           = BuildCert(recordId, Guid.NewGuid(), TrainingCertificationStatus.Pending);

        _certRepoMock
            .Setup(r => r.FindAsync(recordId))
            .ReturnsAsync(cert);

        _certRepoMock
            .Setup(r => r.FindWithDetailsAsync(recordId, null))
            .ReturnsAsync(cert);

        var result = await _service.UpdateStatusAsync(
            new TrainingCertificationAdminUpdateDto { Id = recordId, Status = "Approved" },
            adminUserId);

        Assert.NotNull(result);
        Assert.Equal("Approved", result.Status);
    }

    [Fact]
    public async Task AllAsync_NoFilter_ReturnsAllRecords()
    {
        var certs = new[]
        {
            BuildCert(Guid.NewGuid(), Guid.NewGuid()),
            BuildCert(Guid.NewGuid(), Guid.NewGuid()),
        };
        _certRepoMock.Setup(r => r.AllWithDetailsAsync(null)).ReturnsAsync(certs.ToList());

        var result = (await _service.AllAsync()).ToList();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task UpdateAsync_NotPending_ThrowsInvalidOperation()
    {
        var id     = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var cert   = BuildCert(id, userId, TrainingCertificationStatus.Approved);
        _certRepoMock.Setup(r => r.FindAsync(id)).ReturnsAsync(cert);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.UpdateAsync(
                new TrainingCertificationUserUpdateDto { Id = id, CertifiedDate = DateTime.UtcNow },
                userId));
    }

    [Fact]
    public async Task UpdateAsync_WrongUser_ThrowsUnauthorized()
    {
        var id      = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var cert    = BuildCert(id, ownerId, TrainingCertificationStatus.Pending);
        _certRepoMock.Setup(r => r.FindAsync(id)).ReturnsAsync(cert);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.UpdateAsync(
                new TrainingCertificationUserUpdateDto { Id = id, CertifiedDate = DateTime.UtcNow },
                Guid.NewGuid())); // wrong user
    }

    [Fact]
    public async Task UpdateAsync_OwnerPending_Succeeds()
    {
        var id     = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var cert   = BuildCert(id, userId, TrainingCertificationStatus.Pending);
        _certRepoMock.Setup(r => r.FindAsync(id)).ReturnsAsync(cert);
        _certRepoMock.Setup(r => r.FindWithDetailsAsync(id, null)).ReturnsAsync(cert);
        _certRepoMock.Setup(r => r.Update(It.IsAny<TrainingCertification>())).Returns(cert);

        var result = await _service.UpdateAsync(
            new TrainingCertificationUserUpdateDto { Id = id, CertifiedDate = DateTime.UtcNow },
            userId);

        Assert.NotNull(result);
        _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetByStatusAsync_ReturnsMatchingRecords()
    {
        var certs = new[] { BuildCert(Guid.NewGuid(), Guid.NewGuid(), TrainingCertificationStatus.Approved) };
        _certRepoMock.Setup(r => r.GetByStatusAsync(TrainingCertificationStatus.Approved))
                     .ReturnsAsync(certs.ToList());

        var result = (await _service.GetByStatusAsync("Approved")).ToList();

        Assert.Single(result);
    }

    [Fact]
    public async Task UserHasApprovedCertificationAsync_DelegatesToRepo()
    {
        var userId   = Guid.NewGuid();
        var category = Guid.NewGuid();
        _certRepoMock.Setup(r => r.UserHasApprovedCertificationAsync(userId, category))
                     .ReturnsAsync(true);

        Assert.True(await _service.UserHasApprovedCertificationAsync(userId, category));
    }

    [Fact]
    public async Task RemoveAsync_Found_ReturnsTrue()
    {
        var id = Guid.NewGuid();
        _certRepoMock.Setup(r => r.RemoveAsync(id, null)).ReturnsAsync(true);

        Assert.True(await _service.RemoveAsync(id));
        _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
}
