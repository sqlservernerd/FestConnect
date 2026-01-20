using FestGuide.Application.Dtos;

namespace FestGuide.Application.Services;

/// <summary>
/// Service interface for user management operations.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Gets a user's profile by ID.
    /// </summary>
    Task<UserProfileDto> GetProfileAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Updates a user's profile.
    /// </summary>
    Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken ct = default);

    /// <summary>
    /// Deletes a user account (GDPR erasure).
    /// </summary>
    Task DeleteAccountAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Exports user data (GDPR portability).
    /// </summary>
    Task<UserDataExportDto> ExportDataAsync(Guid userId, CancellationToken ct = default);
}
