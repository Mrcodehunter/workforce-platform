# Projects and Tasks Implementation Plan

## Date: February 9, 2026

## Objective
Complete the implementation of Project and Task management features per Frontend_Spec.txt requirements, including Project Detail page with Task Board, Create Project functionality, member count display, and comprehensive task tracking.

---

## Requirements Analysis

### From Frontend_Spec.txt

**Project & Task Views:**
1. **Project List**: Overview of all projects with status, team size, and progress indication. Must support creating new projects.
2. **Project Detail & Task Board**: 
   - Displays project information and its team members
   - Tasks displayed in workflow states (Kanban-style)
   - Users must be able to create tasks
   - Assign tasks to team members
   - Transition task status
   - View should make current state immediately understandable

---

## Current Issues Identified

### 1. Backend Issues

#### ProjectRepository
- ❌ `GetAllAsync()` doesn't include `ProjectMembers` or `Tasks` - member count shows 0
- ❌ `GetByIdAsync()` doesn't include navigation properties (ProjectMembers with Employee, Tasks with AssignedToEmployee)
- ❌ `CreateAsync()` and `UpdateAsync()` may have navigation property issues (similar to Employee)

#### TaskRepository
- ❌ No method to get tasks by ProjectId
- ❌ No method to get tasks by EmployeeId (AssignedToEmployeeId)
- ❌ `GetAllAsync()` doesn't include navigation properties (Project, AssignedToEmployee)
- ❌ `GetByIdAsync()` doesn't include navigation properties

#### Controllers
- ❌ ProjectsController needs FluentValidation
- ❌ TasksController needs FluentValidation
- ❌ Missing endpoints for:
  - Get tasks by project ID: `GET /api/projects/{id}/tasks`
  - Get tasks by employee ID: `GET /api/employees/{id}/tasks`
  - Create task within project: `POST /api/projects/{id}/tasks`
  - Update task status: `PATCH /api/tasks/{id}/status`

### 2. Frontend Issues

#### Missing Components
- ❌ `ProjectDetail.tsx` - Doesn't exist
- ❌ `ProjectCreate.tsx` - Doesn't exist
- ❌ `TaskBoard.tsx` - Kanban-style task board component
- ❌ `TaskCard.tsx` - Individual task card component
- ❌ `TaskCreateModal.tsx` or `TaskCreateForm.tsx` - Task creation form

#### Missing Routes
- ❌ `/projects/:id` - Project detail route
- ❌ `/projects/new` - Create project route

#### Missing Hooks
- ❌ `useTasks.ts` - Task management hooks
- ❌ `useProjectTasks.ts` - Get tasks for a specific project
- ❌ `useEmployeeTasks.ts` - Get tasks assigned to an employee

#### ProjectList Issues
- ⚠️ Shows member count but backend may not be loading ProjectMembers
- ⚠️ Task count may not be accurate if Tasks aren't loaded

---

## Implementation Plan

### Phase 1: Backend Fixes

#### 1.1 Fix ProjectRepository
**File:** `backend/WorkforceAPI/Repositories/ProjectRepository.cs`

**Changes:**
- Update `GetAllAsync()` to include ProjectMembers and Tasks counts
- Update `GetByIdAsync()` to include:
  - ProjectMembers with Employee navigation
  - Tasks with AssignedToEmployee navigation
  - Break circular references (similar to Employee fix)
- Fix `CreateAsync()` and `UpdateAsync()` to handle navigation properties correctly

#### 1.2 Fix TaskRepository
**File:** `backend/WorkforceAPI/Repositories/TaskRepository.cs`

**Changes:**
- Add `GetByProjectIdAsync(Guid projectId)` method
- Add `GetByEmployeeIdAsync(Guid employeeId)` method
- Update `GetAllAsync()` to include Project and AssignedToEmployee
- Update `GetByIdAsync()` to include navigation properties
- Fix `CreateAsync()` and `UpdateAsync()` to handle navigation properties

#### 1.3 Add Task Endpoints to Controllers
**Files:** 
- `backend/WorkforceAPI/Controllers/ProjectsController.cs`
- `backend/WorkforceAPI/Controllers/TasksController.cs`

**New Endpoints:**
- `GET /api/projects/{id}/tasks` - Get all tasks for a project
- `GET /api/employees/{id}/tasks` - Get all tasks assigned to an employee
- `POST /api/projects/{id}/tasks` - Create task within a project
- `PATCH /api/tasks/{id}/status` - Update task status (workflow transition)

