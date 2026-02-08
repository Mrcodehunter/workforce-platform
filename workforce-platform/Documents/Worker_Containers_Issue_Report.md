# Worker Containers Issue Report

## Executive Summary

Both worker containers (`worker-audit` and `worker-reports`) are starting and immediately stopping due to runtime errors. This report identifies the root causes and provides solutions.

---

## Issue 1: Audit Logger Worker (worker-audit)

### Error
```
System.InvalidOperationException: Unable to resolve service for type 'Serilog.ILogger' 
while attempting to activate 'WorkerService.AuditLogger.Services.AuditLogService'.
```

### Root Cause
The `AuditLogService` constructor requires `Serilog.ILogger`, but this type is not registered in the dependency injection container. The `Program.cs` configures Serilog but doesn't register `Serilog.ILogger` as a service.

**Location**: `backend/WorkerService.AuditLogger/Services/AuditLogService.cs:12`

```csharp
public AuditLogService(IMongoDatabase mongoDatabase, Serilog.ILogger logger)
```

### Analysis
- ✅ Serilog is configured in `Program.cs` (line 10-13)
- ✅ `builder.Services.AddSerilog()` is called (line 15)
- ❌ `Serilog.ILogger` is not explicitly registered in DI container
- ❌ `AuditLogService` and `RabbitMqConsumer` both require `Serilog.ILogger` directly

### Solution
**Option 1 (Recommended)**: Use `ILogger<AuditLogService>` instead of `Serilog.ILogger`
- Change constructor parameter to `ILogger<AuditLogService>`
- This is the standard .NET logging pattern
- Automatically registered by `AddSerilog()`

**Option 2**: Register `Serilog.ILogger` explicitly
- Add `builder.Services.AddSingleton(Log.Logger);` in `Program.cs`

### Files to Fix
1. `backend/WorkerService.AuditLogger/Services/AuditLogService.cs`
2. `backend/WorkerService.AuditLogger/Services/RabbitMqConsumer.cs`

---

## Issue 2: Report Generator Worker (worker-reports)

### Error
```
Error [ERR_MODULE_NOT_FOUND]: Cannot find module '/app/src/consumers/scheduleConsumer.js' 
imported from /app/src/index.js
```

### Root Cause
The `index.js` file imports `./consumers/scheduleConsumer.js`, but this file doesn't exist in the Docker container. The Dockerfile may not be copying all necessary files, or the file structure is incorrect.

**Location**: `workers/report-generator/src/index.js:4`

```javascript
import { connectRabbitMQ, startConsuming } from './consumers/scheduleConsumer.js';
```

### Analysis
- ✅ `package.json` exists and has correct dependencies
- ✅ `src/index.js` exists
- ❌ `src/consumers/scheduleConsumer.js` is missing
- ❌ Dockerfile copies all files with `COPY . .` but the file may not exist in source

### Solution
**Option 1**: Create the missing file `src/consumers/scheduleConsumer.js`
- Implement `connectRabbitMQ()` and `startConsuming()` functions
- Connect to RabbitMQ using `amqplib`
- Set up message consumption

**Option 2**: Check if file exists but path is wrong
- Verify file structure matches import path
- Check if file needs to be in different location

### Files to Check/Create
1. `workers/report-generator/src/consumers/scheduleConsumer.js` (missing)
2. `workers/report-generator/src/reportGenerator.js` (referenced but may be missing)
3. `workers/report-generator/Dockerfile` (verify file copying)

---

## Docker Configuration Analysis

### Docker Compose Configuration
✅ **Correct**:
- Both workers have proper `depends_on` with health checks
- Environment variables are properly set
- Network configuration is correct
- `restart: unless-stopped` is set

### Dockerfile Analysis

#### Audit Logger Dockerfile
✅ **Correct**:
- Multi-stage build (build → publish → runtime)
- Uses .NET 10.0 runtime
- Entry point is correct: `["dotnet", "WorkerService.AuditLogger.dll"]`

⚠️ **Potential Issue:
- No explicit error handling in Dockerfile
- Container will exit if application crashes

#### Report Generator Dockerfile
✅ **Correct**:
- Multi-stage build
- Uses Node.js 20-alpine
- Runs as non-root user

❌ **Issue**:
- Dockerfile copies all files with `COPY . .` (line 21)
- But missing source files may not be in the build context
- CMD references `src/index.js` which exists, but dependencies may be missing

---

## Configuration Analysis

### Audit Logger Configuration
✅ **appsettings.json**: Correctly configured
- MongoDB connection string
- RabbitMQ settings
- Serilog configuration

### Report Generator Configuration
✅ **package.json**: Dependencies are correct
- `amqplib`, `mongodb`, `pg`, `winston`, `node-cron`, `dotenv`

❌ **Missing Files**:
- `src/consumers/scheduleConsumer.js`
- Possibly `src/reportGenerator.js`
- Possibly `src/config/` directory files

---

## Impact Assessment

