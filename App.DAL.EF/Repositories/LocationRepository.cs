using App.DAL.Contracts;
using App.Domain;

namespace App.DAL.EF.Repositories;

public class LocationRepository(AppDbContext context) : EFBaseRepository<Location>(context), ILocationRepository
{
}
