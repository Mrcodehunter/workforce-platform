using FluentValidation;
using WorkforceAPI.Models.MongoDB;

namespace WorkforceAPI.Validators;

public class LeaveRequestValidator : AbstractValidator<LeaveRequest>
{
    private static readonly string[] ValidLeaveTypes = { "Sick", "Casual", "Annual", "Unpaid" };
    private static readonly string[] ValidStatuses = { "Pending", "Approved", "Rejected", "Cancelled" };

    public LeaveRequestValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("Employee ID is required")
            .NotEqual(Guid.Empty).WithMessage("Employee ID is required");

        RuleFor(x => x.EmployeeName)
            .NotEmpty().WithMessage("Employee name is required")
            .MaximumLength(200).WithMessage("Employee name must not exceed 200 characters");

        RuleFor(x => x.LeaveType)
            .NotEmpty().WithMessage("Leave type is required")
            .Must(BeValidLeaveType).WithMessage($"Leave type must be one of: {string.Join(", ", ValidLeaveTypes)}");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required")
            .LessThanOrEqualTo(x => x.EndDate).WithMessage("Start date must be before or equal to end date");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required")
            .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("End date must be after or equal to start date");

        RuleFor(x => x.Status)
            .Must(BeValidStatus).WithMessage($"Status must be one of: {string.Join(", ", ValidStatuses)}")
            .When(x => !string.IsNullOrWhiteSpace(x.Status));

        RuleFor(x => x.Reason)
            .MaximumLength(1000).WithMessage("Reason must not exceed 1000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Reason));
    }

    private bool BeValidLeaveType(string? leaveType)
    {
        return !string.IsNullOrWhiteSpace(leaveType) && ValidLeaveTypes.Contains(leaveType);
    }

    private bool BeValidStatus(string? status)
    {
        return string.IsNullOrWhiteSpace(status) || ValidStatuses.Contains(status);
    }
}
