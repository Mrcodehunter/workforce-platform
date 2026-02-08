import { useAuditLogsByEntity } from '../../hooks/useAuditLogs';
import { Loading } from '../common/Loading';
import { Error } from '../common/Error';
import { Card, CardContent, CardHeader, CardTitle } from '../common/Card';
import { AuditSnapshot } from './AuditSnapshot';
import { Activity, Clock, User } from 'lucide-react';
import { format } from 'date-fns';

interface EntityAuditLogProps {
  entityType: string;
  entityId: string;
  className?: string;
}

const eventTypeLabels: Record<string, string> = {
  'employee.created': 'Employee Created',
  'employee.updated': 'Employee Updated',
  'employee.deleted': 'Employee Deleted',
  'project.created': 'Project Created',
  'project.updated': 'Project Updated',
  'project.deleted': 'Project Deleted',
  'task.created': 'Task Created',
  'task.updated': 'Task Updated',
  'task.deleted': 'Task Deleted',
  'task.status.updated': 'Task Status Updated',
  'leave.request.created': 'Leave Request Created',
  'leave.request.approved': 'Leave Request Approved',
  'leave.request.rejected': 'Leave Request Rejected',
  'leave.request.cancelled': 'Leave Request Cancelled',
};

const eventTypeColors: Record<string, string> = {
  'employee.created': 'text-green-600',
  'employee.updated': 'text-blue-600',
  'employee.deleted': 'text-red-600',
  'project.created': 'text-green-600',
  'project.updated': 'text-blue-600',
  'project.deleted': 'text-red-600',
  'task.created': 'text-green-600',
  'task.updated': 'text-blue-600',
  'task.deleted': 'text-red-600',
  'task.status.updated': 'text-purple-600',
  'leave.request.created': 'text-green-600',
  'leave.request.approved': 'text-green-600',
  'leave.request.rejected': 'text-red-600',
  'leave.request.cancelled': 'text-gray-600',
};

export function EntityAuditLog({ entityType, entityId, className }: EntityAuditLogProps) {
  const { data: auditLogs, isLoading, error, refetch } = useAuditLogsByEntity(entityType, entityId);

  if (isLoading) return <Loading />;
  if (error) return <Error message="Failed to load audit logs" onRetry={refetch} />;

  if (!auditLogs || auditLogs.length === 0) {
    return (
      <Card className={className}>
        <CardHeader>
          <CardTitle className="flex items-center">
            <Activity className="h-5 w-5 mr-2" />
            Audit Trail
          </CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-muted-foreground">No audit history available for this entity.</p>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card className={className}>
      <CardHeader>
        <CardTitle className="flex items-center">
          <Activity className="h-5 w-5 mr-2" />
          Audit Trail
        </CardTitle>
      </CardHeader>
      <CardContent>
        <div className="space-y-4">
          {auditLogs.map((log) => {
            const eventLabel = eventTypeLabels[log.eventType] || log.eventType;
            const eventColor = eventTypeColors[log.eventType] || 'text-gray-600';

            return (
              <div key={log.id} className="border-l-2 border-gray-200 pl-4 pb-4">
                <div className="flex items-start justify-between">
                  <div className="flex-1">
                    <div className="flex items-center space-x-2 mb-1">
                      <span className={`text-sm font-semibold ${eventColor}`}>
                        {eventLabel}
                      </span>
                      {log.actor && (
                        <span className="text-xs text-muted-foreground flex items-center">
                          <User className="h-3 w-3 mr-1" />
                          {log.actor}
                        </span>
                      )}
                    </div>
                    <div className="flex items-center text-xs text-muted-foreground mb-2">
                      <Clock className="h-3 w-3 mr-1" />
                      {format(new Date(log.timestamp), 'MMM dd, yyyy HH:mm:ss')}
                    </div>
                    <AuditSnapshot before={log.before} after={log.after} />
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      </CardContent>
    </Card>
  );
}
