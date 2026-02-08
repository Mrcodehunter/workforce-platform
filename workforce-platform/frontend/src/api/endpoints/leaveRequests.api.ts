import { apiClient } from '../config/axios.config';
import type { LeaveRequest } from '../../types';

interface LeaveRequestFilters {
  status?: string;
  leaveType?: string;
  employeeId?: string;
}

/**
 * Leave Request API endpoints
 */
export const leaveRequestsApi = {
  /**
   * Get all leave requests with optional filters
   */
  getAll: async (filters?: LeaveRequestFilters): Promise<LeaveRequest[]> => {
    const params = new URLSearchParams();
    if (filters?.status) params.append('status', filters.status);
    if (filters?.leaveType) params.append('leaveType', filters.leaveType);
    if (filters?.employeeId) params.append('employeeId', filters.employeeId);
    
    const queryString = params.toString();
    const url = `/leaverequests${queryString ? `?${queryString}` : ''}`;
    const response = await apiClient.get<LeaveRequest[]>(url);
    return response.data;
  },

  /**
   * Get leave request by ID
   */
  getById: async (id: string): Promise<LeaveRequest> => {
    const response = await apiClient.get<LeaveRequest>(`/leaverequests/${id}`);
    return response.data;
  },

  /**
   * Create a new leave request
   */
  create: async (leaveRequest: Partial<LeaveRequest>): Promise<LeaveRequest> => {
    const response = await apiClient.post<LeaveRequest>('/leaverequests', leaveRequest);
    return response.data;
  },

  /**
   * Update leave request status (approve/reject/cancel)
   */
  updateStatus: async (
    id: string,
    status: LeaveRequest['status'],
    comments?: string,
    changedBy?: string
  ): Promise<LeaveRequest> => {
    const response = await apiClient.put<LeaveRequest>(`/leaverequests/${id}/status`, {
      status,
      comments,
      changedBy,
    });
    return response.data;
  },
};
