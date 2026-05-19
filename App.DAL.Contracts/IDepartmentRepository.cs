using App.Domain;

namespace App.DAL.Contracts;

public interface IDepartmentRepository : IBaseRepository<Department>
{
    Task<IEnumerable<Department>> AllWithDetailsAsync();
}
