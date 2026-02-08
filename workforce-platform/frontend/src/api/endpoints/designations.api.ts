import { apiClient } from '../config/axios.config';
import type { Designation } from '../../types';

/**
 * Designation API endpoints
 */
export const designationsApi = {
  /**
   * Get all designations
   */
  getAll: async (): Promise<Designation[]> => {
    const response = await apiClient.get<Designation[]>('/designations');
    return response.data;
  },

  /**
   * Get designation by ID
   */
  getById: async (id: string): Promise<Designation> => {
    const response = await apiClient.get<Designation>(`/designations/${id}`);
    return response.data;
  },

  /**
   * Create a new designation
   */
  create: async (designation: Partial<Designation>): Promise<Designation> => {
    const response = await apiClient.post<Designation>('/designations', designation);
    return response.data;
  },
};
