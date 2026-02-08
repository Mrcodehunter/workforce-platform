import { apiClient } from '../config/axios.config';
import type { AuditLog } from '../../types';

interface AuditLogFilters {
  entityType?: string;
  eventType?: string;
  startDate?: string;
  endDate?: string;
  limit?: number;
}

/**
 * Audit Log API endpoints
 */
export const auditLogsApi = {
  /**
   * Get all audit logs with optional filters
   */
  getAll: async (filters?: AuditLogFilters): Promise<AuditLog[]> => {
    const params = new URLSearchParams();
    if (filters?.entityType) params.append('entityType', filters.entityType);
    if (filters?.eventType) params.append('eventType', filters.eventType);
    if (filters?.startDate) params.append('startDate', filters.startDate);
    if (filters?.endDate) params.append('endDate', filters.endDate);
    if (filters?.limit) params.append('limit', filters.limit.toString());
    
    const queryString = params.toString();
    const url = `/auditlogs${queryString ? `?${queryString}` : ''}`;
    const response = await apiClient.get<AuditLog[]>(url);
    return response.data;
  },

  /**
   * Get recent audit logs (for dashboard)
   */
  getRecent: async (limit: number = 50): Promise<AuditLog[]> => {
    const response = await apiClient.get<AuditLog[]>(`/auditlogs/recent?limit=${limit}`);
    return response.data;
  },

  /**
   * Get audit logs for a specific entity
   */
  getByEntity: async (entityType: string, entityId: string): Promise<AuditLog[]> => {
    const response = await apiClient.get<AuditLog[]>(
      `/auditlogs/entity/${entityType}/${entityId}`
    );
    return response.data;
  },

  /**
   * Get audit logs by event type
   */
  getByEventType: async (eventType: string): Promise<AuditLog[]> => {
    const response = await apiClient.get<AuditLog[]>(`/auditlogs/event/${eventType}`);
    return response.data;
  },
};
