# Project Member Assignment Feature - Implementation Plan

## Overview
Add functionality to assign project members from the project details page. Users should be able to select employees from a searchable dropdown list and assign them to a project with an optional role.

## Requirements
1. **Backend:**
   - API endpoint to add a project member (POST `/api/projects/{projectId}/members`)
   - API endpoint to remove a project member (DELETE `/api/projects/{projectId}/members/{employeeId}`)
   - Validation to prevent duplicate assignments
   - Return updated project with members after assignment/removal

2. **Frontend:**
   - Add "Add Member" button in the Team Members section
   - Modal/dialog with searchable employee dropdown
   - Optional role field (Developer, Lead Developer, QA Engineer, Project Manager, Tech Lead, Architect)
   - Display assigned members with remove option
   - Refresh project data after member assignment/removal

## Backend Implementation

### 1. Create DTOs
**File:** `WorkforceAPI/Models/DTOs/ProjectMemberDto.cs`
- `AddProjectMemberRequestDto` - Request DTO for adding a member
  - `EmployeeId` (Guid, required)
  - `Role` (string, optional)
- `RemoveProjectMemberRequestDto` - Request DTO for removing a member
  - `EmployeeId` (Guid, required)

### 2. Add Repository Methods
**File:** `WorkforceAPI/Repositories/IProjectRepository.cs`
- Add method: `Task<bool> IsMemberAsync(Guid projectId, Guid employeeId)` - Check if employee is already a member
- Add method: `Task<ProjectMember> AddMemberAsync(ProjectMember member)` - Add project member
- Add method: `Task RemoveMemberAsync(Guid projectId, Guid employeeId)` - Remove project member

**File:** `WorkforceAPI/Repositories/ProjectRepository.cs`
- Implement the new methods
- Use `_context.ProjectMembers` for operations
- Handle duplicate key exceptions (composite key: ProjectId + EmployeeId)

### 3. Add Service Methods
**File:** `WorkforceAPI/Services/IProjectService.cs`
- Add method: `Task<ProjectDetailDto> AddMemberAsync(Guid projectId, Guid employeeId, string? role)`
- Add method: `Task<ProjectDetailDto> RemoveMemberAsync(Guid projectId, Guid employeeId)`

**File:** `WorkforceAPI/Services/ProjectService.cs`
- Implement member addition:
  - Check if project exists
  - Check if employee exists (via EmployeeRepository)
  - Check if employee is already a member (prevent duplicates)
  - Create ProjectMember entity
  - Save to database
  - Reload project with navigation properties
  - Return updated ProjectDetailDto
- Implement member removal:
  - Check if project exists
  - Check if member exists
  - Remove ProjectMember
  - Reload project with navigation properties
  - Return updated ProjectDetailDto

### 4. Add Controller Endpoints
**File:** `WorkforceAPI/Controllers/ProjectsController.cs`
- `POST /api/projects/{id}/members`
  - Accept `AddProjectMemberRequestDto` in body
  - Validate project exists
  - Validate employee exists
  - Call service to add member
  - Return 201 Created with updated project
  - Handle duplicate member error (400 Bad Request)
- `DELETE /api/projects/{id}/members/{employeeId}`
  - Validate project exists
  - Call service to remove member
  - Return 204 No Content
  - Handle member not found error (404 Not Found)

### 5. Add Validation
**File:** `WorkforceAPI/Validators/AddProjectMemberRequestValidator.cs` (if needed)
- Validate EmployeeId is not empty
- Validate Role is not too long (optional)

## Frontend Implementation

### 1. Create API Endpoints
**File:** `frontend/src/api/endpoints/projects.api.ts`
- Add `addMember(projectId: string, employeeId: string, role?: string)` method
- Add `removeMember(projectId: string, employeeId: string)` method

### 2. Create React Hooks
**File:** `frontend/src/hooks/useProjects.ts`
- Add `useAddProjectMember()` hook using `useMutation`
- Add `useRemoveProjectMember()` hook using `useMutation`
- Both should invalidate project query cache on success

