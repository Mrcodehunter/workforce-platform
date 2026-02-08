# Audit Trail Generation and Retrieval Guide

This document explains how audit trail logs are generated, processed, and retrieved in the Workforce Platform system.

## Overview

The audit trail system captures all significant domain events (create, update, delete operations) and stores them in MongoDB for historical tracking and compliance purposes. The system uses a **message-driven architecture** with RabbitMQ as the message broker.

## Architecture Flow

```
┌─────────────────┐
│  WorkforceAPI   │
│  (Backend API)  │
└────────┬────────┘
         │
         │ 1. Domain Event Occurs
         │    (e.g., employee.created)
         │
         ▼
┌─────────────────┐
│   RabbitMQ      │
│  Message Broker │
└────────┬────────┘
         │
         │ 2. Event Published
         │    Routing Key: event type
         │
         ▼
┌─────────────────┐
│ Audit Logger    │
│ Worker Service  │
│   (.NET 10)     │
└────────┬────────┘
         │
         │ 3. Event Consumed
         │    & Processed
         │
         ▼
┌─────────────────┐
│    MongoDB      │
│  AuditLogs      │
│  Collection     │
└─────────────────┘
         │
         │ 4. Query via API
         │
         ▼
┌─────────────────┐
│   Frontend      │
│  Audit Trail UI │
└─────────────────┘
```

## Step-by-Step Process

### Phase 1: Event Generation (Backend API)

When a domain operation occurs in the WorkforceAPI:

#### 1.1 Service Layer Publishes Events

**Location**: `backend/WorkforceAPI/Services/*Service.cs`

**Example - Employee Creation**:
```csharp
// In EmployeeService.CreateAsync()
public async Task<Employee> CreateAsync(Employee employee)
{
    // ... business logic ...
    var result = await _repository.CreateAsync(employee);
    
    // Publish domain event
    await _eventPublisher.PublishEventAsync("employee.created", new 
    { 
        EmployeeId = result.Id,
        EventId = Guid.NewGuid().ToString(),
        Timestamp = DateTime.UtcNow
    });
    
    return result;
}
```

**Supported Event Types**:
- `employee.created` - When a new employee is created
- `employee.updated` - When an employee is updated
- `employee.deleted` - When an employee is soft-deleted
- `project.created` - When a new project is created
- `project.updated` - When a project is updated
- `project.deleted` - When a project is deleted
- `task.created` - When a new task is created
- `task.updated` - When a task is updated
- `task.deleted` - When a task is deleted
- `task.status.updated` - When a task status changes
- `leave.request.created` - When a leave request is submitted
- `leave.request.approved` - When a leave request is approved
- `leave.request.rejected` - When a leave request is rejected
- `leave.request.cancelled` - When a leave request is cancelled

#### 1.2 Event Publisher

**Location**: `backend/WorkforceAPI/EventPublisher/RabbitMqPublisher.cs`

The `RabbitMqPublisher` serializes the event data and publishes it to RabbitMQ:

```csharp
public Task PublishEventAsync(string eventType, object eventData)
{
    var eventPayload = new
    {
        EventId = Guid.NewGuid().ToString(),
        EventType = eventType,
        Timestamp = DateTime.UtcNow,
        Data = eventData
    };
    
    var message = JsonSerializer.Serialize(eventPayload);
    var body = Encoding.UTF8.GetBytes(message);
    
    _channel.BasicPublish(
        exchange: "workforce.events",
        routingKey: eventType,  // e.g., "employee.created"
        basicProperties: null,
        body: body);
}
```

**Configuration** (from `appsettings.json`):
```json
{
  "RabbitMQ": {
    "Host": "rabbitmq",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest",
    "ExchangeName": "workforce.events",
    "ExchangeType": "topic"
  }
}
```

### Phase 2: Event Consumption (Worker Service)

#### 2.1 Worker Service Setup

**Location**: `backend/WorkerService.AuditLogger/`

The Audit Logger Worker is a .NET 10 background service that:
- Connects to RabbitMQ
- Subscribes to all domain events
- Processes events and creates audit log entries
- Stores logs in MongoDB

#### 2.2 RabbitMQ Consumer

**Location**: `backend/WorkerService.AuditLogger/Services/RabbitMqConsumer.cs`

