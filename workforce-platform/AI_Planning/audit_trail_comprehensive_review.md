# Comprehensive Audit Trail Review and Fix Plan

## Overview
Review all audit event types and ensure:
1. Events are published correctly
2. Consumer handles all events
3. MongoDB storage works
4. Before/after snapshots contain only entity data (no navigation properties)

## Event Types Analysis

### Employee Events
- ✅ EmployeeCreated - Published, has after snapshot (but includes navigation properties)
- ✅ EmployeeUpdated - Published, has before/after snapshots (but includes navigation properties)
- ✅ EmployeeDeleted - Published, has before snapshot (but includes navigation properties)

### Project Events
- ✅ ProjectCreated - Published, has after snapshot (but includes navigation properties)
- ✅ ProjectUpdated - Published, has before/after snapshots (but includes navigation properties)
- ✅ ProjectDeleted - Published, has before snapshot (but includes navigation properties)
- ✅ ProjectMemberAdded - Published, has before/after snapshots (but includes navigation properties)
- ✅ ProjectMemberRemoved - Published, has before/after snapshots (but includes navigation properties)

### Task Events
- ✅ TaskCreated - Published, has after snapshot (but includes navigation properties)
- ✅ TaskUpdated - Published, has before/after snapshots (but includes navigation properties)
- ✅ TaskDeleted - Published, has before snapshot (but includes navigation properties)
- ✅ TaskStatusUpdated - Published, has before/after snapshots (but includes navigation properties)

### Leave Request Events
- ✅ LeaveRequestCreated - Published, NO before/after snapshots
- ✅ LeaveRequestUpdated - Published, NO before/after snapshots
- ✅ LeaveRequestApproved - Published, NO before/after snapshots
- ✅ LeaveRequestRejected - Published, NO before/after snapshots
- ✅ LeaveRequestCancelled - Published, NO before/after snapshots

### Department Events
- ✅ DepartmentCreated - Published, NO before/after snapshots
- ✅ DepartmentUpdated - Published, NO before/after snapshots
- ✅ DepartmentDeleted - Published, NO before/after snapshots

### Designation Events
- ❌ DesignationCreated - NOT PUBLISHED
- ❌ DesignationUpdated - NOT PUBLISHED
- ❌ DesignationDeleted - NOT PUBLISHED (no delete method exists)

## Issues Found

1. **Navigation Properties in Snapshots**: All entities are serialized with navigation properties (Department, Designation, ProjectMembers, Tasks, etc.)
2. **Missing Before/After Snapshots**: Department, LeaveRequest, and Designation services don't capture snapshots
3. **Missing Event Publishing**: DesignationService doesn't publish any events
4. **Missing Delete Method**: DesignationService has no DeleteAsync method

## Solution

### 1. Create Entity Serialization Helper
Create a helper class to serialize entities without navigation properties:
- `AuditEntitySerializer` class with methods for each entity type
- Serialize only scalar properties and foreign key IDs
- Exclude all navigation properties

### 2. Update All Services
- EmployeeService: Use helper to serialize Employee without navigation properties
- ProjectService: Use helper to serialize Project without ProjectMembers and Tasks
- TaskService: Use helper to serialize TaskItem without Project and AssignedToEmployee
- DepartmentService: Add before/after snapshots with helper
- LeaveRequestService: Add before/after snapshots with helper
- DesignationService: Add event publishing and before/after snapshots

### 3. Verify Consumer
- Consumer uses "#" routing key, so it should handle all events
- Verify ExtractEntityInfo handles all event types correctly

### 4. Verify MongoDB Storage
- AuditLogService should handle all event types
- Verify ConvertToBsonCompatible works for all entity types

## Implementation Steps

1. Create `Workforce.Shared/Helpers/AuditEntitySerializer.cs`
2. Update EmployeeService to use serializer
3. Update ProjectService to use serializer
4. Update TaskService to use serializer
5. Update DepartmentService to add snapshots and use serializer
6. Update LeaveRequestService to add snapshots and use serializer
7. Update DesignationService to add event publishing, snapshots, and serializer
8. Test all event types
