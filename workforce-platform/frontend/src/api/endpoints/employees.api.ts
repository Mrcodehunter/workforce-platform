import { apiClient } from '../config/axios.config';
import type { Employee } from '../../types';

/**
 * Employee API endpoints
 */
export const employeesApi = {
  /**
   * Get all employees
   */
  getAll: async (): Promise<Employee[]> => {
    const response = await apiClient.get<Employee[]>('/employees');
    return response.data;
  },

  /**
   * Get employee by ID
   */
  getById: async (id: string): Promise<Employee> => {
    const response = await apiClient.get<Employee>(`/employees/${id}`);
    return response.data;
  },

  /**
   * Create a new employee
   */
  create: async (employee: Partial<Employee>): Promise<Employee> => {
    const response = await apiClient.post<Employee>('/employees', employee);
    return response.data;
  },

  /**
   * Update an existing employee
   */
  update: async (id: string, employee: Partial<Employee>): Promise<Employee> => {
    const response = await apiClient.put<Employee>(`/employees/${id}`, { ...employee, id });
    return response.data;
  },

  /**
   * Delete an employee (soft delete)
   */
  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`/employees/${id}`);
  },
};
