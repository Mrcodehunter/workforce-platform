# Event Type Enum Refactoring

## Overview

Refactored all audit event types from raw string literals to a centralized enum (`AuditEventType`) in the shared library. This improves type safety, maintainability, and reduces the risk of typos.

## Changes Made

### 1. Updated `IRabbitMqPublisher` Interface

**File:** `Workforce.Shared/EventPublisher/IRabbitMqPublisher.cs`

Changed the method signature to accept `AuditEventType` enum instead of `string`:

```csharp
// Before
Task<string> PublishEventAsync(string eventType, object eventData, string? eventId = null, CancellationToken cancellationToken = default);

// After
Task<string> PublishEventAsync(AuditEventType eventType, object eventData, string? eventId = null, CancellationToken cancellationToken = default);
```

### 2. Updated `RabbitMqPublisher` Implementation

**File:** `Workforce.Shared/EventPublisher/RabbitMqPublisher.cs`

- Added `using Workforce.Shared.Events;`
- Updated `PublishEventAsync` to accept `AuditEventType` enum
- Convert enum to routing key string using `eventType.ToRoutingKey()`
- Updated logging to include both enum value and routing key

```csharp
// Convert enum to routing key string
var routingKey = eventType.ToRoutingKey();

// Use routing key for RabbitMQ
_channel.BasicPublish(
    exchange: exchangeName,
    routingKey: routingKey,
    basicProperties: null,
    body: body);
```

### 3. Replaced Raw Strings in Service Classes

All service classes now use the `AuditEventType` enum:

#### EmployeeService.cs
- `"employee.created"` → `AuditEventType.EmployeeCreated`
- `"employee.updated"` → `AuditEventType.EmployeeUpdated`
- `"employee.deleted"` → `AuditEventType.EmployeeDeleted`

#### ProjectService.cs
- `"project.created"` → `AuditEventType.ProjectCreated`
- `"project.updated"` → `AuditEventType.ProjectUpdated`
- `"project.deleted"` → `AuditEventType.ProjectDeleted`

#### TaskService.cs
- `"task.created"` → `AuditEventType.TaskCreated`
- `"task.updated"` → `AuditEventType.TaskUpdated`
- `"task.deleted"` → `AuditEventType.TaskDeleted`
- `"task.status.updated"` → `AuditEventType.TaskStatusUpdated`

#### DepartmentService.cs
- `"department.created"` → `AuditEventType.DepartmentCreated`
- `"department.updated"` → `AuditEventType.DepartmentUpdated`
- `"department.deleted"` → `AuditEventType.DepartmentDeleted`

#### LeaveRequestService.cs
- `"leave.request.created"` → `AuditEventType.LeaveRequestCreated`
- `"leave.request.updated"` → `AuditEventType.LeaveRequestUpdated`
- `"leave.request.approved"` → `AuditEventType.LeaveRequestApproved`
- `"leave.request.rejected"` → `AuditEventType.LeaveRequestRejected`
- `"leave.request.cancelled"` → `AuditEventType.LeaveRequestCancelled`

**Note:** The switch statement in `LeaveRequestService.UpdateStatusAsync` now returns enum values instead of strings.

## Benefits

✅ **Type Safety**: Compile-time checking prevents typos and invalid event types
✅ **IntelliSense Support**: IDE autocomplete for all available event types
✅ **Centralized Management**: All event types defined in one place (`AuditEventType.cs`)
✅ **Refactoring Safety**: Renaming enum values automatically updates all usages
✅ **Documentation**: Enum values serve as self-documenting code
✅ **Consistency**: Ensures all services use the same event type naming convention

## Enum Location

The `AuditEventType` enum and its extension methods are located in:
- `Workforce.Shared/Events/AuditEventType.cs`

This shared location allows both the API and worker services to use the same enum definitions.

## Backward Compatibility

The enum is converted to the original string format (routing keys) using the `ToRoutingKey()` extension method, ensuring:
- RabbitMQ routing keys remain unchanged
- Existing consumers continue to work without modification
- No breaking changes to the message queue structure

## Files Modified

1. `Workforce.Shared/EventPublisher/IRabbitMqPublisher.cs`
2. `Workforce.Shared/EventPublisher/RabbitMqPublisher.cs`
3. `WorkforceAPI/Services/EmployeeService.cs`
4. `WorkforceAPI/Services/ProjectService.cs`
5. `WorkforceAPI/Services/TaskService.cs`
6. `WorkforceAPI/Services/DepartmentService.cs`
7. `WorkforceAPI/Services/LeaveRequestService.cs`

## Testing

After this refactoring, verify:
1. ✅ All services compile without errors
2. ✅ Events are still published correctly to RabbitMQ
3. ✅ Routing keys match the original string format
4. ✅ Audit logger worker can still consume events
5. ✅ No runtime errors when creating/updating/deleting entities
