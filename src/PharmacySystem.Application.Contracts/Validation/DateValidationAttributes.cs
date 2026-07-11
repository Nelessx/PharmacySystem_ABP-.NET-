using System;
using System.ComponentModel.DataAnnotations;

namespace PharmacySystem.Validation;

/// <summary>
/// Validates that a DateTime value is in the future. Null passes (combine with
/// [Required] to forbid null). Set <see cref="AllowToday"/> to accept today.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class FutureDateAttribute : ValidationAttribute
{
    public bool AllowToday { get; set; }

    public override bool IsValid(object? value)
    {
        if (value is null)
        {
            return true;
        }

        if (value is DateTime date)
        {
            var today = DateTime.Today;
            return AllowToday ? date.Date >= today : date.Date > today;
        }

        return false;
    }

    public override string FormatErrorMessage(string name)
        => ErrorMessage ?? $"{name} must be a future date.";
}

/// <summary>
/// Validates that a DateTime value is not in the future (today or earlier).
/// Null passes (combine with [Required] to forbid null).
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class PastOrPresentDateAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is null)
        {
            return true;
        }

        if (value is DateTime date)
        {
            return date.Date <= DateTime.Today;
        }

        return false;
    }

    public override string FormatErrorMessage(string name)
        => ErrorMessage ?? $"{name} cannot be in the future.";
}
