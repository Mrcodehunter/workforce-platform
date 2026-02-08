import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { leaveRequestsApi } from '../api';
import type { LeaveRequest } from '../types';

interface LeaveRequestFilters {
  status?: string;
  leaveType?: string;
  employeeId?: string;
}

export const useLeaveRequests = (filters?: LeaveRequestFilters) => {
  return useQuery({
    queryKey: ['leaveRequests', filters],
    queryFn: () => leaveRequestsApi.getAll(filters),
  });
};

export const useLeaveRequest = (id: string) => {
  return useQuery({
    queryKey: ['leaveRequests', id],
    queryFn: () => leaveRequestsApi.getById(id),
    enabled: !!id,
  });
};

export const useCreateLeaveRequest = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (leaveRequest: Partial<LeaveRequest>) => leaveRequestsApi.create(leaveRequest),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['leaveRequests'] });
    },
  });
};

export const useUpdateLeaveRequestStatus = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ 
      id, 
      status, 
      comments, 
      changedBy 
    }: { 
      id: string; 
      status: LeaveRequest['status']; 
      comments?: string;
      changedBy?: string;
    }) => leaveRequestsApi.updateStatus(id, status, comments, changedBy),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['leaveRequests'] });
    },
  });
};
