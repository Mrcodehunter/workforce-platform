# Audit Log Serialization Fix

## Problem

The audit logger worker was failing to write audit logs to MongoDB with the following error:

```
MongoDB.Bson.BsonSerializationException: Type System.Text.Json.JsonElement is not configured 
as a type that is allowed to be serialized for this instance of ObjectSerializer.
```

## Root Cause

When deserializing JSON using `JsonSerializer.Deserialize<Dictionary<string, object>>()`, the values in the dictionary are `JsonElement` instances, not actual .NET objects. MongoDB's BSON serializer cannot serialize `JsonElement` directly because it's a read-only struct from `System.Text.Json`.

**Location**: `backend/WorkerService.AuditLogger/Services/AuditLogService.cs:93`

The code was setting:
```csharp
After = after ?? eventData
```

Where `eventData` is a `Dictionary<string, object>` containing `JsonElement` values that MongoDB cannot serialize.

## Solution

Added a conversion method `ConvertToBsonCompatible()` that recursively converts `JsonElement` instances to MongoDB-compatible types:

- **JsonElement.Object** → `Dictionary<string, object>`
- **JsonElement.Array** → `List<object>`
- **JsonElement.String** → `string`
- **JsonElement.Number** → `long` or `double`
- **JsonElement.True/False** → `bool`
- **JsonElement.Null** → `null`

### Changes Made

1. **Added `ConvertToBsonCompatible()` method**:
   - Handles `JsonElement`, `Dictionary<string, object>`, and collections
   - Recursively converts nested structures
   - Preserves primitive types as-is

2. **Added `ConvertJsonElement()` helper method**:
   - Converts `JsonElement` based on its `ValueKind`
   - Handles all JSON value types properly

3. **Updated `LogEventAsync()` method**:
   - Converts `eventData` before using it
   - Converts `before` and `after` values before assignment
   - Ensures all data is BSON-compatible before MongoDB insertion

4. **Updated `ExtractEntityIdFromEventData()` method**:
   - Uses `ConvertToBsonCompatible()` when extracting entity IDs
   - Ensures proper string conversion

## Code Changes

### Before
```csharp
After = after ?? eventData, // eventData contains JsonElement values
```

### After
```csharp
object? serializableEventData = ConvertToBsonCompatible(eventData);
After = after ?? serializableEventData, // All values are BSON-compatible
```

## Testing

After the fix:

1. **Rebuild the worker**:
   ```bash
   docker compose build worker-audit
   ```

2. **Restart the worker**:
   ```bash
   docker compose restart worker-audit
   ```

3. **Trigger an event** (e.g., update an employee)

4. **Check logs**:
   ```bash
   docker compose logs worker-audit
   # Should see: "Audit log created for event {EventId}..."
   ```

5. **Verify in MongoDB**:
   ```bash
   docker compose exec mongodb mongosh -u admin -p changeme
   use workforce_db
   db.AuditLogs.find().sort({ timestamp: -1 }).limit(1).pretty()
   ```

## Impact

✅ **Fixed**: Audit logs can now be written to MongoDB successfully
✅ **Fixed**: All event data is properly serialized
✅ **Fixed**: Before/After snapshots are stored correctly
✅ **Fixed**: Entity ID extraction works with JsonElement values

## Files Modified

- `backend/WorkerService.AuditLogger/Services/AuditLogService.cs`
  - Added `ConvertToBsonCompatible()` method
  - Added `ConvertJsonElement()` helper method
  - Updated `LogEventAsync()` to convert data before storage
  - Updated `ExtractEntityIdFromEventData()` to handle JsonElement
