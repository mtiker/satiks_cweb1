using App.DAL.Contracts;
using App.Domain;

namespace App.DAL.EF.Repositories;

public class ManufacturerRepository(AppDbContext context) : EFBaseRepository<Manufacturer>(context), IManufacturerRepository
{
}
