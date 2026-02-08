// Employee Types
export interface Employee {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  isActive: boolean;
  departmentId: string;
  designationId: string;
  salary: number;
  joiningDate: string;
  phone?: string;
  address?: string;
  city?: string;
  country?: string;
  skills: string[];
  avatarUrl?: string;
  createdAt: string;
  updatedAt: string;
  isDeleted: boolean;
  department?: Department;
  designation?: Designation;
  projectMembers?: ProjectMember[];
  assignedTasks?: TaskItem[];
}

// Department Types
export interface Department {
  id: string;
  name: string;
  description?: string;
  createdAt: string;
  updatedAt: string;
  isDeleted: boolean;
}

// Designation Types
export interface Designation {
  id: string;
  title: string;
  level?: number;
  description?: string;
  createdAt: string;
  updatedAt: string;
}

// Project Types
export interface Project {
  id: string;
  name: string;
  description?: string;
  status: ProjectStatus;
  startDate: string;
  endDate?: string;
  createdAt: string;
  updatedAt: string;
  isDeleted: boolean;
  projectMembers?: ProjectMember[];
  tasks?: TaskItem[];
}

// Project List Item (for list view - uses counts instead of full arrays)
export interface ProjectListItem {
  id: string;
  name: string;
  description?: string;
  status: ProjectStatus;
  startDate: string;
  endDate?: string;
  memberCount: number;
  taskCount: number;
}

export type ProjectStatus = 'Planning' | 'Active' | 'OnHold' | 'Completed' | 'Cancelled';

export interface ProjectMember {
  projectId: string;
  employeeId: string;
  role?: string;
  joinedAt: string;
  project?: Project;
  employee?: Employee;
}

// Task Types
export interface TaskItem {
  id: string;
  projectId: string;
  title: string;
  description?: string;
  status: TaskStatus;
  assignedToEmployeeId?: string;
  priority: TaskPriority;
  dueDate?: string;
  createdAt: string;
  updatedAt: string;
  isDeleted: boolean;
  project?: Project;
  assignedToEmployee?: Employee;
}

export type TaskStatus = 'ToDo' | 'InProgress' | 'InReview' | 'Done' | 'Cancelled';
export type TaskPriority = 0 | 1 | 2 | 3; // Low, Medium, High, Critical

// Leave Request Types (MongoDB)
export interface LeaveRequest {
  id?: string;
  employeeId: string;
  employeeName: string;
  leaveType: LeaveType;
  startDate: string;
  endDate: string;
  status: LeaveStatus;
  reason?: string;
  approvalHistory: ApprovalHistoryEntry[];
  createdAt: string;
  updatedAt: string;
}

export type LeaveType = 'Sick' | 'Casual' | 'Annual' | 'Unpaid';
export type LeaveStatus = 'Pending' | 'Approved' | 'Rejected' | 'Cancelled';

export interface ApprovalHistoryEntry {
  status: string;
  changedBy: string;
  changedAt: string;
  comments?: string;
}

// Audit Log Types (MongoDB)
export interface AuditLog {
  id?: string;
  eventId: string;
  eventType: string;
  entityType: string;
  entityId: string;
  actor?: string;
  timestamp: string;
  before?: any;
  after?: any;
  metadata?: Record<string, any>;
}

// Report Types (MongoDB)
export interface Report {
  id?: string;
  reportType: string;
  generatedAt: string;
  data: any;
}

// Dashboard Summary Types
export interface DashboardSummary {
  totalEmployees?: number;
  activeEmployees?: number;
  totalProjects?: number;
  activeProjects?: number;
  totalTasks?: number;
  completedTasks?: number;
  pendingLeaveRequests?: number;
  departmentHeadcount?: DepartmentHeadcount[];
  projectProgress?: ProjectProgress[];
  leaveStatistics?: LeaveStatistics;
  recentActivity?: AuditLog[];
}

export interface DepartmentHeadcount {
  departmentId: string;
  departmentName: string;
  count: number;
}

export interface ProjectProgress {
  projectId: string;
  projectName: string;
  status: ProjectStatus;
  totalTasks: number;
  completedTasks: number;
  progressPercentage: number;
}

export interface LeaveStatistics {
  totalRequests: number;
  approved: number;
  pending: number;
  rejected: number;
  byType: Record<LeaveType, number>;
}

// API Response Types
export interface PaginatedResponse<T> {
  data: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface ApiError {
  message: string;
  errors?: Record<string, string[]>;
}
