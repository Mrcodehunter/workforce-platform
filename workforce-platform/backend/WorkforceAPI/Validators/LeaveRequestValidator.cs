using FluentValidation;
using WorkforceAPI.Models.MongoDB;

namespace WorkforceAPI.Validators;

/// <summary>
/// FluentValidation validator for LeaveRequest entities (MongoDB model)
/// </summary>
/// <remarks>
/// This validator defines all validation rules for LeaveRequest entities.
/// FluentValidation automatically applies these rules when a LeaveRequest is validated,
/// typically during model binding in API controllers.
/// 
/// Validation rules include:
/// - Required field validation (EmployeeId, EmployeeName, LeaveType, StartDate, EndDate)
/// - Format validation (LeaveType and Status must be valid)
/// - Date validation (StartDate must be before/equal to EndDate)
/// - Length validation (EmployeeName max 200, Reason max 1000)
/// </remarks>
public class LeaveRequestValidator : AbstractValidator<LeaveRequest>
{
    /// <summary>
    /// Valid leave types that can be requested
    /// </summary>
    private static readonly string[] ValidLeaveTypes = { "Sick", "Casual", "Annual", "Unpaid" };
    
    /// <summary>
    /// Valid leave request statuses
    /// </summary>
    private static readonly string[] ValidStatuses = { "Pending", "Approved", "Rejected", "Cancelled" };

    /// <summary>
    /// Initializes validation rules for LeaveRequest entity
    /// </summary>
    public LeaveRequestValidator()
    {
        // EmployeeId: Required, cannot be empty GUID
        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("Employee ID is required")
            .NotEqual(Guid.Empty).WithMessage("Employee ID is required");

        // EmployeeName: Required, max 200 characters
        // Stored denormalized in MongoDB for easier querying
        RuleFor(x => x.EmployeeName)
            .NotEmpty().WithMessage("Employee name is required")
            .MaximumLength(200).WithMessage("Employee name must not exceed 200 characters");

        // LeaveType: Required, must be one of the valid leave types
        RuleFor(x => x.LeaveType)
            .NotEmpty().WithMessage("Leave type is required")
            .Must(BeValidLeaveType).WithMessage($"Leave type must be one of: {string.Join(", ", ValidLeaveTypes)}");

        // StartDate: Required, must be before or equal to EndDate
        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required")
            .LessThanOrEqualTo(x => x.EndDate).WithMessage("Start date must be before or equal to end date");

        // EndDate: Required, must be after or equal to StartDate
        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required")
            .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("End date must be after or equal to start date");

        // Status: Optional, but if provided, must be valid
        // Default status is typically "Pending" when creating a new request
        RuleFor(x => x.Status)
            .Must(BeValidStatus).WithMessage($"Status must be one of: {string.Join(", ", ValidStatuses)}")
            .When(x => !string.IsNullOrWhiteSpace(x.Status));

        // Reason: Optional, but if provided, max 1000 characters
        RuleFor(x => x.Reason)
            .MaximumLength(1000).WithMessage("Reason must not exceed 1000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Reason));
    }

    /// <summary>
    /// Custom validation method to check if leave type is valid
    /// </summary>
    /// <param name="leaveType">The leave type string to validate</param>
    /// <returns>True if leave type is valid, false otherwise</returns>
    /// <remarks>
    /// Valid leave types: "Sick", "Casual", "Annual", "Unpaid"
    /// </remarks>
    private bool BeValidLeaveType(string? leaveType)
    {
        return !string.IsNullOrWhiteSpace(leaveType) && ValidLeaveTypes.Contains(leaveType);
    }

    /// <summary>
    /// Custom validation method to check if status is valid
    /// </summary>
    /// <param name="status">The status string to validate</param>
    /// <returns>True if status is valid or empty, false otherwise</returns>
    /// <remarks>
    /// Valid statuses: "Pending", "Approved", "Rejected", "Cancelled"
    /// Empty/null status is allowed (will default to "Pending" in service layer).
    /// </remarks>
    private bool BeValidStatus(string? status)
    {
        return string.IsNullOrWhiteSpace(status) || ValidStatuses.Contains(status);
    }
}