```csharp
// Consumer subscribes to exchange with routing key pattern
var routingKeys = new[] { "#" }; // "#" means all events

foreach (var routingKey in routingKeys)
{
    _channel.QueueBind("audit.queue", "workforce.events", routingKey);
}

// Event handler
consumer.Received += async (model, ea) =>
{
    var message = Encoding.UTF8.GetString(ea.Body.ToArray());
    var routingKey = ea.RoutingKey; // e.g., "employee.created"
    var eventData = JsonSerializer.Deserialize<Dictionary<string, object>>(message);
    
    // Extract event ID for idempotency
    var eventId = eventData["EventId"]?.ToString();
    
    // Log to audit service
    await _auditLogService.LogEventAsync(eventId, routingKey, eventData);
    
    _channel.BasicAck(ea.DeliveryTag, false);
};
```

#### 2.3 Audit Log Service

**Location**: `backend/WorkerService.AuditLogger/Services/AuditLogService.cs`

The service ensures **idempotency** (prevents duplicate logs) and creates audit entries:

```csharp
public async Task LogEventAsync(string eventId, string eventType, object eventData)
{
    // Check if event already processed (idempotency)
    var existing = await AuditCollection
        .Find(a => a.EventId == eventId)
        .FirstOrDefaultAsync();
    
    if (existing != null)
    {
        _logger.Information("Event {EventId} already processed, skipping", eventId);
        return; // Skip duplicate
    }
    
    // Create audit log entry
    var auditLog = new AuditLogEntry
    {
        EventId = eventId,
        EventType = eventType,
        EventData = eventData,
        Timestamp = DateTime.UtcNow
    };
    
    await AuditCollection.InsertOneAsync(auditLog);
}
```

**Current Model** (`AuditLogEntry`):
```csharp
public class AuditLogEntry
{
    public string? Id { get; set; }
    public string EventId { get; set; }
    public string EventType { get; set; }
    public object EventData { get; set; }
    public DateTime Timestamp { get; set; }
}
```

**Note**: The worker currently uses a simpler model. The API expects a richer `AuditLog` model with:
- `EntityType` (e.g., "Employee", "Project")
- `EntityId` (the ID of the affected entity)
- `Actor` (who triggered the event)
- `Before` (snapshot before change)
- `After` (snapshot after change)
- `Metadata` (additional context)

### Phase 3: Data Storage (MongoDB)

#### 3.1 Collection Structure

**Collection Name**: `AuditLogs`

**Current Document Structure** (from worker):
```json
{
  "_id": "ObjectId(...)",
  "eventId": "guid-string",
  "eventType": "employee.created",
  "eventData": {
    "EmployeeId": "guid",
    "EventId": "guid",
    "Timestamp": "2024-01-01T00:00:00Z"
  },
  "timestamp": "2024-01-01T00:00:00Z"
}
```

**Expected Structure** (for API):
```json
{
  "_id": "ObjectId(...)",
  "eventId": "guid-string",
  "eventType": "employee.created",
  "entityType": "Employee",
  "entityId": "guid-string",
  "actor": "System",
  "timestamp": "2024-01-01T00:00:00Z",
  "before": null,
  "after": { /* employee data */ },
  "metadata": {}
}
```

### Phase 4: Data Retrieval (Backend API)

#### 4.1 API Endpoints

**Location**: `backend/WorkforceAPI/Controllers/AuditLogsController.cs`

**Endpoints**:
- `GET /api/auditlogs` - Get all with optional filters
  - Query params: `entityType`, `eventType`, `startDate`, `endDate`, `limit`
- `GET /api/auditlogs/recent` - Get recent activity (default: 50)
- `GET /api/auditlogs/entity/{entityType}/{entityId}` - Get logs for specific entity
- `GET /api/auditlogs/event/{eventType}` - Get logs by event type

#### 4.2 Repository Layer

**Location**: `backend/WorkforceAPI/Repositories/AuditLogRepository.cs`

The repository queries MongoDB with filters:
```csharp
public async Task<IEnumerable<AuditLog>> GetByEntityIdAsync(string entityType, string entityId)
{
    var filter = Builders<AuditLog>.Filter.And(
        Builders<AuditLog>.Filter.Eq(x => x.EntityType, entityType),
        Builders<AuditLog>.Filter.Eq(x => x.EntityId, entityId)
    );
    return await Collection.Find(filter)
        .SortByDescending(x => x.Timestamp)
        .ToListAsync();
}
```

### Phase 5: Frontend Display

#### 5.1 React Query Hooks

**Location**: `frontend/src/hooks/useAuditLogs.ts`

