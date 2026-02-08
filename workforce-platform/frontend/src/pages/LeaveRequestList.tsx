import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useLeaveRequests } from '../hooks/useLeaveRequests';
import { Loading } from '../components/common/Loading';
import { Error } from '../components/common/Error';
import { EmptyState } from '../components/common/EmptyState';
import { Button } from '../components/common/Button';
import { Card, CardContent, CardHeader, CardTitle } from '../components/common/Card';
import { ApprovalHistory } from '../components/leave-requests/ApprovalHistory';
import { LeaveRequestApproveModal } from '../components/leave-requests/LeaveRequestApproveModal';
import { Calendar, Plus, Filter, ChevronDown, ChevronUp, CheckCircle2 } from 'lucide-react';
import { format } from 'date-fns';
import type { LeaveRequest, LeaveStatus, LeaveType } from '../types';

export function LeaveRequestList() {
  const [statusFilter, setStatusFilter] = useState<string>('all');
  const [leaveTypeFilter, setLeaveTypeFilter] = useState<string>('all');
  const [expandedId, setExpandedId] = useState<string | null>(null);
  const [approveModalRequest, setApproveModalRequest] = useState<LeaveRequest | null>(null);

  const filters = {
    status: statusFilter !== 'all' ? statusFilter : undefined,
    leaveType: leaveTypeFilter !== 'all' ? leaveTypeFilter : undefined,
  };

  const { data: leaveRequests, isLoading, error, refetch } = useLeaveRequests(filters);

  if (isLoading) return <Loading />;
  if (error) return <Error message="Failed to load leave requests" onRetry={refetch} />;

  const getStatusColor = (status: LeaveStatus) => {
    switch (status) {
      case 'Pending':
        return 'bg-yellow-100 text-yellow-800';
      case 'Approved':
        return 'bg-green-100 text-green-800';
      case 'Rejected':
        return 'bg-red-100 text-red-800';
      case 'Cancelled':
        return 'bg-gray-100 text-gray-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  const getLeaveTypeColor = (leaveType: LeaveType) => {
    switch (leaveType) {
      case 'Sick':
        return 'bg-red-50 text-red-700';
      case 'Casual':
        return 'bg-blue-50 text-blue-700';
      case 'Annual':
        return 'bg-purple-50 text-purple-700';
      case 'Unpaid':
        return 'bg-orange-50 text-orange-700';
      default:
        return 'bg-gray-50 text-gray-700';
    }
  };

  const toggleExpand = (id: string) => {
    setExpandedId(expandedId === id ? null : id);
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold">Leave Requests</h1>
          <p className="text-muted-foreground">Manage employee leave requests and approvals</p>
        </div>
        <Link to="/leave-requests/new">
          <Button>
            <Plus className="h-4 w-4 mr-2" />
            Submit Leave Request
          </Button>
        </Link>
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
          <div className="grid gap-4 md:grid-cols-2">
            <div>
              <label className="text-sm font-medium mb-2 block">Status</label>
              <select
                value={statusFilter}
                onChange={(e) => setStatusFilter(e.target.value)}
                className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-ring"
              >
                <option value="all">All Statuses</option>
                <option value="Pending">Pending</option>
                <option value="Approved">Approved</option>
                <option value="Rejected">Rejected</option>
                <option value="Cancelled">Cancelled</option>
              </select>
            </div>
            <div>
              <label className="text-sm font-medium mb-2 block">Leave Type</label>
              <select
                value={leaveTypeFilter}
                onChange={(e) => setLeaveTypeFilter(e.target.value)}
                className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-ring"
              >
                <option value="all">All Types</option>
                <option value="Sick">Sick</option>
                <option value="Casual">Casual</option>
                <option value="Annual">Annual</option>
                <option value="Unpaid">Unpaid</option>
              </select>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Leave Requests List */}
      {!leaveRequests || leaveRequests.length === 0 ? (
        <EmptyState
          icon={Calendar}
          title="No leave requests found"
          description="No leave requests match your filters. Try adjusting your filters or submit a new request."
        />
      ) : (
        <div className="space-y-4">
          {leaveRequests.map((request) => (
            <Card key={request.id} className="hover:shadow-md transition-shadow">
              <CardContent className="pt-6">
                <div className="space-y-4">
                  <div className="flex justify-between items-start">
                    <div className="flex-1">
                      <div className="flex items-center space-x-3 mb-2">
                        <h3 className="text-lg font-semibold">{request.employeeName}</h3>
                        <span
                          className={`px-2 py-1 rounded-full text-xs font-medium ${getStatusColor(
                            request.status
                          )}`}
                        >
                          {request.status}
                        </span>
                        <span
                          className={`px-2 py-1 rounded-full text-xs font-medium ${getLeaveTypeColor(
                            request.leaveType
                          )}`}
                        >
                          {request.leaveType}
                        </span>
                      </div>
                      <div className="flex items-center space-x-4 text-sm text-muted-foreground">
                        <div className="flex items-center">
                          <Calendar className="h-4 w-4 mr-1" />
                          {format(new Date(request.startDate), 'MMM dd, yyyy')} -{' '}
                          {format(new Date(request.endDate), 'MMM dd, yyyy')}
                        </div>
                        <div>
                          {Math.ceil(
                            (new Date(request.endDate).getTime() -
                              new Date(request.startDate).getTime()) /
                              (1000 * 60 * 60 * 24)
                          ) + 1}{' '}
                          day(s)
                        </div>
                      </div>
                      {request.reason && (
                        <p className="text-sm text-muted-foreground mt-2">{request.reason}</p>
                      )}
                    </div>
                    <div className="flex items-center space-x-2">
                      {request.status === 'Pending' && (
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => setApproveModalRequest(request)}
                        >
                          <CheckCircle2 className="h-4 w-4 mr-1" />
                          Review
                        </Button>
                      )}
                      <Button
                        variant="ghost"
                        size="icon"
                        onClick={() => request.id && toggleExpand(request.id)}
                      >
                        {expandedId === request.id ? (
                          <ChevronUp className="h-4 w-4" />
                        ) : (
                          <ChevronDown className="h-4 w-4" />
                        )}
                      </Button>
                    </div>
                  </div>

                  {expandedId === request.id && (
                    <div className="pt-4 border-t">
                      <ApprovalHistory history={request.approvalHistory || []} />
                    </div>
                  )}
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}

      {approveModalRequest && (
        <LeaveRequestApproveModal
          leaveRequest={approveModalRequest}
          onClose={() => setApproveModalRequest(null)}
          onSuccess={() => {
            setApproveModalRequest(null);
            refetch();
          }}
        />
      )}
    </div>
  );
}
