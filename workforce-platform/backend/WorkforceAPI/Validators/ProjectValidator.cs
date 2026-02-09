using FluentValidation;
using WorkforceAPI.Models;

namespace WorkforceAPI.Validators;

/// <summary>
/// FluentValidation validator for Project entities
/// </summary>
/// <remarks>
/// This validator defines all validation rules for Project entities.
/// FluentValidation automatically applies these rules when a Project is validated,
/// typically during model binding in API controllers.
/// 
/// Validation rules include:
/// - Required field validation (Name, Status, StartDate)
/// - Format validation (Status must be valid)
/// - Date validation (StartDate cannot be more than 1 year in future, EndDate must be after StartDate)
/// - Length validation (Name max 200, Description max 1000)
/// </remarks>
public class ProjectValidator : AbstractValidator<Project>
{
    /// <summary>
    /// Initializes validation rules for Project entity
    /// </summary>
    public ProjectValidator()
    {
        // Name: Required, max 200 characters
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Project name is required")
            .MaximumLength(200).WithMessage("Project name must not exceed 200 characters");

        // Description: Optional, but if provided, max 1000 characters
        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        // Status: Required, must be one of the valid statuses
        // Valid values: "Planning", "Active", "OnHold", "Completed", "Cancelled"
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .Must(BeValidStatus).WithMessage("Status must be one of: Planning, Active, OnHold, Completed, Cancelled");

        // StartDate: Required, cannot be more than 1 year in the future
        // Business rule: Projects cannot be planned too far in advance
        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required")
            .LessThanOrEqualTo(DateTime.UtcNow.AddYears(1)).WithMessage("Start date cannot be more than 1 year in the future");

        // EndDate: Optional, but if provided, must be after StartDate
        // Business rule: Projects cannot end before they start
        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date")
            .When(x => x.EndDate.HasValue);
    }

    /// <summary>
    /// Custom validation method to check if status is valid
    /// </summary>
    /// <param name="status">The status string to validate</param>
    /// <returns>True if status is valid, false otherwise</returns>
    /// <remarks>
    /// Valid statuses: "Planning", "Active", "OnHold", "Completed", "Cancelled"
    /// </remarks>
    private bool BeValidStatus(string status)
    {
        var validStatuses = new[] { "Planning", "Active", "OnHold", "Completed", "Cancelled" };
        return validStatuses.Contains(status);
    }
}
