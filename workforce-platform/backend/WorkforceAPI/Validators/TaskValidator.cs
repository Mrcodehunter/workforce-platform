using FluentValidation;
using WorkforceAPI.Models;

namespace WorkforceAPI.Validators;

public class TaskValidator : AbstractValidator<TaskItem>
{
    public TaskValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Task title is required")
            .MaximumLength(200).WithMessage("Task title must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project is required")
            .NotEqual(Guid.Empty).WithMessage("Project is required");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .Must(BeValidStatus).WithMessage("Status must be one of: ToDo, InProgress, InReview, Done, Cancelled");

        RuleFor(x => x.Priority)
            .InclusiveBetween(0, 3).WithMessage("Priority must be between 0 (Low) and 3 (Critical)");

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Due date must be in the future")
            .When(x => x.DueDate.HasValue);
    }

    private bool BeValidStatus(string status)
    {
        var validStatuses = new[] { "ToDo", "InProgress", "InReview", "Done", "Cancelled" };
        return validStatuses.Contains(status);
    }
}
