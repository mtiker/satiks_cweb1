using App.DAL.Contracts;
using App.DAL.EF.Repositories;

namespace App.DAL.EF;

public class AppUOW(AppDbContext dataContext) : EFBaseUOW<AppDbContext>(dataContext), IAppUOW
{
    private IEquipmentRepository? _equipment;
    private IBookingRepository? _bookings;
    private ILaboratoryRepository? _laboratories;
    private IDepartmentRepository? _departments;
    private ILocationRepository? _locations;
    private IEquipmentCategoryRepository? _equipmentCategories;
    private IManufacturerRepository? _manufacturers;
    private IMaintenanceRecordRepository? _maintenanceRecords;
    private ICalibrationRecordRepository? _calibrationRecords;
    private ITrainingCertificationRepository? _trainingCertifications;
    private IAppRefreshTokenRepository? _refreshTokens;

    public IEquipmentRepository Equipment
        => _equipment ??= new EquipmentRepository(UowDbContext);
    public IBookingRepository Bookings
        => _bookings ??= new BookingRepository(UowDbContext);
    public ILaboratoryRepository Laboratories
        => _laboratories ??= new LaboratoryRepository(UowDbContext);
    public IDepartmentRepository Departments
        => _departments ??= new DepartmentRepository(UowDbContext);
    public ILocationRepository Locations
        => _locations ??= new LocationRepository(UowDbContext);
    public IEquipmentCategoryRepository EquipmentCategories
        => _equipmentCategories ??= new EquipmentCategoryRepository(UowDbContext);
    public IManufacturerRepository Manufacturers
        => _manufacturers ??= new ManufacturerRepository(UowDbContext);
    public IMaintenanceRecordRepository MaintenanceRecords
        => _maintenanceRecords ??= new MaintenanceRecordRepository(UowDbContext);
    public ICalibrationRecordRepository CalibrationRecords
        => _calibrationRecords ??= new CalibrationRecordRepository(UowDbContext);
    public ITrainingCertificationRepository TrainingCertifications
        => _trainingCertifications ??= new TrainingCertificationRepository(UowDbContext);
    public IAppRefreshTokenRepository RefreshTokens
        => _refreshTokens ??= new AppRefreshTokenRepository(UowDbContext);
}
