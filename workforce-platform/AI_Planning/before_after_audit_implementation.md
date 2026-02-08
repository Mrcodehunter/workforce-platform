# Before/After Audit Data Implementation Plan

## Overview
Implement Redis-based before/after snapshot capture for audit logs. This approach stores entity snapshots in Redis before operations, then the worker retrieves them when processing events.

## Architecture

```
┌─────────────────┐
│  WorkforceAPI   │
│  (Backend API)  │
└────────┬────────┘
         │
         │ 1. Capture "Before" state
         │    → Save to Redis (key: eventId:before)
         │
         │ 2. Perform operation (create/update/delete)
         │
         │ 3. Capture "After" state
         │    → Save to Redis (key: eventId:after)
         │
         │ 4. Publish event with EventId
         │    → RabbitMQ
         │
         ▼
┌─────────────────┐
│   RabbitMQ      │
└────────┬────────┘
         │
         │ 5. Event consumed
         │
         ▼
┌─────────────────┐
│ Audit Logger    │
│ Worker Service  │
└────────┬────────┘
         │
         │ 6. Read from Redis
         │    → Get "before" (eventId:before)
         │    → Get "after" (eventId:after)
         │
         │ 7. Create audit log with before/after
         │
         │ 8. Delete from Redis (cleanup)
         │
         ▼
┌─────────────────┐
│    MongoDB      │
│  AuditLogs      │
└─────────────────┘
```

## Implementation Steps

### Phase 1: Add Redis to Infrastructure

1. **Add Redis to docker-compose.yml**
   - Redis service
   - Health check
   - Volume for persistence

2. **Update appsettings.json**
   - Add Redis connection string

### Phase 2: Create Redis Service

1. **Create IRedisCache interface**
   - `SetAsync(key, value, expiration)`
   - `GetAsync<T>(key)`
   - `DeleteAsync(key)`

2. **Create RedisCache implementation**
   - Use StackExchange.Redis
   - JSON serialization for values
   - TTL for automatic cleanup (e.g., 1 hour)

### Phase 3: Update Service Layer

1. **Update EmployeeService**
   - `UpdateAsync`: Capture before state → Save to Redis → Update → Capture after state → Save to Redis → Publish event
   - `CreateAsync`: Capture after state → Save to Redis → Publish event
   - `DeleteAsync`: Capture before state → Save to Redis → Delete → Publish event

2. **Update ProjectService**
   - Same pattern for create/update/delete

3. **Update TaskService**
   - Same pattern for create/update/delete

4. **Update LeaveRequestService**
   - Same pattern for status updates

### Phase 4: Update Worker

1. **Update AuditLogService**
   - Read before/after from Redis using EventId
   - Store in audit log
   - Delete from Redis after successful write

2. **Add error handling**
   - If Redis read fails, log warning but continue
   - Set TTL on Redis keys for automatic cleanup

### Phase 5: Fix Task Creation API

1. **Check validation errors**
   - Review TaskValidator rules
   - Check required fields
   - Fix any validation issues

## Redis Key Pattern

- **Before snapshot**: `audit:{eventId}:before`
- **After snapshot**: `audit:{eventId}:after`
- **TTL**: 1 hour (3600 seconds)

## Benefits of Redis Approach

✅ **Event payload stays small** - Only EventId in event
✅ **Decoupled** - Worker can retrieve data independently
✅ **Flexible** - Can store complex nested objects
✅ **Automatic cleanup** - TTL ensures old keys are removed
✅ **Performance** - Fast in-memory storage

## Alternative: Event-Based Approach

If not using Redis, we could:
- Include before/after in event payload
- **Cons**: Larger event payloads, potential serialization issues
- **Pros**: Simpler, no additional infrastructure

## Decision

**Chosen: Redis-based approach** for better scalability and separation of concerns.
