using FluentValidation;
using WorkforceAPI.Models;

namespace WorkforceAPI.Validators;

/// <summary>
/// FluentValidation validator for Employee entities
/// </summary>
/// <remarks>
/// This validator defines all validation rules for Employee entities.
/// FluentValidation automatically applies these rules when an Employee is validated,
/// typically during model binding in API controllers.
/// 
/// Validation rules include:
/// - Required field validation (FirstName, LastName, Email, etc.)
/// - Format validation (Email format, URL format)
/// - Range validation (Salary, date ranges)
/// - Length validation (string max lengths)
/// - Business rule validation (JoiningDate cannot be in future)
/// 
/// When validation fails, the errors are automatically added to ModelState,
/// and the GlobalExceptionHandlerMiddleware returns a 400 Bad Request response.
/// </remarks>
public class EmployeeValidator : AbstractValidator<Employee>
{
    /// <summary>
    /// Initializes validation rules for Employee entity
    /// </summary>
    /// <remarks>
    /// All validation rules are defined in the constructor using FluentValidation's
    /// fluent API. Rules are evaluated in the order they are defined.
    /// </remarks>
    public EmployeeValidator()
    {
        // FirstName: Required, max 100 characters
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

        // LastName: Required, max 100 characters
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

        // Email: Required, valid email format, max 255 characters
        // EmailAddress() validates RFC 5322 email format
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid email address")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters");

        // DepartmentId: Required, cannot be empty GUID
        // Validates that a department is selected
        RuleFor(x => x.DepartmentId)
            .NotEmpty().WithMessage("Department is required")
            .NotEqual(Guid.Empty).WithMessage("Department is required");

        // DesignationId: Required, cannot be empty GUID
        // Validates that a designation is selected
        RuleFor(x => x.DesignationId)
            .NotEmpty().WithMessage("Designation is required")
            .NotEqual(Guid.Empty).WithMessage("Designation is required");

        // Salary: Must be greater than 0, max 999,999,999.99
        // Prevents negative salaries and enforces reasonable maximum
        RuleFor(x => x.Salary)
            .GreaterThan(0).WithMessage("Salary must be greater than 0")
            .LessThanOrEqualTo(999999999.99m).WithMessage("Salary must not exceed 999,999,999.99");

        // JoiningDate: Required, cannot be in the future
        // Business rule: Employees cannot join in the future
        RuleFor(x => x.JoiningDate)
            .NotEmpty().WithMessage("Joining date is required")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Joining date cannot be in the future");

        // Phone: Optional, but if provided, max 20 characters
        // When() makes the rule conditional - only validates if phone is not empty
        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        // Address: Optional, but if provided, max 500 characters
        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("Address must not exceed 500 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Address));

        // City: Optional, but if provided, max 100 characters
        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("City must not exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.City));

        // Country: Optional, but if provided, max 100 characters
        RuleFor(x => x.Country)
            .MaximumLength(100).WithMessage("Country must not exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Country));

        // AvatarUrl: Optional, but if provided, must be valid URL, max 500 characters
        // Custom validation using BeValidUrl() method
        RuleFor(x => x.AvatarUrl)
            .MaximumLength(500).WithMessage("Avatar URL must not exceed 500 characters")
            .Must(BeValidUrl).WithMessage("Avatar URL must be a valid URL")
            .When(x => !string.IsNullOrWhiteSpace(x.AvatarUrl));

        // Skills: Each skill in the list must not be empty and max 50 characters
        // RuleForEach applies the rule to each item in the collection
        RuleForEach(x => x.Skills)
            .NotEmpty().WithMessage("Skill cannot be empty")
            .MaximumLength(50).WithMessage("Each skill must not exceed 50 characters");
    }

    /// <summary>
    /// Custom validation method to check if a string is a valid URL
    /// </summary>
    /// <param name="url">The URL string to validate</param>
    /// <returns>True if the URL is valid (HTTP or HTTPS), false otherwise</returns>
    /// <remarks>
    /// This method validates that:
    /// - The string can be parsed as a URI
    /// - The URI is absolute (not relative)
    /// - The scheme is either HTTP or HTTPS
    /// 
    /// Returns true for null/empty strings (validation is optional).
    /// </remarks>
    private bool BeValidUrl(string? url)
    {
        // Empty URLs are valid (field is optional)
        if (string.IsNullOrWhiteSpace(url))
            return true;

        // Try to parse as absolute URI and check scheme
        return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}
