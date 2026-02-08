# Leave Requests Implementation Plan

## Overview
This document outlines the implementation plan for the Leave Management feature, including both backend API enhancements and frontend UI components.

## Requirements Analysis

### From Frontend_Spec.txt
1. **Leave Request List**: 
   - Show all leave requests with approval status
   - Support filtering by status and leave type
   - View embedded approval history without navigation

2. **Submit Leave Request**:
   - Form with date range selection
   - Leave type choice
   - Validation (end date > start date)
   - Publish domain event on submission

3. **Approve/Reject Flow**:
   - Interface for reviewing pending requests
   - Record approval decisions with optional comments
   - Add entry to embedded approval history in MongoDB

### From Domain_Model.txt
- **Leave Types**: Sick, Casual, Annual, Unpaid
- **Status**: Pending, Approved, Rejected, Cancelled
- **Fields**: employeeId, employeeName, leaveType, startDate, endDate, status, reason, approvalHistory
- **Approval History**: Embedded array with status, changedBy, changedAt, comments

### From Backend_Spec.txt
- Must publish domain events for state changes
- MongoDB storage for leave requests
- Seed data should include leave requests

## Current State Analysis

### Backend
✅ **Implemented**:
- `LeaveRequest` model with all required fields
- `ApprovalHistoryEntry` model
- Basic `LeaveRequestRepository` (MongoDB)
- Basic `LeaveRequestService` (needs refactoring)
- `LeaveRequestsController` with GET endpoints only

❌ **Missing**:
- POST endpoint for creating leave requests
- PUT/PATCH endpoint for status updates (approve/reject)
- Filtering support (by status, leave type, employee)
- Event publishing for leave request operations
- Service refactoring (currently uses `object` types)
- FluentValidation for leave requests
- DTOs for leave requests (optional, but consistent with architecture)

### Frontend
✅ **Implemented**:
- `LeaveRequest` type definition
- Basic API client (`leaveRequestsApi`) with create and updateStatus methods

❌ **Missing**:
- Leave Request List page
- Submit Leave Request form page
- Approve/Reject interface
- React Query hooks for leave requests
- Filtering UI components
- Approval history display component

## Implementation Plan

### Phase 1: Backend API Enhancements

#### 1.1 Refactor Service Layer
- [ ] Update `ILeaveRequestService` to use `LeaveRequest` instead of `object`
- [ ] Update `LeaveRequestService` implementation
- [ ] Add event publisher dependency
- [ ] Add methods: `CreateAsync`, `UpdateStatusAsync`, `GetByEmployeeIdAsync`, `GetByStatusAsync`, `GetByLeaveTypeAsync`

#### 1.2 Add Validation
- [ ] Create `LeaveRequestValidator` using FluentValidation
- [ ] Validate: required fields, date range (endDate > startDate), valid leave types, valid status values
- [ ] Register validator in DI

#### 1.3 Enhance Controller
- [ ] Add `POST /api/leaverequests` - Create leave request
- [ ] Add `PUT /api/leaverequests/{id}/status` - Update status (approve/reject)
- [ ] Add query parameters for filtering: `?status=`, `?leaveType=`, `?employeeId=`
- [ ] Add proper error handling and validation
- [ ] Return appropriate HTTP status codes

#### 1.4 Event Publishing
- [ ] Publish `leave.request.created` event on creation
- [ ] Publish `leave.request.approved` event on approval
- [ ] Publish `leave.request.rejected` event on rejection
- [ ] Publish `leave.request.cancelled` event on cancellation

#### 1.5 Repository Enhancements
- [ ] Add filtering methods: `GetByStatusAsync`, `GetByLeaveTypeAsync`, `GetByEmployeeIdAsync`
- [ ] Add `CreateAsync` method
- [ ] Add `UpdateStatusAsync` method (with approval history update)

### Phase 2: Frontend Implementation

#### 2.1 React Query Hooks
- [ ] Create `useLeaveRequests` hook (with filtering support)
- [ ] Create `useLeaveRequest` hook (get by ID)
- [ ] Create `useCreateLeaveRequest` hook
- [ ] Create `useUpdateLeaveRequestStatus` hook
- [ ] Add proper query invalidation

#### 2.2 Leave Request List Page
- [ ] Create `LeaveRequestList.tsx` page
- [ ] Display leave requests in a table/card layout
- [ ] Add filtering UI:
  - Status filter dropdown (All, Pending, Approved, Rejected, Cancelled)
  - Leave type filter dropdown (All, Sick, Casual, Annual, Unpaid)
  - Employee filter (optional, for admin view)
- [ ] Show approval history in expandable/collapsible section per request
- [ ] Add pagination if needed
- [ ] Add loading and error states