### 3. Create Add Member Modal Component
**File:** `frontend/src/components/projects/AddProjectMemberModal.tsx`
- Props:
  - `projectId: string`
  - `onClose: () => void`
  - `existingMemberIds: string[]` - To filter out already assigned employees
- Features:
  - Fetch all employees using `useEmployees()` hook
  - Filter out employees already assigned to the project
  - Use `SearchableSelect` component for employee selection
  - Optional role dropdown/input field
  - Submit button to add member
  - Loading and error states
  - Close button

### 4. Update Project Detail Page
**File:** `frontend/src/pages/ProjectDetail.tsx`
- Add state for showing Add Member Modal
- Add "Add Member" button in Team Members Card section
- Display remove button (X icon) for each member
- Handle member removal with confirmation
- Refresh project data after member addition/removal
- Show loading state during operations

### 5. Employee List Hook
**File:** `frontend/src/hooks/useEmployees.ts` (check if exists)
- Ensure `useEmployees()` hook exists to fetch all employees
- If not, create it similar to `useProjects()`

## Data Flow

### Adding a Member:
1. User clicks "Add Member" button
2. Modal opens with searchable employee dropdown
3. User selects employee and optionally sets role
4. Frontend calls `POST /api/projects/{id}/members`
5. Backend validates and adds member
6. Backend returns updated project
7. Frontend refreshes project data
8. Modal closes

### Removing a Member:
1. User clicks remove (X) button on a member
2. Confirmation dialog appears
3. Frontend calls `DELETE /api/projects/{id}/members/{employeeId}`
4. Backend removes member
5. Frontend refreshes project data
6. Member disappears from list

## Error Handling

### Backend:
- **400 Bad Request:** Employee already assigned, invalid employee ID, invalid project ID
- **404 Not Found:** Project not found, Employee not found, Member not found (for removal)
- **500 Internal Server Error:** Database errors, unexpected exceptions

### Frontend:
- Display error messages in modal/toast
- Handle network errors gracefully
- Show loading states during API calls
- Disable buttons during operations

## Testing Checklist

### Backend:
- [ ] Add member to project successfully
- [ ] Prevent duplicate member assignment
- [ ] Return 404 if project doesn't exist
- [ ] Return 400 if employee doesn't exist
- [ ] Return 400 if employee already assigned
- [ ] Remove member successfully
- [ ] Return 404 if member doesn't exist
- [ ] Verify project reload includes updated members

### Frontend:
- [ ] Modal opens and closes correctly
- [ ] Employee dropdown shows all employees except already assigned
- [ ] Search filters employees correctly
- [ ] Add member button works
- [ ] Remove member button works with confirmation
- [ ] Project data refreshes after add/remove
- [ ] Error messages display correctly
- [ ] Loading states work correctly

## Files to Create/Modify

### Backend:
1. `WorkforceAPI/Models/DTOs/ProjectMemberDto.cs` (create)
2. `WorkforceAPI/Repositories/IProjectRepository.cs` (modify)
3. `WorkforceAPI/Repositories/ProjectRepository.cs` (modify)
4. `WorkforceAPI/Services/IProjectService.cs` (modify)
5. `WorkforceAPI/Services/ProjectService.cs` (modify)
6. `WorkforceAPI/Controllers/ProjectsController.cs` (modify)

### Frontend:
1. `frontend/src/api/endpoints/projects.api.ts` (modify)
2. `frontend/src/hooks/useProjects.ts` (modify)
3. `frontend/src/components/projects/AddProjectMemberModal.tsx` (create)
4. `frontend/src/pages/ProjectDetail.tsx` (modify)
5. `frontend/src/hooks/useEmployees.ts` (create if doesn't exist)

## Implementation Order

1. **Backend First:**
   - Create DTOs
   - Add repository methods
   - Add service methods
   - Add controller endpoints
   - Test with Postman/API client

2. **Frontend Second:**
   - Create API endpoints
   - Create hooks
   - Create Add Member Modal component
   - Update Project Detail page
   - Test end-to-end

## Notes
- Use existing `SearchableSelect` component for employee selection
- Reuse existing employee API endpoints if available
- Follow existing code patterns and conventions
- Ensure proper error handling and user feedback
- Consider adding audit logging for member additions/removals (future enhancement)
