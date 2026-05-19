using App.DAL.Contracts;
using App.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.DAL.EF.Repositories;

public class AppRefreshTokenRepository(AppDbContext context) : EFBaseRepository<AppRefreshToken>(context), IAppRefreshTokenRepository
{
    public async Task<IEnumerable<AppRefreshToken>> GetByUserAsync(Guid appUserId)
        => await RepositoryDbSet
            .Where(t => t.AppUserId == appUserId)
            .ToListAsync();

    // Matches on current token value or the previous-token grace window value
    public async Task<AppRefreshToken?> FindByTokenValueAsync(string refreshToken)
        => await RepositoryDbSet.FirstOrDefaultAsync(t =>
            t.RefreshToken == refreshToken ||
            t.PreviousRefreshToken == refreshToken);

    public async Task RemoveExpiredAsync(Guid appUserId)
    {
        var expired = await RepositoryDbSet
            .Where(t => t.AppUserId == appUserId && t.ExpirationDT < DateTime.UtcNow)
            .ToListAsync();
        RepositoryDbSet.RemoveRange(expired);
    }
}
