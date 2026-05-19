using App.Domain.Identity;

namespace App.DAL.Contracts;

public interface IAppRefreshTokenRepository : IBaseRepository<AppRefreshToken>
{
    /// <summary>Returns all refresh tokens belonging to a user.</summary>
    Task<IEnumerable<AppRefreshToken>> GetByUserAsync(Guid appUserId);

    /// <summary>
    /// Finds an active token by its value or its previous token value.
    /// Used during token refresh to handle the one-time previous-token grace window.
    /// </summary>
    Task<AppRefreshToken?> FindByTokenValueAsync(string refreshToken);

    /// <summary>Removes all expired tokens for a user — call after successful refresh.</summary>
    Task RemoveExpiredAsync(Guid appUserId);
}
