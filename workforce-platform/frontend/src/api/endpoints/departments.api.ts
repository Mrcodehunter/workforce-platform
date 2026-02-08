import { apiClient } from '../config/axios.config';
import type { Department } from '../../types';

/**
 * Department API endpoints
 */
export const departmentsApi = {
  /**
   * Get all departments
   */
  getAll: async (): Promise<Department[]> => {
    const response = await apiClient.get<Department[]>('/departments');
    return response.data;
  },

  /**
   * Get department by ID
   */
  getById: async (id: string): Promise<Department> => {
    const response = await apiClient.get<Department>(`/departments/${id}`);
    return response.data;
  },

  /**
   * Create a new department
   */
  create: async (department: Partial<Department>): Promise<Department> => {
    const response = await apiClient.post<Department>('/departments', department);
    return response.data;
  },

  /**
   * Update an existing department
   */
  update: async (id: string, department: Partial<Department>): Promise<Department> => {
    const response = await apiClient.put<Department>(`/departments/${id}`, { ...department, id });
    return response.data;
  },

  /**
   * Delete a department
   */
  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`/departments/${id}`);
  },
};
