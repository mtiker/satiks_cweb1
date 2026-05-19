namespace App.DAL.Contracts;

public interface IAppUOW : IBaseUOW
{
    IEquipmentRepository Equipment { get; }
    IBookingRepository Bookings { get; }
    ILaboratoryRepository Laboratories { get; }
    IDepartmentRepository Departments { get; }
    ILocationRepository Locations { get; }
    IEquipmentCategoryRepository EquipmentCategories { get; }
    IManufacturerRepository Manufacturers { get; }
    IMaintenanceRecordRepository MaintenanceRecords { get; }
    ICalibrationRecordRepository CalibrationRecords { get; }
    ITrainingCertificationRepository TrainingCertifications { get; }
    IAppRefreshTokenRepository RefreshTokens { get; }
}