#### 1.4 Add FluentValidation
**Files:**
- `backend/WorkforceAPI/Validators/ProjectValidator.cs` (new)
- `backend/WorkforceAPI/Validators/TaskValidator.cs` (new)

**Validation Rules:**
- **Project**: Name (required, max 200), Status (enum), StartDate (required, not future), EndDate (after StartDate)
- **Task**: Title (required, max 200), ProjectId (required), Status (enum), Priority (0-3), DueDate (optional, not past)

#### 1.5 Update Services
**Files:**
- `backend/WorkforceAPI/Services/ProjectService.cs`
- `backend/WorkforceAPI/Services/TaskService.cs`

**Add Methods:**
- `GetTasksByProjectIdAsync(Guid projectId)`
- `GetTasksByEmployeeIdAsync(Guid employeeId)`
- `UpdateTaskStatusAsync(Guid taskId, string status)`

### Phase 2: Frontend Implementation

#### 2.1 Create ProjectCreate Page
**File:** `frontend/src/pages/ProjectCreate.tsx`

**Features:**
- Form with validation (name, description, status, start date, end date)
- Date validation (end date after start date)
- Status dropdown (Planning, Active, OnHold, Completed, Cancelled)
- Submit creates project and navigates to project list

#### 2.2 Create ProjectDetail Page
**File:** `frontend/src/pages/ProjectDetail.tsx`

**Features:**
- Project information display (name, description, status, dates, team size)
- Team members list (from ProjectMembers)
- Task Board (Kanban-style) showing tasks by status
- Create task button/modal
- Edit/Delete project buttons
- Progress indicators

#### 2.3 Create TaskBoard Component
**File:** `frontend/src/components/projects/TaskBoard.tsx`

**Features:**
- Kanban board with columns: ToDo, InProgress, InReview, Done, Cancelled
- Drag-and-drop task cards (optional, can use buttons for status transition)
- Task cards show: title, assignee, priority, due date
- Click task card to view/edit details
- Empty state for each column

#### 2.4 Create TaskCard Component
**File:** `frontend/src/components/projects/TaskCard.tsx`

**Features:**
- Display task title, description (truncated), assignee name, priority badge, due date
- Click to open task detail/edit modal
- Status transition buttons (if not drag-and-drop)

#### 2.5 Create TaskCreateModal Component
**File:** `frontend/src/components/projects/TaskCreateModal.tsx`

**Features:**
- Form with: title, description, assignee (dropdown of project members), priority, due date
- Validation
- Submit creates task and refreshes task board

#### 2.6 Create Task Hooks
**File:** `frontend/src/hooks/useTasks.ts`

**Hooks:**
- `useTasks()` - Get all tasks
- `useTask(id)` - Get task by ID
- `useProjectTasks(projectId)` - Get tasks for a project
- `useEmployeeTasks(employeeId)` - Get tasks for an employee
- `useCreateTask()` - Create task mutation
- `useUpdateTask()` - Update task mutation
- `useUpdateTaskStatus()` - Update task status mutation
- `useDeleteTask()` - Delete task mutation

#### 2.7 Update ProjectList
**File:** `frontend/src/pages/ProjectList.tsx`

**Fixes:**
- Ensure member count displays correctly (verify backend returns ProjectMembers)
- Add loading state for member count
- Add progress calculation (completed tasks / total tasks)

#### 2.8 Add Routes
**File:** `frontend/src/App.tsx`

**New Routes:**
- `/projects/new` → `ProjectCreate`
- `/projects/:id` → `ProjectDetail`

### Phase 3: Task Tracking Features

#### 3.1 Employee Task View
**Option A:** Add to EmployeeDetail page
- Show "Assigned Tasks" section with task list
- Link to project detail page
- Show task status, priority, due date

**Option B:** Create separate EmployeeTasks page
- Dedicated page showing all tasks assigned to employee
- Filter by status, priority, due date
- Group by project

#### 3.2 Project Task Tracking
- Task Board in ProjectDetail shows all project tasks
- Filter tasks by assignee
- Search tasks
- Sort by priority, due date, status

---

## Technical Details

### Backend Data Loading Strategy

**ProjectRepository.GetAllAsync():**
```csharp
return await _context.Projects
    .Include(p => p.ProjectMembers)
    .Include(p => p.Tasks.Where(t => !t.IsDeleted))
    .Where(p => !p.IsDeleted)
    .ToListAsync();
```

