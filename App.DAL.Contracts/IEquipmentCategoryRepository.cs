using App.Domain;

namespace App.DAL.Contracts;

public interface IEquipmentCategoryRepository : IBaseRepository<EquipmentCategory>
{
    Task<IEnumerable<EquipmentCategory>> AllWithDetailsAsync();

    /// <summary>
    /// Returns only categories where RequiresTraining == true.
    /// Used to gate booking access and display certification prompts.
    /// </summary>
    Task<IEnumerable<EquipmentCategory>> GetRequiringTrainingAsync();
}
