# Data Layer Implementation Plan

## Date: February 8, 2026

## Objective
Complete the data layer implementation for the Workforce Management Platform backend, including database models, connections, migrations, and seed data.

---

## Requirements Analysis

### PostgreSQL Database (SQL)
**Purpose:** Store structured, relational domain data
**Requirements:**
- Employees, Departments, Designations (organizational data)
- Projects and Tasks (project management data)
- Many-to-many relationships (Projects ↔ Employees)
- Referential integrity and joins
- At least 50 employee records with related project and task data
- Use migrations for schema management

### MongoDB Database (Document)
**Purpose:** Store document-oriented and operational data
**Requirements:**
- Leave Requests (with embedded approval history)
- Audit Logs (immutable event records)
- Reports (pre-computed aggregations)
- Seed data matching seeded employees

---

## Implementation Status

### ✅ Completed

1. **Model Updates**
   - ✅ Updated Employee model with all required fields:
     - Salary (decimal with precision)
     - JoiningDate
     - Address, City, Country
     - AvatarUrl
   - ✅ Updated Project model (StartDate required)
   - ✅ Updated TaskItem model (proper status workflow)
   - ✅ Added navigation properties for relationships

2. **Entity Framework Configuration**
   - ✅ Configured all entity relationships
   - ✅ Set up foreign keys with proper delete behaviors
   - ✅ Added indexes for performance
   - ✅ Configured JSONB for Skills array
   - ✅ Set decimal precision for Salary

3. **MongoDB Models**
   - ✅ Created LeaveRequest model with ApprovalHistory
   - ✅ Created AuditLog model
   - ✅ Created Report model
   - ✅ Proper BSON attributes and serialization

4. **Database Seeder**
   - ✅ PostgreSQL seeding:
     - 8 Departments
     - 10 Designations
     - 55+ Employees (exceeds 50 requirement)
     - 8 Projects
     - Project Members (many-to-many)
     - Tasks for each project
   - ✅ MongoDB seeding:
     - Leave requests for all employees (2-4 per employee)
     - Proper approval history embedded

5. **Database Initialization**
   - ✅ Added DatabaseSeeder service
   - ✅ Integrated into Program.cs startup
   - ✅ Automatic migration/creation on startup
   - ✅ Automatic seeding on first run

6. **Repository Updates**
   - ✅ Updated MongoDB repositories to use typed models
   - ✅ Proper collection handling

---

## Database Schema Design

### PostgreSQL Schema

**Departments**
- Id (UUID, PK)
- Name (unique)
- Description
- Timestamps and soft delete

**Designations**
- Id (UUID, PK)
- Title (unique)
- Level (1-5)
- Description
- Timestamps

**Employees**
- Id (UUID, PK)
- FirstName, LastName, Email (unique)
- IsActive
- DepartmentId (FK → Departments)
- DesignationId (FK → Designations)
- Salary (decimal 18,2)
- JoiningDate
- Phone, Address, City, Country
- Skills (JSONB array)
- AvatarUrl
- Timestamps and soft delete

**Projects**
- Id (UUID, PK)
- Name, Description
- Status (enum-like string)
- StartDate (required), EndDate (nullable)
- Timestamps and soft delete

**ProjectMembers** (Many-to-Many)
- ProjectId (FK → Projects)
- EmployeeId (FK → Employees)
- Role
- JoinedAt
- Composite PK

**Tasks**
- Id (UUID, PK)
- ProjectId (FK → Projects)
- Title, Description
- Status (workflow: ToDo → InProgress → InReview → Done)
- AssignedToEmployeeId (FK → Employees, nullable)
- Priority (0-3)
- DueDate
- Timestamps and soft delete

### MongoDB Collections

**LeaveRequests**
- _id (ObjectId)
- employeeId (Guid reference)
- employeeName (denormalized)
- leaveType (Sick, Casual, Annual, Unpaid)
- startDate, endDate
- status (Pending, Approved, Rejected, Cancelled)
- reason
- approvalHistory[] (embedded array)
- timestamps

