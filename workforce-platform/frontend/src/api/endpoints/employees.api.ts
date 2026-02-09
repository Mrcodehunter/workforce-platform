import { apiClient } from '../config/axios.config';
import type { Employee } from '../../types';

/**
 * Paginated result type (matches backend PagedResult)
 * Backend uses PascalCase, but we'll normalize to camelCase for frontend
 */
export interface PagedResult<T> {
  data: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

/**
 * Employee API endpoints
 */
export const employeesApi = {
  /**
   * Get all employees (non-paginated - for backward compatibility)
   */
  getAll: async (): Promise<Employee[]> => {
    const response = await apiClient.get<Employee[]>('/employees/all');
    return response.data;
  },

  /**
   * Get paginated employees
   */
  getPaged: async (page: number = 1, pageSize: number = 10): Promise<PagedResult<Employee>> => {
    const response = await apiClient.get<any>('/employees', {
      params: { page, pageSize },
    });
    // Map backend PascalCase to frontend camelCase
    const backendData = response.data;
    return {
      data: backendData.data || backendData.Data || [],
      page: backendData.page || backendData.Page || 1,
      pageSize: backendData.pageSize || backendData.PageSize || 10,
      totalCount: backendData.totalCount || backendData.TotalCount || 0,
      totalPages: backendData.totalPages || backendData.TotalPages || 0,
      hasPreviousPage: backendData.hasPreviousPage ?? backendData.HasPreviousPage ?? false,
      hasNextPage: backendData.hasNextPage ?? backendData.HasNextPage ?? false,
    };
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
