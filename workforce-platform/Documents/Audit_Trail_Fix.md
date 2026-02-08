# Audit Trail Fix - Worker Model Alignment

## Problem

When updating employees (or performing other domain operations), no audit logs were being created in MongoDB. The `AuditLogs` collection was empty.

## Root Cause

There was a **model mismatch** between the worker service and the API:

1. **Worker Service** was using `AuditLogEntry` model with simple structure:
   - `eventId`, `eventType`, `eventData`, `timestamp`

2. **API** was expecting `AuditLog` model with richer structure:
   - `eventId`, `eventType`, `entityType`, `entityId`, `actor`, `timestamp`, `before`, `after`, `metadata`

3. The API's repository was querying for fields like `EntityType` and `EntityId` that didn't exist in the worker's documents.

## Solution

Updated the worker service to use the same `AuditLog` model structure as the API:

### Changes Made

1. **Created `AuditLog.cs` model** in worker service matching API structure
2. **Updated `AuditLogService.cs`** to:
   - Use `AuditLog` instead of `AuditLogEntry`
   - Extract `EntityType` from event type (e.g., "employee.updated" → "Employee")
   - Extract `EntityId` from event data (e.g., `Data.EmployeeId`)
   - Store event data in `after` field
   - Set default `Actor` to "System"

3. **Updated `RabbitMqConsumer.cs`** to pass full event payload to audit service

## How It Works Now

1. **Event Published** (from API):
   ```json
   {
     "EventId": "guid",
     "EventType": "employee.updated",
     "Timestamp": "2024-01-01T00:00:00Z",
     "Data": {
       "EmployeeId": "guid"
     }
   }
   ```

2. **Worker Processes Event**:
   - Extracts `EntityType`: "Employee" (from "employee.updated")
   - Extracts `EntityId`: from `Data.EmployeeId`
   - Creates `AuditLog` document with all required fields

3. **MongoDB Document**:
   ```json
   {
     "_id": "ObjectId(...)",
     "eventId": "guid",
     "eventType": "employee.updated",
     "entityType": "Employee",
     "entityId": "guid",
     "actor": "System",
     "timestamp": "2024-01-01T00:00:00Z",
     "before": null,
     "after": { /* event data */ },
     "metadata": {}
   }
   ```

4. **API Can Query**:
   - All fields exist and can be queried
   - Filtering by `entityType`, `entityId`, `eventType` works correctly

## Testing

To verify the fix works:

1. **Rebuild the worker service**:
   ```bash
   docker compose build auditlogger
   ```

2. **Restart services**:
   ```bash
   docker compose restart auditlogger
   ```

3. **Perform an operation** (e.g., update an employee)

4. **Check MongoDB**:
   ```bash
   docker compose exec mongodb mongosh -u admin -p changeme
   use workforce_db
   db.AuditLogs.find().sort({ timestamp: -1 }).limit(5).pretty()
   ```

5. **Check worker logs**:
   ```bash
   docker compose logs auditlogger
   ```
   Should see: `Audit log created for event {EventId}, entity: Employee/{EntityId}`

6. **Check frontend**:
   - Navigate to `/audit`
   - Should see audit logs appearing

## Files Changed

- `backend/WorkerService.AuditLogger/Models/AuditLog.cs` (new)
- `backend/WorkerService.AuditLogger/Services/AuditLogService.cs` (updated)
- `backend/WorkerService.AuditLogger/Services/RabbitMqConsumer.cs` (updated)

## Next Steps (Optional Enhancements)

1. **Extract Actor**: Get user context from authentication and include in events
2. **Before/After Snapshots**: Include entity state before and after changes
3. **Metadata**: Add additional context (IP address, user agent, etc.)
4. **Error Handling**: Better error recovery and retry logic