**AuditLogs**
- _id (ObjectId)
- eventId (unique)
- eventType (employee.created, etc.)
- entityType, entityId
- actor
- timestamp
- before/after snapshots
- metadata

**Reports**
- _id (ObjectId)
- reportType
- generatedAt
- data (flexible structure)

---

## Connection Configuration

### PostgreSQL Connection
- **Provider:** Npgsql.EntityFrameworkCore.PostgreSQL
- **Connection String:** From appsettings.json
- **Context:** WorkforceDbContext
- **Migrations:** Entity Framework Core migrations
- **Initialization:** `Database.EnsureCreatedAsync()` (dev) or `Database.MigrateAsync()` (prod)

### MongoDB Connection
- **Driver:** MongoDB.Driver
- **Connection String:** From appsettings.json
- **Database:** workforce_db
- **Collections:** LeaveRequests, AuditLogs, Reports
- **Initialization:** Collections created on first insert

---

## Seed Data Specifications

### PostgreSQL Seed Data
- **8 Departments:** Engineering, Product, Sales, Marketing, HR, Finance, Operations, Support
- **10 Designations:** Junior Developer through Engineering Manager, Product Manager, Sales roles, HR roles
- **55 Employees:** 
  - Random names from common name lists
  - Distributed across departments
  - Various designations
  - Salary range: $50,000 - $150,000
  - Joining dates: Last 2000 days
  - Skills: 3-8 random skills per employee
  - 90% active employees
- **8 Projects:** Mix of Active, Planning, OnHold, Completed
- **Project Members:** 3-8 employees per project
- **Tasks:** 5-15 tasks per active project

### MongoDB Seed Data
- **Leave Requests:** 2-4 per employee (110-220 total)
- **Leave Types:** Sick, Casual, Annual, Unpaid
- **Statuses:** Mix of Pending, Approved, Rejected, Cancelled
- **Approval History:** Embedded in each request
- **Date Range:** Last 365 days

---

## Next Steps

1. **Generate EF Core Migrations**
   ```bash
   cd backend/WorkforceAPI
   dotnet ef migrations add InitialCreate
   ```

2. **Update Program.cs for Production**
   - Replace `EnsureCreatedAsync()` with `MigrateAsync()`
   - Add migration files to repository

3. **Test Database Initialization**
   - Verify PostgreSQL schema creation
   - Verify MongoDB collections
   - Verify seed data population
   - Test relationships and queries

4. **Repository Implementation**
   - Complete repository methods
   - Add query methods (filtering, pagination)
   - Add proper error handling

5. **Service Layer**
   - Implement business logic
   - Add validation
   - Add event publishing

---

## Files Created/Modified

### Created
- `Models/MongoDB/LeaveRequest.cs`
- `Models/MongoDB/AuditLog.cs`
- `Models/MongoDB/Report.cs`
- `Data/DatabaseSeeder.cs`

### Modified
- `Models/Employee.cs` - Added all required fields
- `Models/Project.cs` - Updated with navigation properties
- `Models/TaskItem.cs` - Updated with navigation properties
- `Models/ProjectMember.cs` - Added navigation properties
- `Data/WorkforceDbContext.cs` - Added relationships and constraints
- `Program.cs` - Added database initialization
- `Repositories/LeaveRequestRepository.cs` - Updated to use typed model
- `Repositories/AuditLogRepository.cs` - Updated to use typed model
- `Repositories/ReportRepository.cs` - Updated to use typed model

---

## Testing Checklist

- [ ] Database connections work
- [ ] PostgreSQL schema created correctly
- [ ] MongoDB collections accessible
- [ ] Seed data populated (55+ employees)
- [ ] Relationships work (foreign keys)
- [ ] Many-to-many relationships functional
- [ ] Soft delete works
- [ ] Indexes improve query performance
- [ ] JSONB Skills array works
- [ ] MongoDB embedded arrays work

---

## Notes

- Using `EnsureCreatedAsync()` for development - will switch to migrations for production
- Seed data uses random generation for variety
- Employee IDs are properly linked between PostgreSQL and MongoDB
- All required fields from Domain_Model.txt are implemented
- Navigation properties added for easier querying
