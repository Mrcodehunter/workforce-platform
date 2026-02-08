# Frontend Implementation Plan

## Date: February 8, 2026

## Objective
Build a comprehensive React + TypeScript single-page application for the Workforce Management Platform, implementing all required views and features per Frontend_Spec.txt.

---

## Technology Stack

### Core Framework
- **React 18.2.0** - UI library
- **TypeScript 5.3.3** - Type safety
- **Vite 5.0.11** - Build tool and dev server

### Routing & State Management
- **React Router DOM 6.21.3** - Client-side routing
- **Zustand 4.5.0** - Global state management
- **@tanstack/react-query 5.17.19** - Server state management and caching

### UI & Styling
- **Tailwind CSS 3.4.1** - Utility-first CSS framework
- **tailwindcss-animate 1.0.7** - Animation utilities
- **lucide-react 0.323.0** - Icon library
- **class-variance-authority 0.7.0** - Component variants
- **clsx & tailwind-merge** - Conditional class utilities

### Forms & Validation
- **react-hook-form 7.49.3** - Form management
- Built-in HTML5 validation

### Data Visualization
- **recharts 2.10.4** - Chart library for dashboard

### HTTP Client
- **axios 1.6.5** - HTTP requests

### Utilities
- **date-fns 3.3.1** - Date formatting and manipulation

---

## Architecture

### Folder Structure
```
src/
├── api/                 # API communication layer
│   ├── config/
│   │   ├── axios.config.ts    # Axios client configuration
│   │   └── interceptors.ts   # Request/response interceptors
│   ├── endpoints/       # API endpoint definitions
│   │   ├── employees.api.ts
│   │   ├── departments.api.ts
│   │   ├── designations.api.ts
│   │   ├── projects.api.ts
│   │   ├── tasks.api.ts
│   │   ├── leaveRequests.api.ts
│   │   ├── dashboard.api.ts
│   │   ├── auditLogs.api.ts
│   │   └── index.ts
│   ├── constants.ts     # API constants
│   └── index.ts         # Main export
├── components/
│   ├── common/          # Reusable UI components
│   │   ├── Button.tsx
│   │   ├── Input.tsx
│   │   ├── Card.tsx
│   │   ├── Loading.tsx
│   │   ├── Error.tsx
│   │   └── EmptyState.tsx
│   └── layout/          # Layout components
│       ├── Navbar.tsx
│       └── Layout.tsx
├── pages/               # Page components
│   ├── Dashboard.tsx
│   ├── EmployeeList.tsx
│   ├── EmployeeDetail.tsx
│   ├── ProjectList.tsx
│   └── (more pages to be added)
├── hooks/               # Custom React hooks
│   ├── useEmployees.ts
│   ├── useProjects.ts
│   └── useDashboard.ts
├── types/               # TypeScript type definitions
│   └── index.ts
├── utils/               # Utility functions
│   └── cn.ts
├── App.tsx              # Main app component with routing
└── main.tsx             # Entry point
```

---

## Implementation Status

### ✅ Completed

1. **Foundation**
   - ✅ TypeScript types matching backend models
   - ✅ API service layer with axios
   - ✅ React Query setup for data fetching
   - ✅ Reusable UI components (Button, Input, Card, Loading, Error, EmptyState)
   - ✅ Layout components (Navbar, Layout)
   - ✅ Routing setup

2. **Pages Implemented**
   - ✅ Dashboard - Summary statistics and charts
   - ✅ Employee List - Paginated, searchable, filterable table
   - ✅ Employee Detail - Composite view with profile, projects, leave history
   - ✅ Project List - Card-based overview with status indicators

3. **Hooks Created**
   - ✅ useEmployees - CRUD operations for employees
   - ✅ useProjects - CRUD operations for projects
   - ✅ useDashboard - Dashboard data fetching

### ⚠️ In Progress / Pending

1. **Employee Management**
   - ⚠️ Create Employee form (with skills multi-input)
   - ⚠️ Edit Employee functionality
   - ⚠️ Delete Employee (soft-delete)

2. **Project & Task Management**
   - ⚠️ Project Detail page
   - ⚠️ Task Board (Kanban-style workflow)
   - ⚠️ Create/Edit Project
   - ⚠️ Create/Assign Tasks
   - ⚠️ Task status transitions

3. **Leave Management**
   - ⚠️ Leave Request List (with filters)
   - ⚠️ Submit Leave Request form
   - ⚠️ Approve/Reject interface