```typescript
export const useAuditLogs = (filters?: AuditLogFilters) => {
  return useQuery({
    queryKey: ['auditLogs', filters],
    queryFn: () => auditLogsApi.getAll(filters),
  });
};
```

#### 5.2 UI Components

- **Audit Trail Page** (`/audit`) - System-wide activity feed with filtering
- **Entity Audit Log** - Component for entity detail pages (e.g., Employee Detail)

## How to Generate Audit Trail Data

### Method 1: Perform Domain Operations

The easiest way to generate audit logs is to perform operations in the application:

1. **Start all services**:
   ```bash
   docker compose up -d
   ```

2. **Create an Employee**:
   - Navigate to `/employees/new`
   - Fill in the form and submit
   - This triggers `employee.created` event

3. **Update an Employee**:
   - Navigate to an employee detail page
   - Click "Edit"
   - Make changes and save
   - This triggers `employee.updated` event

4. **Create a Project**:
   - Navigate to `/projects/new`
   - Create a new project
   - This triggers `project.created` event

5. **Create a Task**:
   - Navigate to a project detail page
   - Create a task
   - This triggers `task.created` event

6. **Submit a Leave Request**:
   - Navigate to `/leave-requests/new`
   - Submit a leave request
   - This triggers `leave.request.created` event

### Method 2: Verify Worker is Running

Check if the Audit Logger Worker is running and processing events:

```bash
# Check worker logs
docker compose logs auditlogger

# You should see messages like:
# [AuditWorker] Received event: employee.created
# [AuditWorker] Audit log created for event {EventId}
```

### Method 3: Check MongoDB Directly

Connect to MongoDB and query the `AuditLogs` collection:

```bash
# Connect to MongoDB container
docker compose exec mongodb mongosh -u admin -p changeme

# Switch to database
use workforce_db

# Query audit logs
db.AuditLogs.find().sort({ timestamp: -1 }).limit(10).pretty()
```

### Method 4: Verify RabbitMQ

Check if events are being published to RabbitMQ:

```bash
# Check RabbitMQ management UI (if enabled)
# Or check logs
docker compose logs rabbitmq
```

## Troubleshooting

### Issue: No Audit Logs Appearing

**Possible Causes**:

1. **Worker Not Running**:
   ```bash
   docker compose ps
   # Check if auditlogger service is running
   ```

2. **RabbitMQ Connection Issues**:
   - Verify RabbitMQ is running: `docker compose ps rabbitmq`
   - Check connection string in `appsettings.json`

3. **Events Not Being Published**:
   - Check service layer code for `PublishEventAsync` calls
   - Verify `IRabbitMqPublisher` is registered in DI

4. **MongoDB Connection Issues**:
   - Verify MongoDB is running: `docker compose ps mongodb`
   - Check connection string in worker's `appsettings.json`

5. **Model Mismatch**:
   - Worker uses `AuditLogEntry` (simpler)
   - API expects `AuditLog` (richer)
   - May need to update worker to extract entity info from event data

### Issue: Duplicate Audit Logs

The worker implements idempotency by checking `EventId`. If duplicates appear:
- Check if `EventId` is being generated consistently
- Verify the idempotency check in `AuditLogService.LogEventAsync`

### Issue: Missing Entity Information

The current worker model doesn't extract `EntityType`, `EntityId`, `Actor`, `Before`, `After`. To fix:

1. **Update Worker Model** to match API's `AuditLog` model
2. **Extract Entity Info** from event data in the worker
3. **Include Before/After Snapshots** when publishing events from services

## Future Enhancements

1. **Enrich Event Data**: Include entity snapshots (before/after) in event payload
2. **Actor Tracking**: Add user context to events (who performed the action)
3. **Worker Model Alignment**: Update worker to use the same `AuditLog` model as API
4. **Event Enrichment**: Extract `EntityType` and `EntityId` from event data automatically
5. **Retry Logic**: Implement retry mechanism for failed event processing
6. **Dead Letter Queue**: Handle events that fail processing

## Summary

The audit trail system works as follows:

1. **Domain operations** in the API trigger events
2. **Events are published** to RabbitMQ with routing keys
3. **Worker service consumes** events from RabbitMQ
4. **Audit logs are created** in MongoDB (with idempotency)
5. **API queries** MongoDB to retrieve audit logs
6. **Frontend displays** audit logs in UI

To generate audit trail data, simply perform operations in the application (create/update/delete employees, projects, tasks, leave requests) and the system will automatically capture these events.
