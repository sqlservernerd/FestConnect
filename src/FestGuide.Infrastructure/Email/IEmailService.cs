namespace FestGuide.Infrastructure.Email;

/// <summary>
/// Service interface for sending emails.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email verification link to the user.
    /// </summary>
    /// <param name="email">The recipient's email address.</param>
    /// <param name="displayName">The user's display name.</param>
    /// <param name="verificationToken">The verification token.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SendVerificationEmailAsync(string email, string displayName, string verificationToken, CancellationToken ct = default);

    /// <summary>
    /// Sends a password reset link to the user.
    /// </summary>
    /// <param name="email">The recipient's email address.</param>
    /// <param name="displayName">The user's display name.</param>
    /// <param name="resetToken">The password reset token.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SendPasswordResetEmailAsync(string email, string displayName, string resetToken, CancellationToken ct = default);

    /// <summary>
    /// Sends a notification that the user's password has been changed.
    /// </summary>
    /// <param name="email">The recipient's email address.</param>
    /// <param name="displayName">The user's display name.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SendPasswordChangedNotificationAsync(string email, string displayName, CancellationToken ct = default);
}
