import { useState } from 'react';
import { useUpdateLeaveRequestStatus } from '../../hooks/useLeaveRequests';
import { Button } from '../common/Button';
import { Card, CardContent, CardHeader, CardTitle } from '../common/Card';
import { ApprovalHistory } from './ApprovalHistory';
import { X, CheckCircle2, XCircle, Ban } from 'lucide-react';
import { format } from 'date-fns';
import type { LeaveRequest } from '../../types';

interface LeaveRequestApproveModalProps {
  leaveRequest: LeaveRequest;
  onClose: () => void;
  onSuccess?: () => void;
}

export function LeaveRequestApproveModal({
  leaveRequest,
  onClose,
  onSuccess,
}: LeaveRequestApproveModalProps) {
  const updateStatus = useUpdateLeaveRequestStatus();
  const [comments, setComments] = useState('');

  const handleAction = async (status: LeaveRequest['status']) => {
    if (!leaveRequest.id) return;

    try {
      await updateStatus.mutateAsync({
        id: leaveRequest.id,
        status,
        comments: comments || undefined,
        changedBy: 'Current User', // TODO: Get from auth context
      });

      onSuccess?.();
      onClose();
    } catch (error) {
      console.error('Error updating leave request status:', error);
      alert('Failed to update leave request status. Please try again.');
    }
  };

  const canApprove = leaveRequest.status === 'Pending';
  const canReject = leaveRequest.status === 'Pending';
  const canCancel = leaveRequest.status === 'Pending';

  return (
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
      <Card className="w-full max-w-2xl max-h-[90vh] overflow-y-auto">
        <CardHeader>
          <div className="flex justify-between items-start">
            <CardTitle>Review Leave Request</CardTitle>
            <Button variant="ghost" size="icon" onClick={onClose}>
              <X className="h-4 w-4" />
            </Button>
          </div>
        </CardHeader>
        <CardContent className="space-y-6">
          {/* Leave Request Details */}
          <div className="space-y-4">
            <div>
              <h3 className="text-lg font-semibold mb-2">Employee</h3>
              <p className="text-muted-foreground">{leaveRequest.employeeName}</p>
            </div>

            <div className="grid gap-4 md:grid-cols-2">
              <div>
                <h3 className="text-sm font-medium mb-1">Leave Type</h3>
                <p className="text-muted-foreground">{leaveRequest.leaveType}</p>
              </div>
              <div>
                <h3 className="text-sm font-medium mb-1">Status</h3>
                <span
                  className={`px-2 py-1 rounded-full text-xs font-medium ${
                    leaveRequest.status === 'Pending'
                      ? 'bg-yellow-100 text-yellow-800'
                      : leaveRequest.status === 'Approved'
                      ? 'bg-green-100 text-green-800'
                      : leaveRequest.status === 'Rejected'
                      ? 'bg-red-100 text-red-800'
                      : 'bg-gray-100 text-gray-800'
                  }`}
                >
                  {leaveRequest.status}
                </span>
              </div>
            </div>

            <div className="grid gap-4 md:grid-cols-2">
              <div>
                <h3 className="text-sm font-medium mb-1">Start Date</h3>
                <p className="text-muted-foreground">
                  {format(new Date(leaveRequest.startDate), 'MMM dd, yyyy')}
                </p>
              </div>
              <div>
                <h3 className="text-sm font-medium mb-1">End Date</h3>
                <p className="text-muted-foreground">
                  {format(new Date(leaveRequest.endDate), 'MMM dd, yyyy')}
                </p>
              </div>
            </div>

            <div>
              <h3 className="text-sm font-medium mb-1">Duration</h3>
              <p className="text-muted-foreground">
                {Math.ceil(
                  (new Date(leaveRequest.endDate).getTime() -
                    new Date(leaveRequest.startDate).getTime()) /
                    (1000 * 60 * 60 * 24)
                ) + 1}{' '}
                day(s)
              </p>
            </div>

            {leaveRequest.reason && (
              <div>
                <h3 className="text-sm font-medium mb-1">Reason</h3>
                <p className="text-muted-foreground">{leaveRequest.reason}</p>
              </div>
            )}
          </div>

          {/* Approval History */}
          <ApprovalHistory history={leaveRequest.approvalHistory || []} />

          {/* Action Section */}
          {canApprove || canReject || canCancel ? (
            <div className="space-y-4 pt-4 border-t">
              <div>
                <label htmlFor="comments" className="text-sm font-medium mb-2 block">
                  Comments (Optional)
                </label>
                <textarea
                  id="comments"
                  value={comments}
                  onChange={(e) => setComments(e.target.value)}
                  rows={3}
                  className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-ring"
                  placeholder="Add any comments about your decision..."
                />
              </div>

              <div className="flex justify-end space-x-3">
                {canCancel && (
                  <Button
                    variant="outline"
                    onClick={() => handleAction('Cancelled')}
                    disabled={updateStatus.isPending}
                  >
                    <Ban className="h-4 w-4 mr-2" />
                    Cancel Request
                  </Button>
                )}
                {canReject && (
                  <Button
                    variant="destructive"
                    onClick={() => handleAction('Rejected')}
                    disabled={updateStatus.isPending}
                  >
                    <XCircle className="h-4 w-4 mr-2" />
                    Reject
                  </Button>
                )}
                {canApprove && (
                  <Button
                    onClick={() => handleAction('Approved')}
                    disabled={updateStatus.isPending}
                  >
                    <CheckCircle2 className="h-4 w-4 mr-2" />
                    Approve
                  </Button>
                )}
                <Button variant="outline" onClick={onClose}>
                  Close
                </Button>
              </div>
            </div>
          ) : (
            <div className="flex justify-end pt-4 border-t">
              <Button variant="outline" onClick={onClose}>
                Close
              </Button>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
