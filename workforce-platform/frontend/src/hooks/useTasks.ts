import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { tasksApi } from '../api';
import type { TaskItem } from '../types';

export const useTasks = () => {
  return useQuery({
    queryKey: ['tasks'],
    queryFn: tasksApi.getAll,
  });
};

export const useTask = (id: string) => {
  return useQuery({
    queryKey: ['tasks', id],
    queryFn: () => tasksApi.getById(id),
    enabled: !!id,
  });
};

export const useProjectTasks = (projectId: string) => {
  return useQuery({
    queryKey: ['tasks', 'project', projectId],
    queryFn: () => tasksApi.getByProjectId(projectId),
    enabled: !!projectId,
  });
};

export const useEmployeeTasks = (employeeId: string) => {
  return useQuery({
    queryKey: ['tasks', 'employee', employeeId],
    queryFn: () => tasksApi.getByEmployeeId(employeeId),
    enabled: !!employeeId,
  });
};

export const useCreateTask = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (task: Partial<TaskItem>) => tasksApi.create(task),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['tasks'] });
      if (variables.projectId) {
        queryClient.invalidateQueries({ queryKey: ['tasks', 'project', variables.projectId] });
      }
      if (variables.assignedToEmployeeId) {
        queryClient.invalidateQueries({ queryKey: ['tasks', 'employee', variables.assignedToEmployeeId] });
      }
    },
  });
};

export const useUpdateTask = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, task }: { id: string; task: Partial<TaskItem> }) =>
      tasksApi.update(id, task),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['tasks'] });
      queryClient.invalidateQueries({ queryKey: ['tasks', variables.id] });
      if (variables.task.projectId) {
        queryClient.invalidateQueries({ queryKey: ['tasks', 'project', variables.task.projectId] });
      }
      if (variables.task.assignedToEmployeeId) {
        queryClient.invalidateQueries({ queryKey: ['tasks', 'employee', variables.task.assignedToEmployeeId] });
      }
    },
  });
};

export const useUpdateTaskStatus = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, status }: { id: string; status: string; projectId?: string }) =>
      tasksApi.updateStatus(id, status),
    onSuccess: (data, variables) => {
      // Invalidate all task queries
      queryClient.invalidateQueries({ queryKey: ['tasks'] });
      queryClient.invalidateQueries({ queryKey: ['tasks', data.id] });
      
      // Use projectId from variables if provided, otherwise try from response
      const projectIdToUse = variables.projectId || data.projectId;
      if (projectIdToUse) {
        queryClient.invalidateQueries({ queryKey: ['tasks', 'project', projectIdToUse] });
      }
      
      if (data.assignedToEmployeeId) {
        queryClient.invalidateQueries({ queryKey: ['tasks', 'employee', data.assignedToEmployeeId] });
      }
    },
  });
};

export const useDeleteTask = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => tasksApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tasks'] });
    },
  });
};
