using FestGuide.Domain.Entities;

namespace FestGuide.DataAccess.Abstractions;

/// <summary>
/// Repository interface for PasswordResetToken data access operations.
/// </summary>
public interface IPasswordResetTokenRepository
{
    /// <summary>
    /// Creates a new password reset token.
    /// </summary>
    Task<Guid> CreateAsync(PasswordResetToken token, CancellationToken ct = default);

    /// <summary>
    /// Gets a token by its hash value.
    /// </summary>
    Task<PasswordResetToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default);

    /// <summary>
    /// Gets the most recent unused token for a user.
    /// </summary>
    Task<PasswordResetToken?> GetActiveByUserIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Marks a token as used.
    /// </summary>
    Task MarkAsUsedAsync(Guid tokenId, CancellationToken ct = default);

    /// <summary>
    /// Invalidates all unused tokens for a user.
    /// </summary>
    Task InvalidateAllForUserAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Deletes expired tokens older than the specified date.
    /// </summary>
    Task DeleteExpiredAsync(DateTime olderThan, CancellationToken ct = default);
}