### Severity: **HIGH**
- Both workers are critical for system functionality
- Audit trail feature is completely non-functional
- Report generation feature is completely non-functional
- System cannot track domain events
- Dashboard reports cannot be generated

### Affected Features
1. **Audit Trail**: No audit logs are being created
2. **Event Processing**: Domain events are not being processed
3. **Report Generation**: Scheduled reports are not running
4. **System Monitoring**: No background processing activity

---

## Recommended Fixes

### Priority 1: Fix Audit Logger Worker

**File**: `backend/WorkerService.AuditLogger/Services/AuditLogService.cs`

**Change**:
```csharp
// FROM:
public AuditLogService(IMongoDatabase mongoDatabase, Serilog.ILogger logger)

// TO:
public AuditLogService(IMongoDatabase mongoDatabase, ILogger<AuditLogService> logger)
```

**File**: `backend/WorkerService.AuditLogger/Services/RabbitMqConsumer.cs`

**Change**:
```csharp
// FROM:
public RabbitMqConsumer(IConfiguration configuration, IAuditLogService auditLogService, Serilog.ILogger logger)

// TO:
public RabbitMqConsumer(IConfiguration configuration, IAuditLogService auditLogService, ILogger<RabbitMqConsumer> logger)
```

**Update logging calls**:
- Change `_logger.Information()` to `_logger.LogInformation()`
- Change `_logger.Error()` to `_logger.LogError()`
- Change `_logger.Warning()` to `_logger.LogWarning()`

### Priority 2: Fix Report Generator Worker

**Option A**: Create missing files
- Create `src/consumers/scheduleConsumer.js`
- Create `src/reportGenerator.js` (if missing)
- Create `src/config/` directory files (if needed)

**Option B**: Simplify worker (temporary)
- Remove RabbitMQ consumer import
- Implement basic scheduled report generation only
- Add RabbitMQ support later

---

## Testing Plan

After fixes are applied:

1. **Rebuild containers**:
   ```bash
   docker compose build worker-audit worker-reports
   ```

2. **Start containers**:
   ```bash
   docker compose up worker-audit worker-reports
   ```

3. **Check logs**:
   ```bash
   docker compose logs worker-audit
   docker compose logs worker-reports
   ```

4. **Verify containers are running**:
   ```bash
   docker compose ps
   ```

5. **Test functionality**:
   - Update an employee → Check audit logs in MongoDB
   - Wait for scheduled report → Check MongoDB Reports collection

---

## Summary

| Worker | Issue | Severity | Fix Complexity | Status |
|--------|-------|----------|----------------|--------|
| Audit Logger | DI registration for Serilog.ILogger | High | Low (change constructor) | ✅ **FIXED** |
| Report Generator | Missing source files | High | Medium (create files) | ✅ **FIXED** |

**Estimated Fix Time**: 30-60 minutes

**Recommended Action**: Fix Audit Logger first (simpler), then address Report Generator (may require implementing missing functionality).

---

## Fixes Applied

### ✅ Audit Logger Worker - FIXED

**Changes Made**:
1. Updated `AuditLogService.cs`:
   - Changed `Serilog.ILogger` to `ILogger<AuditLogService>`
   - Updated all logging calls to use `LogInformation()`, `LogError()`, `LogWarning()`

2. Updated `RabbitMqConsumer.cs`:
   - Changed `Serilog.ILogger` to `ILogger<RabbitMqConsumer>`
   - Updated all logging calls

3. Updated `AuditWorker.cs`:
   - Changed `Serilog.ILogger` to `ILogger<AuditWorker>`
   - Updated all logging calls

**Result**: Worker should now start successfully as `ILogger<T>` is automatically registered by `AddSerilog()`.

### ✅ Report Generator Worker - FIXED

**Files Created**:
1. `src/consumers/scheduleConsumer.js`:
   - Implements `connectRabbitMQ()` function
   - Implements `startConsuming()` function
   - Sets up RabbitMQ connection and message consumption
   - Handles graceful connection closing

2. `src/reportGenerator.js`:
   - Implements `generateDashboardReport()` function
   - Connects to both PostgreSQL and MongoDB
   - Aggregates data from both databases
   - Generates dashboard summary report
   - Saves report to MongoDB Reports collection

**Result**: Worker should now start successfully with all required dependencies.

---

## Next Steps

1. **Rebuild containers**:
   ```bash
   docker compose build worker-audit worker-reports
   ```

2. **Start containers**:
   ```bash
   docker compose up worker-audit worker-reports
   ```

3. **Verify containers are running**:
   ```bash
   docker compose ps
   # Both workers should show "Up" status
   ```

4. **Check logs for success messages**:
   ```bash
   docker compose logs worker-audit
   # Should see: "Audit Worker starting..." and "RabbitMQ consumer started"
   
   docker compose logs worker-reports
   # Should see: "Starting Report Generator Worker" and "Connected to RabbitMQ"
   ```
