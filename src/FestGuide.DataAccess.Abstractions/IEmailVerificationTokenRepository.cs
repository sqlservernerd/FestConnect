using FestGuide.Domain.Entities;

namespace FestGuide.DataAccess.Abstractions;

/// <summary>
/// Repository interface for EmailVerificationToken data access operations.
/// </summary>
public interface IEmailVerificationTokenRepository
{
    /// <summary>
    /// Creates a new email verification token.
    /// </summary>
    Task<Guid> CreateAsync(EmailVerificationToken token, CancellationToken ct = default);

    /// <summary>
    /// Gets a token by its hash value.
    /// </summary>
    Task<EmailVerificationToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default);

    /// <summary>
    /// Gets the most recent unused token for a user.
    /// </summary>
    Task<EmailVerificationToken?> GetActiveByUserIdAsync(Guid userId, CancellationToken ct = default);

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
