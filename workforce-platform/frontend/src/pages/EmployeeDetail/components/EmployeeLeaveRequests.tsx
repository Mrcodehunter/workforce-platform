import { useQuery } from '@tanstack/react-query';
import { Card, CardContent, CardHeader, CardTitle } from '../../../components/common/Card';
import { leaveRequestsApi } from '../../../api';
import { Calendar } from 'lucide-react';
import { format } from 'date-fns';
import type { LeaveRequest } from '../../../types';

interface EmployeeLeaveRequestsProps {
  employeeId: string;
}

export function EmployeeLeaveRequests({ employeeId }: EmployeeLeaveRequestsProps) {
  const { data: leaveRequests } = useQuery({
    queryKey: ['leaveRequests', 'employee', employeeId],
    queryFn: () => leaveRequestsApi.getAll({ employeeId }),
    enabled: !!employeeId,
  });

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center">
          <Calendar className="h-5 w-5 mr-2" />
          Leave Request History
        </CardTitle>
      </CardHeader>
      <CardContent>
        {leaveRequests && leaveRequests.length > 0 ? (
          <div className="space-y-4">
            {leaveRequests.map((lr: LeaveRequest) => (
              <div key={lr.id} className="p-4 border rounded-lg">
                <div className="flex justify-between items-start">
                  <div>
                    <p className="font-medium">{lr.leaveType} Leave</p>
                    <p className="text-sm text-muted-foreground">
                      {format(new Date(lr.startDate), 'PPP')} - {format(new Date(lr.endDate), 'PPP')}
                    </p>
                    {lr.reason && <p className="text-sm mt-1">{lr.reason}</p>}
                  </div>
                  <span
                    className={`px-2 py-1 rounded-full text-xs ${
                      lr.status === 'Approved'
                        ? 'bg-green-100 text-green-800'
                        : lr.status === 'Rejected'
                        ? 'bg-red-100 text-red-800'
                        : 'bg-yellow-100 text-yellow-800'
                    }`}
                  >
                    {lr.status}
                  </span>
                </div>
                {lr.approvalHistory && Array.isArray(lr.approvalHistory) && lr.approvalHistory.length > 0 && (
                  <div className="mt-3 pt-3 border-t">
                    <p className="text-sm font-medium mb-2">Approval History</p>
                    <div className="space-y-1">
                      {lr.approvalHistory.map((entry, idx) => (
                        <div key={idx} className="text-sm text-muted-foreground">
                          <span className="font-medium">{entry.changedBy}</span> - {entry.status} on{' '}
                          {format(new Date(entry.changedAt), 'PPP')}
                          {entry.comments && <span>: {entry.comments}</span>}
                        </div>
                      ))}
                    </div>
                  </div>
                )}
              </div>
            ))}
          </div>
        ) : (
          <p className="text-muted-foreground">No leave requests</p>
        )}
      </CardContent>
    </Card>
  );
}
