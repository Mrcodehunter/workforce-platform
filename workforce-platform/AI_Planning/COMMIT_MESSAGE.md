# Commit Message

```
refactor: Replace raw string event types with AuditEventType enum

- Created AuditEventType enum in Workforce.Shared/Events with all event types
- Updated IRabbitMqPublisher interface to accept AuditEventType enum instead of string
- Replaced all raw string event types in service classes (Employee, Project, Task, Department, LeaveRequest)
- Added ToRoutingKey() extension method to convert enum to RabbitMQ routing keys
- Fixed bug in DepartmentService.DeleteAsync using wrong event type
- Added .dockerignore to exclude old Cache/EventPublisher directories
```

# PR Title

```
Refactor: Replace raw string event types with centralized AuditEventType enum
```

# PR Description

```
Refactored all audit event types from raw string literals to a centralized AuditEventType enum in the shared library. Updated IRabbitMqPublisher interface and all service classes to use the enum, improving type safety and maintainability. Fixed DepartmentService.DeleteAsync bug that was publishing wrong event type. Added .dockerignore to prevent old Cache/EventPublisher directories from being copied during Docker builds.
```
