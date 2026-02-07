# Workforce Management Platform - Architecture Plan
## Complete System Design & Technology Stack

**Document Version:** 1.0  
**Date:** February 7, 2026  
**Project Type:** Distributed Systems Assignment  
**Stack:** .NET 10, React, TypeScript, MongoDB, SQL Database, Docker

---

## Table of Contents
1. [Executive Summary](#executive-summary)
2. [Technology Stack Recommendations](#technology-stack-recommendations)
3. [System Architecture](#system-architecture)
4. [Domain Model Design](#domain-model-design)
5. [API Design](#api-design)
6. [Worker Services Design](#worker-services-design)
7. [Monorepo Structure](#monorepo-structure)
8. [GitHub Integration & Workflow](#github-integration--workflow)
9. [Development Roadmap](#development-roadmap)
10. [AI-Assisted Development Strategy](#ai-assisted-development-strategy)

---

## Executive Summary

This document provides a complete architectural blueprint for building a Workforce Management Platform as a distributed system. The platform handles employee management, project tracking, task management, and leave/time-off requests using a microservices architecture with event-driven communication.

**Key Architectural Decisions:**
- **Multi-database strategy**: PostgreSQL for relational data, MongoDB for document-oriented data
- **Event-driven architecture**: RabbitMQ for reliable message brokering
- **Polyglot workers**: .NET for audit logging, Node.js for report generation
- **Containerized deployment**: Complete Docker Compose orchestration
- **Monorepo structure**: Separate frontend/backend with shared CI/CD

---

## Technology Stack Recommendations

### 1. SQL Database: PostgreSQL

**Choice:** PostgreSQL 16

**Justification:**
- **ACID compliance**: Critical for employee, department, project, and task data integrity
- **Advanced features**: Native support for JSON columns (useful for flexible metadata), full-text search, and CTEs
- **Robust migration tooling**: Excellent compatibility with Entity Framework Core migrations
- **Wide adoption**: Extensive .NET ecosystem support, Docker images readily available
- **Performance**: Superior query optimizer for complex joins (employees → projects → tasks)
- **Open-source**: No licensing costs, active community

**Alternatives Considered:**
- **SQL Server**: Excellent for .NET but requires licensing, larger Docker images
- **MySQL**: Good performance but less feature-rich, weaker type system
- **Decision**: PostgreSQL offers the best balance of features, performance, and ecosystem fit

### 2. Message Broker: RabbitMQ

**Choice:** RabbitMQ 3.12

**Justification:**
- **Reliability**: Guaranteed message delivery with acknowledgments and persistence
- **Routing flexibility**: Exchange types (topic, fanout, direct) support complex event routing
- **Dead letter queues**: Essential for handling failed message processing
- **Management UI**: Built-in monitoring and debugging interface
- **Multi-language support**: Native clients for both .NET and Node.js
- **Proven track record**: Battle-tested in production distributed systems

**Alternatives Considered:**
- **Redis Pub/Sub**: Simpler but lacks persistence and delivery guarantees
- **Apache Kafka**: Overkill for this scale, complex setup
- **Azure Service Bus / AWS SQS**: Cloud-specific, not Docker-friendly
- **Decision**: RabbitMQ provides the right balance of reliability, features, and ease of deployment

### 3. Frontend UI Library: shadcn/ui + Tailwind CSS

**Choice:** shadcn/ui (component library) + Tailwind CSS

**Justification:**
- **Modern & composable**: Copy-paste components you own, not a dependency
- **TypeScript-first**: Excellent type safety and IntelliSense support
- **Tailwind integration**: Utility-first CSS for rapid, consistent styling
- **Accessibility**: WCAG-compliant components out of the box
- **Customizable**: Full control over component styling and behavior
- **React 18 optimized**: Supports concurrent features, server components ready

**Alternatives Considered:**
- **Material-UI (MUI)**: Heavier bundle, Google-specific design language
- **Ant Design**: Excellent but opinionated, larger bundle size
- **Chakra UI**: Good but shadcn offers more control
- **Decision**: shadcn/ui provides modern, accessible components with complete customization freedom

### 4. Additional Technology Choices

| Component | Technology | Justification |
|-----------|-----------|---------------|
| **Backend API** | .NET 10 Web API | Required by assignment; latest features, native async, minimal APIs |
| **Frontend Framework** | React 18 + TypeScript | Required by assignment; wide adoption, excellent tooling |
| **ORM (SQL)** | Entity Framework Core 9 | Type-safe LINQ queries, code-first migrations, change tracking |
| **MongoDB Driver** | MongoDB.Driver (official .NET) | Native async, LINQ support, aggregation pipeline |
| **API Validation** | FluentValidation | Expressive validation rules, separation of concerns |
| **Logging** | Serilog | Structured logging, multiple sinks (console, file, seq) |
| **API Documentation** | Swagger/OpenAPI (Swashbuckle) | Auto-generated, interactive API docs |
| **Frontend State Management** | Zustand or React Query | Simple, performant state management |
| **HTTP Client (Frontend)** | Axios | Interceptors for auth, better error handling than fetch |
| **Date Handling** | date-fns | Lightweight, tree-shakeable alternative to moment.js |
| **Form Handling** | React Hook Form | Performant, minimal re-renders, TypeScript support |
| **Charts** | Recharts | React-native charts, composable, responsive |
| **Background Worker (.NET)** | .NET BackgroundService | Native worker host, lifecycle management |
| **Background Worker (Node.js)** | Node.js with amqplib | Direct RabbitMQ integration, simple and fast |

---

## System Architecture

### High-Level Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                            Docker Compose                             │
│                                                                       │
│  ┌──────────────┐         ┌──────────────────────────────────┐      │
│  │   Frontend   │────────▶│         API Server              │      │
│  │   (React)    │  HTTP   │        (.NET 10)                │      │
│  │  Port: 3000  │         │        Port: 5000               │      │
│  └──────────────┘         └──────┬─────────────┬────────────┘      │
│                                   │             │                    │
│                          ┌────────┴────┐   ┌────┴──────────┐        │
│                          │             │   │               │        │
│                    ┌─────▼─────┐ ┌────▼────────▼─────┐    │        │
│                    │PostgreSQL │ │     MongoDB        │    │        │
│                    │ Port:5432 │ │    Port:27017      │    │        │
│                    └───────────┘ └────────────────────┘    │        │
│                                                             │        │
│                          ┌──────────────────────────────────┘        │
│                          │                                           │
│                    ┌─────▼──────────┐                                │
│                    │   RabbitMQ     │                                │
│                    │  Port: 5672    │                                │
│                    │  UI: 15672     │                                │
│                    └────┬──────┬────┘                                │
│                         │      │                                     │
│              ┌──────────┘      └──────────┐                          │
│              │                            │                          │
│      ┌───────▼────────┐          ┌────────▼─────────┐               │
│      │  Worker 1      │          │    Worker 2      │               │
│      │  (.NET 10)     │          │   (Node.js)      │               │
│      │ Audit Logging  │          │ Report Generator │               │
│      └────┬──────┬────┘          └────┬────────┬────┘               │
│           │      │                    │        │                     │
│           │      └────────┐           │        └────────┐            │
│           │               │           │                 │            │
│     ┌─────▼─────┐    ┌────▼───────────▼────┐      ┌────▼─────┐      │
│     │PostgreSQL │    │      MongoDB        │      │PostgreSQL│      │
│     │  (Read)   │    │  (Write Audit +     │      │  (Read)  │      │
│     └───────────┘    │   Reports)          │      └──────────┘      │
│                      └─────────────────────┘                         │
└───────────────────────────────────────────────────────────────────────┘
```

### Data Flow Patterns

**1. User Action Flow (e.g., Create Employee)**
```
Frontend → API Server → PostgreSQL (Write)
                      → RabbitMQ (Publish Event: "employee.created")
                      → Return Response to Frontend

RabbitMQ → Worker 1 (Consume Event) → MongoDB (Write Audit Log)
```

**2. Background Report Generation Flow**
```
Worker 2 (Scheduled/Triggered)
  → PostgreSQL (Read employee, project data)
  → MongoDB (Read leave request data)
  → Aggregate & Compute
  → MongoDB (Write to Reports collection)

Frontend → API Server → MongoDB (Read Reports) → Display Dashboard
```

**3. Leave Request Approval Flow**
```
Frontend → API Server → MongoDB (Update leave request, add approval history)
                      → RabbitMQ (Publish: "leave.approved")
                      → Return Response

RabbitMQ → Worker 1 → MongoDB (Audit Log)
        → Worker 2 → Could trigger notification (bonus feature)
```

### Component Responsibilities

#### API Server (.NET 10)
- **Primary Responsibilities:**
  - RESTful API endpoint exposure
  - Business logic orchestration
  - Authentication/authorization (if implemented)
  - Data validation
  - Database transaction management
  - Event publishing to RabbitMQ

- **Database Access:**
  - PostgreSQL: Employees, Departments, Designations, Projects, Tasks
  - MongoDB: Leave Requests, Reports (read-only), Audit Logs (read-only for viewing)

- **No Direct Worker Logic:** API server is stateless and synchronous

#### Worker 1 (.NET Background Service)
- **Purpose:** Audit Trail Generation
- **Event Subscriptions:**
  - `employee.*` (created, updated, deleted)
  - `project.*` (created, updated, deleted)
  - `task.*` (created, updated, status_changed)
  - `leave.*` (submitted, approved, rejected)

- **Processing:**
  - Consumes events from RabbitMQ
  - Reads "before" state from PostgreSQL/MongoDB if needed
  - Creates immutable audit log entries in MongoDB
  - Each log entry includes: timestamp, event type, entity ID, user/actor, before/after snapshot

- **Error Handling:**
  - Retry with exponential backoff (3 attempts)
  - Dead letter queue for persistent failures
  - Idempotency: Uses event ID to prevent duplicate audit logs

#### Worker 2 (Node.js Service)
- **Purpose:** Scheduled Report Generation & Data Aggregation
- **Trigger:** Time-based (e.g., every hour or on-demand via RabbitMQ message)

- **Processing:**
  - Reads from PostgreSQL: Employee counts, project stats, task completion rates
  - Reads from MongoDB: Leave request statistics
  - Computes aggregates:
    - Headcount by department
    - Project progress (tasks completed vs. total)
    - Leave usage by type and status
    - Recent activity summary (last 24 hours)
  - Writes computed reports to MongoDB `Reports` collection

- **Why Node.js?**
  - Demonstrates polyglot architecture
  - Node.js excels at I/O-heavy aggregation tasks
  - Lighter weight for scheduled batch jobs
  - Different runtime reduces single-point-of-failure (technology diversity)

---

## Domain Model Design

### SQL Database Schema (PostgreSQL)

#### Entity Relationship Diagram (Conceptual)

```
Department (1) ──────< (N) Employee (N) ────────> (N) Project
     │                       │                           │
     │                       │                           │
     └─ Designation (1) ────<                           │
                                                         │
                                                         └──────< (N) Task
                                                                     │
                                                             AssignedTo Employee (1)
```

#### Detailed Schema

**Table: Departments**
```sql
CREATE TABLE Departments (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Name VARCHAR(100) NOT NULL UNIQUE,
    Description TEXT,
    CreatedAt TIMESTAMP NOT NULL DEFAULT NOW(),
    UpdatedAt TIMESTAMP NOT NULL DEFAULT NOW(),
    IsDeleted BOOLEAN NOT NULL DEFAULT FALSE
);
```

**Table: Designations**
```sql
CREATE TABLE Designations (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Title VARCHAR(100) NOT NULL UNIQUE,
    Level INT, -- e.g., 1=Junior, 2=Mid, 3=Senior, 4=Lead, 5=Manager
    Description TEXT,
    CreatedAt TIMESTAMP NOT NULL DEFAULT NOW(),
    UpdatedAt TIMESTAMP NOT NULL DEFAULT NOW()
);
```

**Table: Employees**
```sql
CREATE TABLE Employees (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    FirstName VARCHAR(100) NOT NULL,
    LastName VARCHAR(100) NOT NULL,
    Email VARCHAR(255) NOT NULL UNIQUE,
    Phone VARCHAR(20),
    DateOfBirth DATE,
    HireDate DATE NOT NULL,
    DepartmentId UUID NOT NULL REFERENCES Departments(Id),
    DesignationId UUID NOT NULL REFERENCES Designations(Id),
    Skills JSONB, -- Array of skill strings: ["C#", "React", "Docker"]
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    CreatedAt TIMESTAMP NOT NULL DEFAULT NOW(),
    UpdatedAt TIMESTAMP NOT NULL DEFAULT NOW(),
    IsDeleted BOOLEAN NOT NULL DEFAULT FALSE, -- Soft delete
    
    INDEX idx_department (DepartmentId),
    INDEX idx_designation (DesignationId),
    INDEX idx_email (Email),
    INDEX idx_active (IsActive)
);
```

**Table: Projects**
```sql
CREATE TABLE Projects (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Name VARCHAR(200) NOT NULL,
    Description TEXT,
    Status VARCHAR(50) NOT NULL, -- 'Planning', 'Active', 'OnHold', 'Completed', 'Cancelled'
    StartDate DATE,
    EndDate DATE,
    CreatedAt TIMESTAMP NOT NULL DEFAULT NOW(),
    UpdatedAt TIMESTAMP NOT NULL DEFAULT NOW(),
    IsDeleted BOOLEAN NOT NULL DEFAULT FALSE,
    
    INDEX idx_status (Status)
);
```

**Table: ProjectMembers** (Many-to-Many)
```sql
CREATE TABLE ProjectMembers (
    ProjectId UUID NOT NULL REFERENCES Projects(Id) ON DELETE CASCADE,
    EmployeeId UUID NOT NULL REFERENCES Employees(Id) ON DELETE CASCADE,
    Role VARCHAR(50), -- 'Developer', 'Lead', 'QA', etc.
    JoinedAt TIMESTAMP NOT NULL DEFAULT NOW(),
    
    PRIMARY KEY (ProjectId, EmployeeId),
    INDEX idx_employee (EmployeeId)
);
```

**Table: Tasks**
```sql
CREATE TABLE Tasks (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    ProjectId UUID NOT NULL REFERENCES Projects(Id) ON DELETE CASCADE,
    Title VARCHAR(200) NOT NULL,
    Description TEXT,
    Status VARCHAR(50) NOT NULL, -- 'ToDo', 'InProgress', 'InReview', 'Done', 'Blocked'
    Priority VARCHAR(20) NOT NULL, -- 'Low', 'Medium', 'High', 'Urgent'
    AssignedToId UUID REFERENCES Employees(Id) ON DELETE SET NULL,
    DueDate DATE,
    CreatedAt TIMESTAMP NOT NULL DEFAULT NOW(),
    UpdatedAt TIMESTAMP NOT NULL DEFAULT NOW(),
    
    INDEX idx_project (ProjectId),
    INDEX idx_assigned_to (AssignedToId),
    INDEX idx_status (Status),
    INDEX idx_due_date (DueDate)
);
```

**Seed Data Strategy:**
- 10 Departments
- 15 Designations (Junior to Executive levels)
- 50+ Employees distributed across departments
- 15 Projects (various statuses)
- Each project has 3-8 team members
- 100+ Tasks across projects (various statuses)

### MongoDB Schema Design

#### Collection: LeaveRequests

**Document Structure:**
```json
{
  "_id": ObjectId("..."),
  "employeeId": "UUID-string", // References SQL Employee
  "employeeName": "John Doe", // Denormalized for quick display
  "employeeEmail": "john.doe@company.com",
  "departmentName": "Engineering", // Denormalized
  
  "leaveType": "Vacation", // Vacation, Sick, Personal, Bereavement, etc.
  "startDate": ISODate("2026-03-01"),
  "endDate": ISODate("2026-03-05"),
  "totalDays": 5,
  "reason": "Family vacation",
  "status": "Approved", // Pending, Approved, Rejected
  
  "approvalHistory": [ // Embedded array - full lifecycle in one document
    {
      "timestamp": ISODate("2026-02-07T10:00:00Z"),
      "action": "Submitted",
      "actor": null,
      "comments": null
    },
    {
      "timestamp": ISODate("2026-02-08T14:30:00Z"),
      "action": "Approved",
      "actor": "manager@company.com",
      "actorName": "Jane Manager",
      "comments": "Enjoy your vacation!"
    }
  ],
  
  "createdAt": ISODate("2026-02-07T10:00:00Z"),
  "updatedAt": ISODate("2026-02-08T14:30:00Z")
}
```

**Indexes:**
```javascript
db.LeaveRequests.createIndex({ employeeId: 1 });
db.LeaveRequests.createIndex({ status: 1 });
db.LeaveRequests.createIndex({ leaveType: 1 });
db.LeaveRequests.createIndex({ startDate: 1, endDate: 1 });
```

**Why MongoDB for Leave Requests?**
- Self-contained documents: Full approval history embedded, no joins needed
- Flexible schema: Easy to add new fields (e.g., attachments, notifications)
- Natural workflow representation: Each status change appended to history array
- Audit trail built-in: No separate tracking table needed

#### Collection: AuditLogs

**Document Structure:**
```json
{
  "_id": ObjectId("..."),
  "eventId": "unique-event-uuid", // For idempotency
  "eventType": "employee.updated", // Hierarchical naming: entity.action
  "entityType": "Employee",
  "entityId": "UUID-string",
  "timestamp": ISODate("2026-02-07T15:00:00Z"),
  "actor": "admin@company.com",
  "actorName": "Admin User",
  
  "changes": {
    "before": {
      "departmentId": "old-uuid",
      "departmentName": "Sales"
    },
    "after": {
      "departmentId": "new-uuid",
      "departmentName": "Engineering"
    }
  },
  
  "metadata": {
    "ipAddress": "192.168.1.100",
    "userAgent": "Mozilla/5.0...",
    "source": "API"
  }
}
```

**Indexes:**
```javascript
db.AuditLogs.createIndex({ eventId: 1 }, { unique: true }); // Idempotency
db.AuditLogs.createIndex({ entityType: 1, entityId: 1 });
db.AuditLogs.createIndex({ timestamp: -1 }); // Recent first
db.AuditLogs.createIndex({ eventType: 1 });
```

**Why MongoDB for Audit Logs?**
- Write-heavy: MongoDB handles high-volume inserts efficiently
- Immutable: Once written, never modified
- Flexible schema: Different event types have different metadata
- Time-series nature: Indexed by timestamp for recent activity queries

#### Collection: Reports

**Document Structure:**
```json
{
  "_id": ObjectId("..."),
  "reportType": "DepartmentHeadcount",
  "generatedAt": ISODate("2026-02-07T12:00:00Z"),
  "data": {
    "totalEmployees": 52,
    "byDepartment": [
      { "departmentName": "Engineering", "count": 25, "activeCount": 24 },
      { "departmentName": "Sales", "count": 15, "activeCount": 15 },
      { "departmentName": "HR", "count": 5, "activeCount": 5 }
    ]
  }
}
```

```json
{
  "_id": ObjectId("..."),
  "reportType": "ProjectProgress",
  "generatedAt": ISODate("2026-02-07T12:00:00Z"),
  "data": {
    "projects": [
      {
        "projectId": "uuid",
        "projectName": "Mobile App Redesign",
        "totalTasks": 45,
        "completedTasks": 32,
        "progress": 71.1,
        "teamSize": 6
      }
    ]
  }
}
```

```json
{
  "_id": ObjectId("..."),
  "reportType": "LeaveStatistics",
  "generatedAt": ISODate("2026-02-07T12:00:00Z"),
  "data": {
    "totalRequests": 128,
    "byStatus": {
      "Pending": 5,
      "Approved": 110,
      "Rejected": 13
    },
    "byType": {
      "Vacation": 85,
      "Sick": 30,
      "Personal": 13
    }
  }
}
```

**Indexes:**
```javascript
db.Reports.createIndex({ reportType: 1, generatedAt: -1 });
```

**Why MongoDB for Reports?**
- Pre-computed aggregates: Fast dashboard loading
- Complex nested structures: Department breakdowns, project lists
- Time-series snapshots: Historical report comparison possible

**Seed Data Strategy:**
- 50+ Leave Requests distributed across employees
- Various statuses and types
- Approval histories with 1-3 entries each
- Initial report snapshot generated by Worker 2

---

## API Design

### RESTful API Principles

**Base URL:** `http://localhost:5000/api/v1`

**Versioning:** URL-based versioning (`/api/v1/`) for clear contract management

**Response Format:** JSON with consistent structure
```json
{
  "success": true,
  "data": { ... },
  "message": "Operation successful",
  "errors": null
}
```

Error response:
```json
{
  "success": false,
  "data": null,
  "message": "Validation failed",
  "errors": [
    {
      "field": "email",
      "message": "Email is already in use"
    }
  ]
}
```

### Endpoint Structure

#### Employees Endpoints

| Method | Endpoint | Description | Request Body | Response |
|--------|----------|-------------|--------------|----------|
| GET | `/api/v1/employees` | List employees with pagination, filtering, sorting | Query params: `page`, `pageSize`, `department`, `search`, `sortBy`, `isActive` | Paginated employee list |
| GET | `/api/v1/employees/{id}` | Get employee details | - | Employee with projects, leave summary |
| POST | `/api/v1/employees` | Create new employee | Employee DTO | Created employee |
| PUT | `/api/v1/employees/{id}` | Update employee | Employee DTO | Updated employee |
| DELETE | `/api/v1/employees/{id}` | Soft delete employee | - | Success message |
| GET | `/api/v1/employees/{id}/projects` | Get employee's projects | - | Project list |
| GET | `/api/v1/employees/{id}/leaves` | Get employee's leave requests | - | Leave request list (from MongoDB) |
| GET | `/api/v1/employees/{id}/audit` | Get employee's audit trail | - | Audit log entries (from MongoDB) |

#### Departments & Designations Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/departments` | List all departments |
| GET | `/api/v1/departments/{id}` | Get department details with employee count |
| POST | `/api/v1/departments` | Create department |
| PUT | `/api/v1/departments/{id}` | Update department |
| DELETE | `/api/v1/departments/{id}` | Delete department (if no employees) |
| GET | `/api/v1/designations` | List all designations |
| POST | `/api/v1/designations` | Create designation |

#### Projects & Tasks Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/projects` | List projects with filters |
| GET | `/api/v1/projects/{id}` | Get project details with team members |
| POST | `/api/v1/projects` | Create project |
| PUT | `/api/v1/projects/{id}` | Update project |
| DELETE | `/api/v1/projects/{id}` | Soft delete project |
| POST | `/api/v1/projects/{id}/members` | Add team member |
| DELETE | `/api/v1/projects/{projectId}/members/{employeeId}` | Remove team member |
| GET | `/api/v1/projects/{id}/tasks` | Get project tasks |
| POST | `/api/v1/projects/{id}/tasks` | Create task for project |
| PUT | `/api/v1/tasks/{id}` | Update task (including status change) |
| DELETE | `/api/v1/tasks/{id}` | Delete task |
| PATCH | `/api/v1/tasks/{id}/assign` | Assign task to employee |

#### Leave Requests Endpoints (MongoDB)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/leaves` | List leave requests with filters |
| GET | `/api/v1/leaves/{id}` | Get leave request with full history |
| POST | `/api/v1/leaves` | Submit new leave request |
| PATCH | `/api/v1/leaves/{id}/approve` | Approve leave request |
| PATCH | `/api/v1/leaves/{id}/reject` | Reject leave request |

#### Dashboard & Reports Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/dashboard/summary` | Get latest dashboard summary report |
| GET | `/api/v1/reports/departments` | Department headcount report |
| GET | `/api/v1/reports/projects` | Project progress report |
| GET | `/api/v1/reports/leaves` | Leave statistics report |

#### Audit Trail Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/audit` | System-wide audit log with filters |
| GET | `/api/v1/audit/{entityType}/{entityId}` | Entity-specific audit trail |

### Event Publishing Strategy

**Event Naming Convention:** `entity.action`

**Events Published:**
- `employee.created`
- `employee.updated`
- `employee.deleted`
- `department.created`
- `department.updated`
- `project.created`
- `project.updated`
- `project.deleted`
- `task.created`
- `task.updated`
- `task.status_changed`
- `leave.submitted`
- `leave.approved`
- `leave.rejected`

**Event Payload Example:**
```json
{
  "eventId": "unique-uuid",
  "eventType": "employee.updated",
  "timestamp": "2026-02-07T15:00:00Z",
  "entityType": "Employee",
  "entityId": "employee-uuid",
  "actor": "admin@company.com",
  "changes": {
    "before": { "departmentId": "old-uuid" },
    "after": { "departmentId": "new-uuid" }
  }
}
```

### API Layer Architecture

```
Controllers (HTTP)
    ↓
Services (Business Logic)
    ↓
Repositories (Data Access Abstraction)
    ↓
[PostgreSQL Context] [MongoDB Client]
```

**Key Patterns:**
- **Dependency Injection:** All services and repositories injected via .NET DI
- **Repository Pattern:** Abstraction over EF Core and MongoDB driver
- **Unit of Work:** Transaction management for multi-table operations
- **DTOs:** Separate models for API contracts vs. database entities
- **AutoMapper:** Entity ↔ DTO mapping
- **FluentValidation:** Request validation

---

## Worker Services Design

### Worker 1: Audit Trail Service (.NET)

**Technology:** .NET 10 BackgroundService with RabbitMQ consumer

**File Structure:**
```
WorkerService.AuditLogger/
├── Program.cs
├── AuditWorker.cs (BackgroundService)
├── Services/
│   ├── RabbitMqConsumer.cs
│   └── AuditLogService.cs
├── Models/
│   └── AuditLogEntry.cs
└── appsettings.json
```

**Event Processing Flow:**
1. Subscribe to all event topics via RabbitMQ topic exchange
2. Receive event message
3. Check if event already processed (query MongoDB by `eventId`)
4. If new, create audit log entry
5. Fetch "before" state if needed (query PostgreSQL/MongoDB)
6. Write audit log to MongoDB
7. Acknowledge message to RabbitMQ

**Idempotency Implementation:**
```csharp
// Check if already processed
var existing = await _auditCollection
    .Find(a => a.EventId == eventId)
    .FirstOrDefaultAsync();

if (existing != null)
{
    _logger.LogInformation("Event {EventId} already processed, skipping", eventId);
    return; // Idempotent - don't duplicate
}

// Process and insert
await _auditCollection.InsertOneAsync(auditLog);
```

**Retry Strategy:**
- Initial attempt
- Retry after 5 seconds
- Retry after 30 seconds
- After 3 failures → Dead Letter Queue

**Health Check:**
```csharp
// Expose health endpoint or log heartbeat
public class WorkerHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken)
    {
        // Check RabbitMQ connection
        // Check MongoDB connection
        return Task.FromResult(HealthCheckResult.Healthy());
    }
}
```

### Worker 2: Report Generator (Node.js)

**Technology:** Node.js with `amqplib` for RabbitMQ, `pg` for PostgreSQL, `mongodb` driver

**File Structure:**
```
worker-report-generator/
├── src/
│   ├── index.js
│   ├── reportGenerator.js
│   ├── consumers/
│   │   └── scheduleConsumer.js
│   ├── services/
│   │   ├── postgresService.js
│   │   └── mongoService.js
│   └── config/
│       └── database.js
├── package.json
└── Dockerfile
```

**Trigger Mechanisms:**
1. **Time-based:** Internal scheduler (e.g., `node-cron`) runs every hour
2. **Event-based:** Listens for `report.generate` events from RabbitMQ (for on-demand generation)

**Report Generation Process:**
```javascript
async function generateDashboardReport() {
  // 1. Query PostgreSQL
  const employeeStats = await pg.query(`
    SELECT d.Name, COUNT(e.Id) as Count
    FROM Departments d
    LEFT JOIN Employees e ON e.DepartmentId = d.Id AND e.IsDeleted = false
    GROUP BY d.Name
  `);

  const projectStats = await pg.query(`
    SELECT 
      p.Id, p.Name,
      COUNT(t.Id) as TotalTasks,
      COUNT(CASE WHEN t.Status = 'Done' THEN 1 END) as CompletedTasks,
      COUNT(DISTINCT pm.EmployeeId) as TeamSize
    FROM Projects p
    LEFT JOIN Tasks t ON t.ProjectId = p.Id
    LEFT JOIN ProjectMembers pm ON pm.ProjectId = p.Id
    WHERE p.IsDeleted = false
    GROUP BY p.Id, p.Name
  `);

  // 2. Query MongoDB
  const leaveStats = await mongoDb.collection('LeaveRequests').aggregate([
    {
      $group: {
        _id: '$status',
        count: { $sum: 1 }
      }
    }
  ]).toArray();

  // 3. Combine and write report
  const report = {
    reportType: 'DashboardSummary',
    generatedAt: new Date(),
    data: {
      departments: employeeStats,
      projects: projectStats,
      leaveStatistics: leaveStats
    }
  };

  await mongoDb.collection('Reports').insertOne(report);
  logger.info('Dashboard report generated');
}
```

**Why Node.js for This Worker?**
- **I/O efficiency:** Node.js excels at concurrent database reads across PostgreSQL and MongoDB
- **Lightweight:** Smaller Docker image, faster startup
- **JSON native:** Natural fit for aggregating and transforming JSON data
- **Scheduled jobs:** Libraries like `node-cron` or `bull` for job scheduling
- **Polyglot demonstration:** Shows ability to integrate multiple technology stacks

**Structured Logging:**
```javascript
const winston = require('winston');

const logger = winston.createLogger({
  level: 'info',
  format: winston.format.combine(
    winston.format.timestamp(),
    winston.format.json()
  ),
  transports: [
    new winston.transports.Console()
  ]
});
```

---

## Monorepo Structure

### Repository Organization

```
workforce-platform/
├── .github/
│   └── workflows/
│       └── ci-cd.yml
├── backend/
│   ├── WorkforceAPI/
│   │   ├── WorkforceAPI.csproj
│   │   ├── Program.cs
│   │   ├── Controllers/
│   │   ├── Services/
│   │   ├── Repositories/
│   │   ├── Models/
│   │   ├── DTOs/
│   │   ├── Data/
│   │   │   ├── PostgresContext.cs
│   │   │   └── MongoContext.cs
│   │   ├── Migrations/
│   │   └── appsettings.json
│   ├── WorkerService.AuditLogger/
│   │   ├── WorkerService.AuditLogger.csproj
│   │   ├── Program.cs
│   │   ├── AuditWorker.cs
│   │   └── Services/
│   └── backend.sln
├── frontend/
│   ├── package.json
│   ├── tsconfig.json
│   ├── vite.config.ts
│   ├── public/
│   ├── src/
│   │   ├── main.tsx
│   │   ├── App.tsx
│   │   ├── components/
│   │   │   ├── employees/
│   │   │   ├── projects/
│   │   │   ├── leaves/
│   │   │   ├── dashboard/
│   │   │   └── common/
│   │   ├── pages/
│   │   ├── services/
│   │   │   └── api.ts
│   │   ├── hooks/
│   │   ├── types/
│   │   └── utils/
│   └── Dockerfile
├── workers/
│   └── report-generator/
│       ├── package.json
│       ├── src/
│       │   ├── index.js
│       │   └── reportGenerator.js
│       └── Dockerfile
├── docker-compose.yml
├── .env.example
├── README.md
├── AI-WORKFLOW.md
├── ARCHITECTURE.md (this document)
└── .gitignore
```

### Rationale for Monorepo Structure

**Benefits:**
1. **Single source of truth:** All code in one repository
2. **Atomic commits:** Frontend + backend changes in same commit
3. **Easier refactoring:** Cross-cutting changes in one PR
4. **Shared CI/CD:** Single pipeline for all components
5. **Simplified dependency management:** Clear project boundaries

**Separation Strategy:**
- **Backend:** .NET solution with multiple projects
- **Frontend:** Separate Node.js project
- **Workers:** Separate directory for non-.NET workers

### File Naming Conventions

**Backend (.NET):**
- PascalCase for files: `EmployeeController.cs`, `EmployeeService.cs`
- Namespaces match folder structure: `WorkforceAPI.Controllers`

**Frontend (React):**
- PascalCase for components: `EmployeeList.tsx`, `ProjectCard.tsx`
- camelCase for utilities: `apiClient.ts`, `formatDate.ts`
- Kebab-case for directories: `employee-management/`, `leave-requests/`

**Configuration:**
- Lowercase with hyphens: `docker-compose.yml`, `.env.example`

---

## GitHub Integration & Workflow

### Step 1: Initialize Git Repository

```bash
# In your local development directory
cd workforce-platform
git init
git add .
git commit -m "Initial commit: Project structure and Docker setup"
```

### Step 2: Create GitHub Repository

1. Go to https://github.com/new
2. Create new repository: `workforce-platform`
3. **Do NOT** initialize with README (we already have one)
4. Set visibility: Public (for assignment submission)

### Step 3: Connect Local to GitHub

```bash
# Add remote origin
git remote add origin https://github.com/YOUR_USERNAME/workforce-platform.git

# Push to main branch
git branch -M main
git push -u origin main
```

### Step 4: Branch Strategy

**Main Branch:** Production-ready code (protected)

**Development Workflow:**
```bash
# Feature development
git checkout -b feature/employee-management
# ... make changes ...
git add .
git commit -m "feat: Add employee CRUD endpoints"
git push origin feature/employee-management

# Create Pull Request on GitHub
# Merge to main after CI passes
```

**Recommended Branch Naming:**
- `feature/` - New features
- `fix/` - Bug fixes
- `refactor/` - Code refactoring
- `docs/` - Documentation updates
- `test/` - Test additions

### Step 5: Git Configuration

**.gitignore:**
```gitignore
# .NET
bin/
obj/
*.user
*.suo
.vs/

# Node.js
node_modules/
npm-debug.log
yarn-error.log
dist/
build/

# Environment
.env
.env.local
*.env

# IDEs
.idea/
.vscode/
*.swp
*.swo

# OS
.DS_Store
Thumbs.db

# Docker
docker-compose.override.yml

# Logs
logs/
*.log
```

**.env.example:**
```bash
# PostgreSQL
POSTGRES_HOST=postgres
POSTGRES_PORT=5432
POSTGRES_DB=workforce_db
POSTGRES_USER=admin
POSTGRES_PASSWORD=changeme

# MongoDB
MONGO_HOST=mongodb
MONGO_PORT=27017
MONGO_DB=workforce_db
MONGO_USER=admin
MONGO_PASSWORD=changeme

# RabbitMQ
RABBITMQ_HOST=rabbitmq
RABBITMQ_PORT=5672
RABBITMQ_USER=guest
RABBITMQ_PASSWORD=guest

# API
API_PORT=5000
API_BASE_URL=http://localhost:5000

# Frontend
VITE_API_BASE_URL=http://localhost:5000/api/v1
FRONTEND_PORT=3000
```

### Step 6: Commit Message Conventions

**Format:** `type(scope): message`

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation
- `style`: Formatting, no code change
- `refactor`: Code restructuring
- `test`: Adding tests
- `chore`: Maintenance tasks

**Examples:**
```
feat(employees): Add employee creation endpoint
fix(tasks): Resolve task status update bug
docs(readme): Update setup instructions
refactor(api): Extract validation to separate service
test(employees): Add employee service unit tests
chore(docker): Update PostgreSQL to version 16
```

### Step 7: CI/CD Pipeline (GitHub Actions)

**File:** `.github/workflows/ci-cd.yml`

```yaml
name: CI/CD Pipeline

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  backend-build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '10.0.x'
      
      - name: Restore dependencies
        run: dotnet restore backend/backend.sln
      
      - name: Build
        run: dotnet build backend/backend.sln --no-restore
      
      - name: Test
        run: dotnet test backend/backend.sln --no-build --verbosity normal

  frontend-build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup Node.js
        uses: actions/setup-node@v3
        with:
          node-version: '20'
      
      - name: Install dependencies
        working-directory: ./frontend
        run: npm ci
      
      - name: Lint
        working-directory: ./frontend
        run: npm run lint
      
      - name: Build
        working-directory: ./frontend
        run: npm run build

  worker-build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup Node.js
        uses: actions/setup-node@v3
        with:
          node-version: '20'
      
      - name: Install dependencies
        working-directory: ./workers/report-generator
        run: npm ci
      
      - name: Lint
        working-directory: ./workers/report-generator
        run: npm run lint

  docker-build:
    runs-on: ubuntu-latest
    needs: [backend-build, frontend-build, worker-build]
    steps:
      - uses: actions/checkout@v3
      
      - name: Build Docker images
        run: docker-compose build
      
      - name: Verify Docker Compose
        run: docker-compose config
```

### Step 8: README Badge

Add to top of README.md:
```markdown
[![CI/CD Pipeline](https://github.com/YOUR_USERNAME/workforce-platform/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/YOUR_USERNAME/workforce-platform/actions/workflows/ci-cd.yml)
```

### Monorepo Management Best Practices

1. **Clear ownership:** Each directory has a responsible team/person
2. **Independent versioning:** Backend, frontend, workers can version independently
3. **Shared tooling:** ESLint, Prettier configs at root level
4. **Documentation:** Each major directory has its own README
5. **Consistent code style:** EditorConfig at root for cross-language consistency

**.editorconfig** (root):
```ini
root = true

[*]
charset = utf-8
end_of_line = lf
insert_final_newline = true
trim_trailing_whitespace = true

[*.{cs,csproj}]
indent_style = space
indent_size = 4

[*.{js,ts,tsx,json}]
indent_style = space
indent_size = 2

[*.md]
trim_trailing_whitespace = false
```

---

## Development Roadmap

### Phase 1: Project Setup (Days 1-2)

**Deliverables:**
- [x] GitHub repository created
- [ ] Monorepo structure established
- [ ] Docker Compose configuration
- [ ] PostgreSQL + MongoDB containers
- [ ] RabbitMQ container
- [ ] Environment configuration
- [ ] README with setup instructions

**Steps:**
1. Create directory structure
2. Initialize .NET solution and projects
3. Initialize React app with Vite + TypeScript
4. Initialize Node.js worker project
5. Create docker-compose.yml
6. Test: `docker-compose up` should start all databases
7. Commit and push to GitHub

### Phase 2: Backend Foundation (Days 3-5)

**Deliverables:**
- [ ] Entity Framework Core setup with PostgreSQL
- [ ] MongoDB connection and configuration
- [ ] Domain models created
- [ ] Database migrations
- [ ] Seed data scripts
- [ ] RabbitMQ producer service
- [ ] Basic API structure

**Steps:**
1. Define EF Core entities (Employee, Department, Designation, Project, Task)
2. Create DbContext with configurations
3. Add initial migration
4. Create seed data
5. MongoDB models and collections
6. RabbitMQ connection service
7. Test database connections

### Phase 3: API Development (Days 6-10)

**Deliverables:**
- [ ] Employee CRUD endpoints
- [ ] Department & Designation endpoints
- [ ] Project & Task endpoints
- [ ] Leave request endpoints (MongoDB)
- [ ] Pagination & filtering implementation
- [ ] FluentValidation setup
- [ ] Event publishing to RabbitMQ
- [ ] Swagger documentation

**Steps:**
1. Implement repository pattern for PostgreSQL
2. Implement repository for MongoDB
3. Create DTOs and AutoMapper profiles
4. Build controllers with validation
5. Integrate event publishing
6. Add Swagger configuration
7. Test all endpoints with Postman/Thunder Client

### Phase 4: Worker Services (Days 11-13)

**Deliverables:**
- [ ] .NET Audit Logger worker
- [ ] Node.js Report Generator worker
- [ ] RabbitMQ consumer implementations
- [ ] Retry and error handling
- [ ] Health checks
- [ ] Logging configuration

**Steps:**
1. Create .NET BackgroundService
2. Implement RabbitMQ consumer
3. Audit log creation logic
4. Idempotency checks
5. Create Node.js worker
6. Report generation queries
7. Scheduled execution setup
8. Test workers with manual events

### Phase 5: Frontend Development (Days 14-20)

**Deliverables:**
- [ ] React app structure with routing
- [ ] shadcn/ui components integrated
- [ ] Employee management views
- [ ] Project & Task management views
- [ ] Leave request views
- [ ] Dashboard with reports
- [ ] Audit trail viewer
- [ ] Form validation
- [ ] Error handling & loading states

**Steps:**
1. Setup React Router
2. Create API service layer with Axios
3. Install and configure shadcn/ui
4. Build employee list (pagination, filtering, sorting)
5. Build employee detail page
6. Build project list and detail
7. Build task board/workflow UI
8. Build leave request forms and list
9. Build dashboard with charts (Recharts)
10. Build audit log viewer
11. Responsive design and polish

### Phase 6: Integration & Testing (Days 21-23)

**Deliverables:**
- [ ] End-to-end integration tests
- [ ] Unit tests for critical services
- [ ] API integration tests
- [ ] Frontend component tests
- [ ] Docker Compose verification
- [ ] Performance testing

**Steps:**
1. Write backend unit tests
2. Write integration tests with test database
3. Frontend component tests with React Testing Library
4. Test full workflows end-to-end
5. Load testing with k6 or similar
6. Fix bugs discovered during testing

### Phase 7: CI/CD & Documentation (Days 24-25)

**Deliverables:**
- [ ] GitHub Actions CI/CD pipeline
- [ ] Comprehensive README
- [ ] AI-WORKFLOW.md
- [ ] Architecture diagrams (Mermaid or draw.io)
- [ ] API documentation
- [ ] Known limitations documented

**Steps:**
1. Create GitHub Actions workflow
2. Add status badge to README
3. Write complete setup instructions
4. Document all technology choices
5. Create architecture diagrams
6. Write AI-WORKFLOW.md (document AI usage throughout)
7. Final review and polish

### Phase 8: Bonus Features (Optional, Days 26-28)

**Deliverables:**
- [ ] Full-text search implementation
- [ ] E2E tests with Playwright or Cypress
- [ ] Cloud deployment (Azure, AWS, or GCP)
- [ ] Advanced dashboard charts
- [ ] Real-time notifications
- [ ] API rate limiting

---

## AI-Assisted Development Strategy

### Recommended AI Tools

**Primary Coding Assistants:**
1. **Claude Code** - For terminal-based development, file operations, complex refactoring
2. **GitHub Copilot** - For in-IDE code completion and suggestions
3. **Cursor** - For AI-powered coding with codebase context

**Supplementary Tools:**
4. **ChatGPT/Claude** - For architectural discussions, debugging, documentation
5. **Codeium** - Free alternative to Copilot

### How to Use AI Effectively

**Planning Phase:**
- Discuss architecture decisions with Claude/ChatGPT
- Generate domain model diagrams
- Create API endpoint specifications
- Draft database schemas

**Code Generation:**
- Use Copilot/Cursor for boilerplate code (controllers, DTOs, models)
- Generate repository implementations
- Create validation rules
- Write test scaffolding

**Debugging:**
- Paste error messages into AI for explanations
- Ask for debugging strategies
- Code review with AI before committing

**Documentation:**
- Generate code comments
- Create README sections
- Write API documentation

### AI-WORKFLOW.md Template

Your AI-WORKFLOW.md should cover:

```markdown
# AI-Assisted Development Workflow

## Tools Used
- **Claude Code**: Architecture planning, complex file operations
- **GitHub Copilot**: Real-time code completion
- **ChatGPT**: Debugging assistance, documentation

## Planning Phase
[Describe how AI helped with architecture decisions]

Example:
> I used Claude to design the database schema by providing the requirements 
> and asking for optimal table structures. Claude suggested using UUID 
> primary keys and explained the benefits of soft deletes for audit trails.

## Code Generation
[List which components were AI-generated vs. hand-written]

Example:
- 80% AI-generated: DTOs, AutoMapper profiles, initial controllers
- 50% AI-assisted: Business logic services (AI suggestions, manual refinement)
- 20% AI-generated: Complex queries, custom validation

## Debugging & Iteration
[At least one specific failure case and how you resolved it]

Example:
> GitHub Copilot generated a RabbitMQ consumer that didn't handle 
> message acknowledgment properly, causing messages to be reprocessed. 
> I identified the issue by reviewing RabbitMQ management console and 
> manually added proper `channel.BasicAck()` calls.

## Model Behavior Observations
[If using multiple AI tools, compare their strengths]

Example:
> Copilot excelled at generating repetitive CRUD code but struggled with 
> complex business logic. Claude Code was better at understanding multi-file 
> refactoring and architectural patterns.

## Reflection
**Where AI Helped Most:**
- Rapid prototyping and boilerplate reduction
- Suggesting best practices I wasn't aware of
- Debugging cryptic error messages

**Where AI Fell Short:**
- Understanding project-specific business rules
- Optimizing complex database queries
- Integration between unfamiliar technologies

**Future Improvements:**
- Would provide more context to AI upfront (domain model, relationships)
- Use AI for test generation earlier in the process
- Create a prompt library for common tasks
```

### Critical Success Factors

1. **Always understand AI-generated code** - Never copy-paste without comprehension
2. **Review and test** - AI makes mistakes; verify everything
3. **Incremental development** - Build small, test often
4. **Document as you go** - Keep notes on AI usage for AI-WORKFLOW.md
5. **Ask "why"** - Request explanations for AI suggestions to learn

---

## Conclusion

This architecture plan provides a comprehensive blueprint for building the Workforce Management Platform. The key success factors are:

1. **Clear separation of concerns:** SQL for relational data, MongoDB for documents, workers for async processing
2. **Event-driven design:** Decoupled services communicate via RabbitMQ
3. **Polyglot architecture:** Demonstrates .NET, Node.js, React expertise
4. **Production-ready practices:** Proper error handling, logging, health checks
5. **Docker orchestration:** Complete containerized deployment

**Next Steps:**
1. Review this plan and ask questions
2. Begin Phase 1: Project setup and Docker configuration
3. Establish GitHub repository and CI/CD pipeline
4. Implement backend foundation
5. Document AI usage throughout

This is an ambitious project that showcases distributed systems knowledge, full-stack development skills, and modern DevOps practices. The modular design allows for incremental development and testing at each phase.

**Questions or clarifications needed before proceeding?**
