using App.Domain;

namespace App.DAL.Contracts;

public interface ILaboratoryRepository : IBaseRepository<Laboratory>
{
    Task<IEnumerable<Laboratory>> AllWithDetailsAsync();
    Task<Laboratory?> FindWithDetailsAsync(Guid id);
}
