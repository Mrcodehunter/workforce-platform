import { apiClient } from '../config/axios.config';
import type { TaskItem } from '../../types';

/**
 * Task API endpoints
 */
export const tasksApi = {
  /**
   * Get all tasks
   */
  getAll: async (): Promise<TaskItem[]> => {
    const response = await apiClient.get<TaskItem[]>('/tasks');
    return response.data;
  },

  /**
   * Get task by ID
   */
  getById: async (id: string): Promise<TaskItem> => {
    const response = await apiClient.get<TaskItem>(`/tasks/${id}`);
    return response.data;
  },

  /**
   * Create a new task
   */
  create: async (task: Partial<TaskItem>): Promise<TaskItem> => {
    const response = await apiClient.post<TaskItem>('/tasks', task);
    return response.data;
  },

  /**
   * Update an existing task
   */
  update: async (id: string, task: Partial<TaskItem>): Promise<TaskItem> => {
    const response = await apiClient.put<TaskItem>(`/tasks/${id}`, { ...task, id });
    return response.data;
  },

  /**
   * Delete a task
   */
  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`/tasks/${id}`);
  },
};
