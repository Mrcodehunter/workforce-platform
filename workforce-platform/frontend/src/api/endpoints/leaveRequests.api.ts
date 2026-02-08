import { apiClient } from '../config/axios.config';
import type { LeaveRequest } from '../../types';

/**
 * Leave Request API endpoints
 */
export const leaveRequestsApi = {
  /**
   * Get all leave requests
   */
  getAll: async (): Promise<LeaveRequest[]> => {
    const response = await apiClient.get<LeaveRequest[]>('/leaverequests');
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
   * Update leave request status (approve/reject)
   */
  updateStatus: async (
    id: string,
    status: LeaveRequest['status'],
    comments?: string
  ): Promise<LeaveRequest> => {
    const response = await apiClient.put<LeaveRequest>(`/leaverequests/${id}/status`, {
      status,
      comments,
    });
    return response.data;
  },
};
