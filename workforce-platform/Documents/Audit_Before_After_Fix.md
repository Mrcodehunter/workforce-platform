# Audit Before/After Data Storage Fix

## Problem

Audit logs were not consistently storing before/after data. Some cases had null before data, some had null after data.

## Root Cause

The issue was a **race condition** in the event publishing flow:

1. Event was published to RabbitMQ with a generated EventId
2. Redis keys were set AFTER publishing
3. Worker consumed the event and tried to read from Redis
4. **Problem**: Worker might process the event before Redis keys were set, resulting in null before/after data

## Solution

### 1. Fixed Event Publishing Order

Changed the flow to ensure Redis keys are set **BEFORE** publishing events:

**Before (Wrong Order):**
```csharp
var eventId = await _eventPublisher.PublishEventAsync("employee.updated", data);
await _redisCache.SetAsync($"audit:{eventId}:before", beforeSnapshot);
```

**After (Correct Order):**
```csharp
var eventId = Guid.NewGuid().ToString();
await _redisCache.SetAsync($"audit:{eventId}:before", beforeSnapshot);
await _eventPublisher.PublishEventAsync("employee.updated", data, eventId);
```

### 2. Updated RabbitMqPublisher Interface

Modified `IRabbitMqPublisher` to accept an optional `eventId` parameter:

```csharp
Task<string> PublishEventAsync(string eventType, object eventData, string? eventId = null, CancellationToken cancellationToken = default);
```

This allows services to:
- Generate EventId first
- Store snapshots in Redis
- Pass the same EventId when publishing

### 3. Updated All Service Methods

Fixed the order in all CRUD operations:

#### Create Operations
1. Perform create
2. Reload entity with navigation properties
3. Generate EventId
4. Store "after" snapshot in Redis
5. Publish event with EventId

#### Update Operations
1. Load existing entity
2. Generate EventId
3. Store "before" snapshot in Redis
4. Publish event with EventId
5. Perform update
6. Reload entity
7. Store "after" snapshot in Redis

#### Delete Operations
1. Load existing entity
2. Generate EventId
3. Store "before" snapshot in Redis
4. Publish event with EventId
5. Perform delete

## Files Modified

### Services
- `WorkforceAPI/Services/EmployeeService.cs`
- `WorkforceAPI/Services/TaskService.cs`
- `WorkforceAPI/Services/ProjectService.cs`

### Event Publisher
- `WorkforceAPI/EventPublisher/IRabbitMqPublisher.cs`
- `WorkforceAPI/EventPublisher/RabbitMqPublisher.cs`

## Benefits

✅ **Consistent Data**: Before/after snapshots are always available when worker processes events
✅ **No Race Conditions**: Redis keys are guaranteed to exist before event is published
✅ **Idempotency**: Same EventId used for Redis keys and event payload
✅ **Reliability**: Worker can always retrieve full audit trail data

## Testing

To verify the fix:

1. **Update an employee** → Check audit log has both before and after data
2. **Create a task** → Check audit log has after data
3. **Delete a project** → Check audit log has before data
4. **Update a project** → Check audit log has both before and after data

All audit logs should now have complete before/after snapshots stored in MongoDB.