#### 2.3 Submit Leave Request Form
- [ ] Create `LeaveRequestCreate.tsx` page
- [ ] Form fields:
  - Leave type dropdown (Sick, Casual, Annual, Unpaid)
  - Start date picker
  - End date picker
  - Reason textarea (optional)
- [ ] Client-side validation:
  - End date must be after start date
  - Required fields validation
- [ ] Auto-populate employeeId and employeeName from current user context
- [ ] Submit to API and show success/error messages
- [ ] Navigate to list after successful submission

#### 2.4 Approve/Reject Interface
- [ ] Create `LeaveRequestApproveModal.tsx` or inline component
- [ ] Show request details
- [ ] Action buttons: Approve, Reject, Cancel
- [ ] Comments textarea (optional)
- [ ] Show current approval history
- [ ] Update status via API
- [ ] Refresh list after update

#### 2.5 Approval History Component
- [ ] Create `ApprovalHistory.tsx` component
- [ ] Display approval history entries in timeline format
- [ ] Show: status, changed by, timestamp, comments
- [ ] Use in Leave Request List and Detail views

#### 2.6 Routing
- [ ] Add routes in `App.tsx`:
  - `/leave-requests` - List page
  - `/leave-requests/new` - Create page
  - `/leave-requests/:id` - Detail page (optional)

#### 2.7 Navigation
- [ ] Add "Leave Requests" link to navigation menu
- [ ] Add "Submit Leave Request" button/link

### Phase 3: Integration & Testing

#### 3.1 Backend Testing
- [ ] Test create leave request endpoint
- [ ] Test status update endpoint
- [ ] Test filtering endpoints
- [ ] Test event publishing
- [ ] Test validation rules

#### 3.2 Frontend Testing
- [ ] Test leave request list with filters
- [ ] Test create leave request form
- [ ] Test approve/reject flow
- [ ] Test approval history display
- [ ] Test error handling

#### 3.3 Integration Testing
- [ ] Test full flow: Create → List → Approve → View History
- [ ] Test event publishing and consumption
- [ ] Test data consistency between SQL (employees) and MongoDB (leave requests)

## Technical Decisions

### Backend Architecture
- **Service Layer**: Use `LeaveRequest` type instead of `object` for type safety
- **Validation**: Use FluentValidation for consistent validation approach
- **Events**: Publish events for all state changes (create, approve, reject, cancel)
- **Filtering**: Support query parameters for flexible filtering

### Frontend Architecture
- **State Management**: Use React Query for server state
- **Form Handling**: Use controlled components with validation
- **UI Components**: Reuse existing components (Button, Input, Card, etc.)
- **Date Handling**: Use `date-fns` for date formatting (already in use)

### Data Flow
1. User submits leave request → Frontend validates → API creates → Event published
2. Manager reviews → Approves/Rejects → API updates status + approval history → Event published
3. List view filters and displays requests with embedded approval history

## File Structure

### Backend Files to Create/Modify
```
backend/WorkforceAPI/
├── Controllers/
│   └── LeaveRequestsController.cs (enhance)
├── Services/
│   ├── ILeaveRequestService.cs (enhance)
│   └── LeaveRequestService.cs (enhance)
├── Repositories/
│   ├── ILeaveRequestRepository.cs (enhance)
│   └── LeaveRequestRepository.cs (enhance)
├── Validators/
│   └── LeaveRequestValidator.cs (new)
└── Models/
    └── MongoDB/
        └── LeaveRequest.cs (already exists)
```

### Frontend Files to Create/Modify
```
frontend/src/
├── pages/
│   ├── LeaveRequestList.tsx (new)
│   ├── LeaveRequestCreate.tsx (new)
│   └── LeaveRequestDetail.tsx (optional, new)
├── components/
│   └── leave-requests/
│       ├── ApprovalHistory.tsx (new)
│       ├── LeaveRequestCard.tsx (new, optional)
│       └── LeaveRequestFilters.tsx (new, optional)
├── hooks/
│   └── useLeaveRequests.ts (new)
└── api/
    └── endpoints/
        └── leaveRequests.api.ts (enhance)
```

## Success Criteria

✅ **Backend**:
- All CRUD operations work correctly
- Filtering by status and leave type works
- Events are published for all state changes
- Validation prevents invalid data
- Approval history is properly maintained

✅ **Frontend**:
- Users can view all leave requests with filters
- Users can submit new leave requests
- Managers can approve/reject requests with comments
- Approval history is visible inline
- All states (loading, error, empty) are handled

## Next Steps

1. Start with Phase 1: Backend API enhancements
2. Then Phase 2: Frontend implementation
3. Finally Phase 3: Integration and testing
