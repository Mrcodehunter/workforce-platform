import { apiClient } from '../config/axios.config';
import type { DashboardSummary } from '../../types';

/**
 * Dashboard API endpoints
 */
export const dashboardApi = {
  /**
   * Get dashboard summary data
   */
  getSummary: async (): Promise<DashboardSummary> => {
    const response = await apiClient.get<DashboardSummary>('/dashboard/summary');
    return response.data;
  },
};
