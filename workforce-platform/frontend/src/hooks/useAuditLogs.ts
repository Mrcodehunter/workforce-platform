import { useQuery } from '@tanstack/react-query';
import { auditLogsApi } from '../api';

interface AuditLogFilters {
  entityType?: string;
  eventType?: string;
  startDate?: string;
  endDate?: string;
  limit?: number;
}

export const useAuditLogs = (filters?: AuditLogFilters) => {
  return useQuery({
    queryKey: ['auditLogs', filters],
    queryFn: () => auditLogsApi.getAll(filters),
  });
};

export const useRecentAuditLogs = (limit: number = 50) => {
  return useQuery({
    queryKey: ['auditLogs', 'recent', limit],
    queryFn: () => auditLogsApi.getRecent(limit),
  });
};

export const useAuditLogsByEntity = (entityType: string, entityId: string) => {
  return useQuery({
    queryKey: ['auditLogs', 'entity', entityType, entityId],
    queryFn: () => auditLogsApi.getByEntity(entityType, entityId),
    enabled: !!entityType && !!entityId,
  });
};

export const useAuditLogsByEventType = (eventType: string) => {
  return useQuery({
    queryKey: ['auditLogs', 'event', eventType],
    queryFn: () => auditLogsApi.getByEventType(eventType),
    enabled: !!eventType,
  });
};