4. **Audit Trail**
   - ⚠️ Entity-scoped audit trail (in detail pages)
   - ⚠️ System-wide audit log page

5. **Additional Features**
   - ⚠️ Form validation
   - ⚠️ Error boundaries
   - ⚠️ Loading skeletons
   - ⚠️ Toast notifications
   - ⚠️ Responsive design refinements

---

## Required Views (Per Frontend_Spec.txt)

### Employee Management Views

#### 1. Employee List ✅
- **Status:** Implemented
- **Features:**
  - ✅ Paginated data grid
  - ✅ Search functionality
  - ✅ Filter by department
  - ✅ Filter by active status
  - ✅ Column sorting
  - ✅ Links to detail view

#### 2. Employee Detail ✅
- **Status:** Implemented (partial)
- **Features:**
  - ✅ Profile information (from SQL)
  - ✅ Assigned projects (from SQL)
  - ✅ Leave request history (from MongoDB)
  - ⚠️ Audit trail (placeholder - needs API endpoint)
  - ⚠️ Edit functionality (button present, needs form)
  - ⚠️ Delete functionality (button present, needs confirmation)

#### 3. Create Employee ⚠️
- **Status:** Pending
- **Requirements:**
  - Form with validation for all required fields
  - Multi-value input for skills (add/remove tags)
  - Department and Designation dropdowns

---

### Project & Task Views

#### 1. Project List ✅
- **Status:** Implemented
- **Features:**
  - ✅ Overview of all projects
  - ✅ Status indicators
  - ✅ Team size display
  - ✅ Progress indication
  - ⚠️ Create new project (button present, needs form)

#### 2. Project Detail & Task Board ⚠️
- **Status:** Pending
- **Requirements:**
  - Project information display
  - Team members list
  - Task board (Kanban-style workflow states)
  - Create tasks
  - Assign tasks to team members
  - Transition task status
  - Visual project state

---

### Leave Management Views

#### 1. Leave Request List ⚠️
- **Status:** Pending
- **Requirements:**
  - Show all leave requests
  - Filter by status
  - Filter by leave type
  - View approval history inline (without navigation)

#### 2. Submit Leave Request ⚠️
- **Status:** Pending
- **Requirements:**
  - Date range selection
  - Leave type choice
  - Validation (end date after start date)
  - Domain event publication on submission

#### 3. Approve / Reject Flow ⚠️
- **Status:** Pending
- **Requirements:**
  - Review pending requests
  - Record approval decisions
  - Optional comments
  - Update embedded approval history in MongoDB

---

### Dashboard & Reports ✅

#### Dashboard ✅
- **Status:** Implemented
- **Features:**
  - ✅ Summary statistics cards
  - ✅ Department headcount chart (Bar chart)
  - ✅ Leave requests by type (Pie chart)
  - ✅ Project progress indicators
  - ✅ Data from both databases (SQL + MongoDB)
  - ✅ Auto-refresh every 30 seconds

---

### Audit Trail ⚠️

#### 1. Entity-Scoped Audit Trail
- **Status:** Placeholder in Employee Detail
- **Requirements:**
  - Display audit logs for specific entity
  - Show in detail pages

#### 2. System-Wide Audit Log ⚠️
- **Status:** Pending
- **Requirements:**
  - Standalone activity feed/log page
  - Recent system-wide activity
  - Filterable/searchable

---

## Design System

