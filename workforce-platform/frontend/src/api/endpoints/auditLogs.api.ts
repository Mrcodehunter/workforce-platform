import { apiClient } from '../config/axios.config';
import type { AuditLog } from '../../types';

/**
 * Audit Log API endpoints
 */
export const auditLogsApi = {
  /**
   * Get all audit logs
   */
  getAll: async (): Promise<AuditLog[]> => {
    const response = await apiClient.get<AuditLog[]>('/auditlogs');
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
