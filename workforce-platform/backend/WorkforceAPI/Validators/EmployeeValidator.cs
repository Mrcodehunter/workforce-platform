using FluentValidation;
using WorkforceAPI.Models;

namespace WorkforceAPI.Validators;

public class EmployeeValidator : AbstractValidator<Employee>
{
    public EmployeeValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid email address")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters");

        RuleFor(x => x.DepartmentId)
            .NotEmpty().WithMessage("Department is required")
            .NotEqual(Guid.Empty).WithMessage("Department is required");

        RuleFor(x => x.DesignationId)
            .NotEmpty().WithMessage("Designation is required")
            .NotEqual(Guid.Empty).WithMessage("Designation is required");

        RuleFor(x => x.Salary)
            .GreaterThan(0).WithMessage("Salary must be greater than 0")
            .LessThanOrEqualTo(999999999.99m).WithMessage("Salary must not exceed 999,999,999.99");

        RuleFor(x => x.JoiningDate)
            .NotEmpty().WithMessage("Joining date is required")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Joining date cannot be in the future");

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("Address must not exceed 500 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Address));

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("City must not exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.City));

        RuleFor(x => x.Country)
            .MaximumLength(100).WithMessage("Country must not exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Country));

        RuleFor(x => x.AvatarUrl)
            .MaximumLength(500).WithMessage("Avatar URL must not exceed 500 characters")
            .Must(BeValidUrl).WithMessage("Avatar URL must be a valid URL")
            .When(x => !string.IsNullOrWhiteSpace(x.AvatarUrl));

        RuleForEach(x => x.Skills)
            .NotEmpty().WithMessage("Skill cannot be empty")
            .MaximumLength(50).WithMessage("Each skill must not exceed 50 characters");
    }

    private bool BeValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return true;

        return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}