### Color Palette
- Primary: Blue (#3b82f6)
- Success: Green (#10b981)
- Warning: Yellow (#f59e0b)
- Error: Red (#ef4444)
- Muted: Gray shades

### Components
- **Button:** Multiple variants (default, destructive, outline, secondary, ghost, link)
- **Input:** Styled form inputs with focus states
- **Card:** Container component with header, content, footer
- **Loading:** Spinner component
- **Error:** Error display with retry option
- **EmptyState:** Empty state with icon, title, description, action

### Responsive Breakpoints
- Mobile: < 768px
- Tablet: 768px - 1024px
- Desktop: > 1024px

---

## API Integration

### Base URL
- Development: `http://localhost:63890/api`
- Production: Configurable via `VITE_API_URL` environment variable

### Endpoints Used
- `GET /api/employees` - Get all employees
- `GET /api/employees/:id` - Get employee by ID
- `POST /api/employees` - Create employee
- `PUT /api/employees/:id` - Update employee
- `DELETE /api/employees/:id` - Delete employee
- `GET /api/departments` - Get all departments
- `GET /api/projects` - Get all projects
- `GET /api/projects/:id` - Get project by ID
- `GET /api/leaverequests` - Get all leave requests
- `GET /api/dashboard/summary` - Get dashboard summary

---

## Error Handling

### Strategy
1. **React Query Error Handling**
   - Automatic retry (1 attempt)
   - Error state management
   - Refetch on error

2. **Component-Level Error Handling**
   - Error component with retry button
   - User-friendly error messages
   - Fallback UI

3. **Null/Undefined Safety**
   - All array operations use null checks: `(array || [])`
   - Optional chaining for nested properties: `data?.property`
   - Array.isArray() checks before mapping
   - Empty state components for missing data

4. **Form Validation**
   - HTML5 validation
   - Custom validation rules
   - Error message display

5. **API Error Interceptor**
   - Transforms axios errors to ApiError format
   - Logs errors in development
   - Consistent error handling across all API calls

---

## Loading States

### Implementation
- **Loading Component:** Spinner for full-page loads
- **Loading Spinner:** Inline spinner for partial loads
- **Skeleton Loaders:** (To be implemented) For better UX

---

## Next Steps

1. **Complete Employee Management**
   - Create Employee form
   - Edit Employee form
   - Delete confirmation dialog

2. **Implement Project Detail & Task Board**
   - Project detail page
   - Kanban board for tasks
   - Task creation/editing
   - Drag-and-drop for status transitions (optional enhancement)

3. **Implement Leave Management**
   - Leave request list with filters
   - Submit leave request form
   - Approve/reject interface

4. **Implement Audit Trail**
   - Entity-scoped audit display
   - System-wide audit log page

5. **Enhancements**
   - Toast notifications
   - Loading skeletons
   - Error boundaries
   - Form validation improvements
   - Responsive design refinements

---

## Testing Checklist

- [ ] All pages load correctly
- [ ] Navigation works
- [ ] API calls succeed
- [ ] Error states display properly
- [ ] Loading states show
- [ ] Forms validate correctly
- [ ] Responsive design works on mobile/tablet
- [ ] Charts render correctly
- [ ] Filters and search work
- [ ] Sorting works

---

## Files Created

### API Layer
- `src/api/config/axios.config.ts` - Axios client configuration
- `src/api/config/interceptors.ts` - Request/response interceptors
- `src/api/endpoints/employees.api.ts`
- `src/api/endpoints/departments.api.ts`
- `src/api/endpoints/designations.api.ts`
- `src/api/endpoints/projects.api.ts`
- `src/api/endpoints/tasks.api.ts`
- `src/api/endpoints/leaveRequests.api.ts`
- `src/api/endpoints/dashboard.api.ts`
- `src/api/endpoints/auditLogs.api.ts`
- `src/api/endpoints/index.ts` - Central export
- `src/api/constants.ts` - API constants
- `src/api/index.ts` - Main export
- `src/api/README.md` - API layer documentation

### Types
- `src/types/index.ts` - All TypeScript interfaces

### Hooks
- `src/hooks/useEmployees.ts`
- `src/hooks/useProjects.ts`
- `src/hooks/useDashboard.ts`

### Components
- `src/components/common/Button.tsx`
- `src/components/common/Input.tsx`
- `src/components/common/Card.tsx`
- `src/components/common/Loading.tsx`
- `src/components/common/Error.tsx`
- `src/components/common/EmptyState.tsx`
- `src/components/layout/Navbar.tsx`
- `src/components/layout/Layout.tsx`

### Pages
- `src/pages/Dashboard.tsx` - With null safety and empty states
- `src/pages/EmployeeList.tsx` - With search, filter, sort
- `src/pages/EmployeeDetail.tsx` - Composite view with null safety
- `src/pages/ProjectList.tsx` - Card-based overview

### Configuration
- `src/App.tsx` - Updated with routing
- `src/main.tsx` - Updated with React Query provider

### Documentation
- `frontend/README.md` - Frontend documentation
- `frontend/API_CONNECTION.md` - API connection troubleshooting guide

---

## Notes

- Using Tailwind CSS for styling (no separate UI library like Material-UI or Ant Design)
- React Query handles all server state management
- Form validation uses HTML5 + custom rules
- Charts use Recharts library
- Icons from Lucide React
- All components are TypeScript-typed
- Responsive design using Tailwind breakpoints
