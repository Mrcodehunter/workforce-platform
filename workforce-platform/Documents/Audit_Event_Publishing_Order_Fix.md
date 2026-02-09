# Audit Event Publishing Order Fix

## Overview
Fixed all services to ensure events are published **AFTER** all operations are completed, following the correct sequence:

1. Save beforeSnapshot into Redis
2. Execute business logic
3. Execute DB operations
4. Retrieve updated data from DB (or after operation data)
5. Save afterSnapshot into Redis
6. Publish the event
7. Event consumer consumes event and stores audit trail log

## Changes Made

### EmployeeService
**File:** `backend/WorkforceAPI/Services/EmployeeService.cs`

- ✅ **UpdateAsync**: Moved event publishing to after DB operations and after snapshot storage
- ✅ **DeleteAsync**: Moved event publishing to after DB operations

### ProjectService
**File:** `backend/WorkforceAPI/Services/ProjectService.cs`

- ✅ **UpdateAsync**: Moved event publishing to after DB operations and after snapshot storage
- ✅ **DeleteAsync**: Moved event publishing to after DB operations
- ✅ **AddMemberAsync**: Already correct, added clearer comments
- ✅ **RemoveMemberAsync**: Already correct, added clearer comments

### TaskService
**File:** `backend/WorkforceAPI/Services/TaskService.cs`

- ✅ **UpdateAsync**: Moved event publishing to after DB operations and after snapshot storage
- ✅ **UpdateTaskStatusAsync**: Already correct (event published after snapshots)
- ✅ **DeleteAsync**: Moved event publishing to after DB operations

### DepartmentService
**File:** `backend/WorkforceAPI/Services/DepartmentService.cs`

- ✅ **UpdateAsync**: Moved event publishing to after DB operations and after snapshot storage
- ✅ **DeleteAsync**: Moved event publishing to after DB operations

### DesignationService
**File:** `backend/WorkforceAPI/Services/DesignationService.cs`

- ✅ **UpdateAsync**: Moved event publishing to after DB operations and after snapshot storage
- ✅ **DeleteAsync**: Moved event publishing to after DB operations

### LeaveRequestService
**File:** `backend/WorkforceAPI/Services/LeaveRequestService.cs`

- ✅ **UpdateStatusAsync**: Already correct (event published after snapshots)

## Correct Flow Pattern

All services now follow this pattern:

```csharp
// Generate event ID first
var eventId = Guid.NewGuid().ToString();

// Step 1: Save beforeSnapshot into Redis
var beforeSnapshot = AuditEntitySerializer.SerializeEntity(existingEntity);
await _redisCache.SetAsync($"audit:{eventId}:before", beforeSnapshot, TimeSpan.FromHours(1));

// Step 2: Execute business logic
// ... business logic here ...

// Step 3: Execute DB operations
var result = await _repository.UpdateAsync(entity);

// Step 4: Retrieve updated data from DB
var reloaded = await _repository.GetByIdAsync(result.Id);

// Step 5: Save afterSnapshot into Redis
if (reloaded != null)
{
    var afterSnapshot = AuditEntitySerializer.SerializeEntity(reloaded);
    await _redisCache.SetAsync($"audit:{eventId}:after", afterSnapshot, TimeSpan.FromHours(1));
}

// Step 6: Publish the event (after all operations completed)
await _eventPublisher.PublishEventAsync(AuditEventType.EntityUpdated, new { EntityId = entity.Id }, eventId);
```

## Benefits

1. **Data Consistency**: Events are only published after all operations succeed
2. **Reliability**: Before/after snapshots are guaranteed to be in Redis before event consumption
3. **Error Handling**: If DB operations fail, no event is published
4. **Audit Trail Accuracy**: Audit logs always have complete before/after data

## Testing Checklist

- [ ] Employee update operations publish events after all operations complete
- [ ] Project update operations publish events after all operations complete
- [ ] Task update operations publish events after all operations complete
- [ ] Department update operations publish events after all operations complete
- [ ] Designation update operations publish events after all operations complete
- [ ] All delete operations publish events after DB operations complete
- [ ] Project member add/remove operations publish events after all operations complete
- [ ] All audit logs have complete before/after snapshots
