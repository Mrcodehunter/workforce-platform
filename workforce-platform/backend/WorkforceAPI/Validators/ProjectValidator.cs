using FluentValidation;
using WorkforceAPI.Models;

namespace WorkforceAPI.Validators;

public class ProjectValidator : AbstractValidator<Project>
{
    public ProjectValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Project name is required")
            .MaximumLength(200).WithMessage("Project name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .Must(BeValidStatus).WithMessage("Status must be one of: Planning, Active, OnHold, Completed, Cancelled");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required")
            .LessThanOrEqualTo(DateTime.UtcNow.AddYears(1)).WithMessage("Start date cannot be more than 1 year in the future");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date")
            .When(x => x.EndDate.HasValue);
    }

    private bool BeValidStatus(string status)
    {
        var validStatuses = new[] { "Planning", "Active", "OnHold", "Completed", "Cancelled" };
        return validStatuses.Contains(status);
    }
}
