using FluentValidation;
using WorkforceAPI.Models;

namespace WorkforceAPI.Validators;

/// <summary>
/// FluentValidation validator for TaskItem entities
/// </summary>
/// <remarks>
/// This validator defines all validation rules for TaskItem entities.
/// FluentValidation automatically applies these rules when a TaskItem is validated,
/// typically during model binding in API controllers.
/// 
/// Validation rules include:
/// - Required field validation (Title, ProjectId)
/// - Format validation (Status must be valid)
/// - Range validation (Priority 0-3, DueDate must be in future)
/// - Length validation (Title max 200, Description max 1000)
/// </remarks>
public class TaskValidator : AbstractValidator<TaskItem>
{
    /// <summary>
    /// Initializes validation rules for TaskItem entity
    /// </summary>
    public TaskValidator()
    {
        // Title: Required, max 200 characters
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Task title is required")
            .MaximumLength(200).WithMessage("Task title must not exceed 200 characters");

        // Description: Optional, but if provided, max 1000 characters
        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        // ProjectId: Required, cannot be empty GUID
        // Validates that a project is selected
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project is required")
            .NotEqual(Guid.Empty).WithMessage("Project is required");

        // Status: Must be one of the valid statuses
        // Valid values: "ToDo", "InProgress", "InReview", "Done", "Cancelled"
        RuleFor(x => x.Status)
            .Must(BeValidStatus).WithMessage("Status must be one of: ToDo, InProgress, InReview, Done, Cancelled")
            .When(x => !string.IsNullOrWhiteSpace(x.Status));

        // Priority: Must be between 0 (Low) and 3 (Critical)
        RuleFor(x => x.Priority)
            .InclusiveBetween(0, 3).WithMessage("Priority must be between 0 (Low) and 3 (Critical)");

        // DueDate: Optional, but if provided, must be in the future
        // Business rule: Tasks cannot have past due dates
        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Due date must be in the future")
            .When(x => x.DueDate.HasValue);
    }

    /// <summary>
    /// Custom validation method to check if status is valid
    /// </summary>
    /// <param name="status">The status string to validate</param>
    /// <returns>True if status is valid, false otherwise</returns>
    /// <remarks>
    /// Valid statuses: "ToDo", "InProgress", "InReview", "Done", "Cancelled"
    /// </remarks>
    private bool BeValidStatus(string status)
    {
        var validStatuses = new[] { "ToDo", "InProgress", "InReview", "Done", "Cancelled" };
        return validStatuses.Contains(status);
    }
}
