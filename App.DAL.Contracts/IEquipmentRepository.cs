using App.Domain;

namespace App.DAL.Contracts;

public interface IEquipmentRepository : IBaseRepository<Equipment>
{
    Task<IEnumerable<Equipment>> AllWithDetailsAsync();
    Task<Equipment?> FindWithDetailsAsync(Guid id);

    /// <summary>Returns equipment where IsAvailableForBooking == true.</summary>
    Task<IEnumerable<Equipment>> GetAvailableEquipmentAsync();

    Task<IEnumerable<Equipment>> GetByLaboratoryAsync(Guid laboratoryId);
    Task<IEnumerable<Equipment>> GetByCategoryAsync(Guid categoryId);

    /// <summary>
    /// Filters by EquipmentCondition (e.g. show only Good/New, hide Decommissioned).
    /// Useful for admin dashboards and maintenance scheduling.
    /// </summary>
    Task<IEnumerable<Equipment>> GetByConditionAsync(EquipmentCondition condition);
}
