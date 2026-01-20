using FluentValidation;
using FestGuide.Application.Dtos;

namespace FestGuide.Application.Validators;

/// <summary>
/// Validator for profile update requests.
/// </summary>
public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    private static readonly HashSet<string> ValidTimezones = new(TimeZoneInfo.GetSystemTimeZones().Select(tz => tz.Id));

    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.DisplayName)
            .MinimumLength(2).WithMessage("Display name must be at least 2 characters long.")
            .MaximumLength(100).WithMessage("Display name must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.DisplayName));

        RuleFor(x => x.PreferredTimezoneId)
            .Must(BeValidTimezone).WithMessage("Invalid timezone identifier.")
            .When(x => !string.IsNullOrEmpty(x.PreferredTimezoneId));
    }

    private static bool BeValidTimezone(string? timezoneId)
    {
        if (string.IsNullOrEmpty(timezoneId))
            return true;

        return ValidTimezones.Contains(timezoneId);
    }
}
