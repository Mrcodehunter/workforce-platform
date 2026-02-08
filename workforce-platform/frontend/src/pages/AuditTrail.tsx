import { useState } from 'react';
import { useAuditLogs } from '../hooks/useAuditLogs';
import { Loading } from '../components/common/Loading';
import { Error } from '../components/common/Error';
import { EmptyState } from '../components/common/EmptyState';
import { Card, CardContent, CardHeader, CardTitle } from '../components/common/Card';
import { AuditSnapshot } from '../components/audit/AuditSnapshot';
import { Activity, Filter, Clock, User } from 'lucide-react';
import { format } from 'date-fns';
import type { AuditLog } from '../types';

const entityTypes = ['All', 'Employee', 'Project', 'Task', 'LeaveRequest'];
const eventTypes = [
  'All',
  'employee.created',
  'employee.updated',
  'employee.deleted',
  'project.created',
  'project.updated',
  'project.deleted',
  'task.created',
  'task.updated',
  'task.deleted',
  'task.status.updated',
  'leave.request.created',
  'leave.request.approved',
  'leave.request.rejected',
  'leave.request.cancelled',
];

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
  'employee.created': 'bg-green-100 text-green-800',
  'employee.updated': 'bg-blue-100 text-blue-800',
  'employee.deleted': 'bg-red-100 text-red-800',
  'project.created': 'bg-green-100 text-green-800',
  'project.updated': 'bg-blue-100 text-blue-800',
  'project.deleted': 'bg-red-100 text-red-800',
  'task.created': 'bg-green-100 text-green-800',
  'task.updated': 'bg-blue-100 text-blue-800',
  'task.deleted': 'bg-red-100 text-red-800',
  'task.status.updated': 'bg-purple-100 text-purple-800',
  'leave.request.created': 'bg-green-100 text-green-800',
  'leave.request.approved': 'bg-green-100 text-green-800',
  'leave.request.rejected': 'bg-red-100 text-red-800',
  'leave.request.cancelled': 'bg-gray-100 text-gray-800',
};

export function AuditTrail() {
  const [entityTypeFilter, setEntityTypeFilter] = useState<string>('All');
  const [eventTypeFilter, setEventTypeFilter] = useState<string>('All');
  const [startDate, setStartDate] = useState<string>('');
  const [endDate, setEndDate] = useState<string>('');

  const filters = {
    entityType: entityTypeFilter !== 'All' ? entityTypeFilter : undefined,
    eventType: eventTypeFilter !== 'All' ? eventTypeFilter : undefined,
    startDate: startDate || undefined,
    endDate: endDate || undefined,
    limit: 200,
  };

  const { data: auditLogs, isLoading, error, refetch } = useAuditLogs(filters);

  const handleClearFilters = () => {
    setEntityTypeFilter('All');
    setEventTypeFilter('All');
    setStartDate('');
    setEndDate('');
  };

  if (isLoading) return <Loading />;
  if (error) return <Error message="Failed to load audit logs" onRetry={refetch} />;

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Audit Trail</h1>
        <p className="text-muted-foreground">System-wide activity log and change history</p>
      </div>

      {/* Filters */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center">
            <Filter className="h-5 w-5 mr-2" />
            Filters
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
            <div>
              <label className="block text-sm font-medium text-muted-foreground mb-1">
                Entity Type
              </label>
              <select
                value={entityTypeFilter}
                onChange={(e) => setEntityTypeFilter(e.target.value)}
                className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-ring"
              >
                {entityTypes.map((type) => (
                  <option key={type} value={type}>
                    {type}
                  </option>
                ))}
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-muted-foreground mb-1">
                Event Type
              </label>
              <select
                value={eventTypeFilter}
                onChange={(e) => setEventTypeFilter(e.target.value)}
                className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-ring"
              >
                {eventTypes.map((type) => (
                  <option key={type} value={type}>
                    {eventTypeLabels[type] || type}
                  </option>
                ))}
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-muted-foreground mb-1">
                Start Date
              </label>
              <input
                type="date"
                value={startDate}
                onChange={(e) => setStartDate(e.target.value)}
                className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-ring"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-muted-foreground mb-1">
                End Date
              </label>
              <input
                type="date"
                value={endDate}
                onChange={(e) => setEndDate(e.target.value)}
                className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-ring"
              />
            </div>
          </div>
          <div className="mt-4 flex justify-end">
            <button
              onClick={handleClearFilters}
              className="text-sm text-muted-foreground hover:text-foreground"
            >
              Clear Filters
            </button>
          </div>
        </CardContent>
      </Card>

      {/* Audit Logs List */}
      {!auditLogs || auditLogs.length === 0 ? (
        <EmptyState
          icon={Activity}
          title="No audit logs found"
          description="No audit logs match your filters. Try adjusting your filters or check back later."
        />
      ) : (
        <div className="space-y-4">
          {auditLogs.map((log: AuditLog) => {
            const eventLabel = eventTypeLabels[log.eventType] || log.eventType;
            const eventColor = eventTypeColors[log.eventType] || 'bg-gray-100 text-gray-800';

            return (
              <Card key={log.id} className="hover:shadow-md transition-shadow">
                <CardContent className="pt-6">
                  <div className="flex items-start justify-between">
                    <div className="flex-1">
                      <div className="flex items-center space-x-3 mb-2">
                        <span className={`px-2 py-1 rounded-full text-xs font-medium ${eventColor}`}>
                          {eventLabel}
                        </span>
                        <span className="text-sm font-medium">
                          {log.entityType} #{log.entityId}
                        </span>
                      </div>
                      <div className="flex items-center space-x-4 text-sm text-muted-foreground mb-3">
                        {log.actor && (
                          <div className="flex items-center">
                            <User className="h-4 w-4 mr-1" />
                            {log.actor}
                          </div>
                        )}
                        <div className="flex items-center">
                          <Clock className="h-4 w-4 mr-1" />
                          {format(new Date(log.timestamp), 'MMM dd, yyyy HH:mm:ss')}
                        </div>
                        {log.eventId && (
                          <div className="text-xs">
                            Event ID: {log.eventId.substring(0, 8)}...
                          </div>
                        )}
                      </div>
                      <AuditSnapshot before={log.before} after={log.after} />
                    </div>
                  </div>
                </CardContent>
              </Card>
            );
          })}
        </div>
      )}
    </div>
  );
}