**ProjectRepository.GetByIdAsync():**
```csharp
var project = await _context.Projects
    .Include(p => p.ProjectMembers)
        .ThenInclude(pm => pm.Employee)
    .Include(p => p.Tasks.Where(t => !t.IsDeleted))
        .ThenInclude(t => t.AssignedToEmployee)
    .AsSplitQuery()
    .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

// Break circular references
if (project != null)
{
    foreach (var task in project.Tasks)
    {
        if (task.Project != null)
        {
            task.Project.ProjectMembers = new List<ProjectMember>();
            task.Project.Tasks = new List<TaskItem>();
        }
    }
    foreach (var member in project.ProjectMembers)
    {
        if (member.Employee != null)
        {
            member.Employee.ProjectMembers = new List<ProjectMember>();
        }
    }
}
```

### Frontend Task Board Design

**Kanban Columns:**
- ToDo (gray)
- InProgress (blue)
- InReview (yellow)
- Done (green)
- Cancelled (red)

**Task Card Display:**
- Title (bold)
- Assignee avatar/name
- Priority indicator (color-coded)
- Due date (with warning if overdue)
- Drag handle or status buttons

---

## Files to Create

### Backend
1. `Validators/ProjectValidator.cs`
2. `Validators/TaskValidator.cs`

### Frontend
1. `pages/ProjectCreate.tsx`
2. `pages/ProjectDetail.tsx`
3. `components/projects/TaskBoard.tsx`
4. `components/projects/TaskCard.tsx`
5. `components/projects/TaskCreateModal.tsx`
6. `hooks/useTasks.ts`

---

## Files to Modify

### Backend
1. `Repositories/ProjectRepository.cs` - Add includes, fix circular references
2. `Repositories/TaskRepository.cs` - Add methods, includes
3. `Controllers/ProjectsController.cs` - Add task endpoints, FluentValidation
4. `Controllers/TasksController.cs` - Add endpoints, FluentValidation
5. `Services/ProjectService.cs` - Add task-related methods
6. `Services/TaskService.cs` - Add new methods
7. `Services/IProjectService.cs` - Add method signatures
8. `Services/ITaskService.cs` - Add method signatures
9. `Repositories/IProjectRepository.cs` - Add method signatures
10. `Repositories/ITaskRepository.cs` - Add method signatures

### Frontend
1. `pages/ProjectList.tsx` - Fix member count display
2. `App.tsx` - Add routes
3. `api/endpoints/projects.api.ts` - Add task-related endpoints
4. `api/endpoints/tasks.api.ts` - Add new endpoints
5. `types/index.ts` - Verify types are complete

---

## Testing Checklist

### Backend
- [ ] ProjectRepository returns ProjectMembers and Tasks counts
- [ ] ProjectRepository.GetByIdAsync includes all navigation properties
- [ ] TaskRepository.GetByProjectIdAsync works correctly
- [ ] TaskRepository.GetByEmployeeIdAsync works correctly
- [ ] Create/Update project works without circular reference errors
- [ ] Create/Update task works correctly
- [ ] FluentValidation catches invalid project data
- [ ] FluentValidation catches invalid task data
- [ ] All new endpoints return correct data

### Frontend
- [ ] ProjectList shows correct member counts
- [ ] ProjectList shows correct task counts
- [ ] Create Project form validates correctly
- [ ] Create Project submits and navigates correctly
- [ ] ProjectDetail page loads and displays all data
- [ ] Task Board displays tasks in correct columns
- [ ] Create Task modal works
- [ ] Task status transitions work
- [ ] Task assignment works
- [ ] EmployeeDetail shows assigned tasks (if implemented)
- [ ] All routes work correctly

---

## Priority Order

1. **High Priority:**
   - Fix ProjectRepository to include ProjectMembers and Tasks
   - Create ProjectCreate page
   - Create ProjectDetail page with basic info
   - Fix member count in ProjectList

2. **Medium Priority:**
   - Implement Task Board (Kanban)
   - Create Task hooks
   - Add task creation functionality
   - Add task status transitions

3. **Low Priority:**
   - Add FluentValidation for Project and Task
   - Add task filtering and search
   - Add drag-and-drop (if time permits)
   - Add employee task view

---

## Estimated Implementation Time

- Backend fixes: 2-3 hours
- Frontend ProjectCreate: 1 hour
- Frontend ProjectDetail: 2-3 hours
- Task Board implementation: 3-4 hours
- Task management features: 2-3 hours
- Testing and fixes: 2-3 hours

**Total:** ~12-16 hours

---

## Notes

- Use similar patterns from Employee implementation
- Break circular references in repository (like Employee fix)
- Consider using DTOs for complex responses (optional)
- Task Board can start with button-based status transitions (drag-and-drop is nice-to-have)
- Focus on core functionality first, then add enhancements
