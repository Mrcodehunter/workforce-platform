import { format } from 'date-fns';
import { Clock, CheckCircle2, XCircle, Ban, AlertCircle } from 'lucide-react';
import type { ApprovalHistoryEntry } from '../../types';

interface ApprovalHistoryProps {
  history: ApprovalHistoryEntry[];
  className?: string;
}

const statusIcons = {
  Pending: AlertCircle,
  Approved: CheckCircle2,
  Rejected: XCircle,
  Cancelled: Ban,
};

const statusColors = {
  Pending: 'text-yellow-600',
  Approved: 'text-green-600',
  Rejected: 'text-red-600',
  Cancelled: 'text-gray-600',
};

export function ApprovalHistory({ history, className }: ApprovalHistoryProps) {
  if (!history || history.length === 0) {
    return (
      <div className={className}>
        <p className="text-sm text-muted-foreground">No approval history available</p>
      </div>
    );
  }

  return (
    <div className={className}>
      <h4 className="text-sm font-semibold mb-3">Approval History</h4>
      <div className="space-y-3">
        {history.map((entry, index) => {
          const Icon = statusIcons[entry.status as keyof typeof statusIcons] || Clock;
          const colorClass = statusColors[entry.status as keyof typeof statusColors] || 'text-gray-600';

          return (
            <div key={index} className="flex items-start space-x-3 p-3 border rounded-lg">
              <div className={`flex-shrink-0 ${colorClass}`}>
                <Icon className="h-5 w-5" />
              </div>
              <div className="flex-1 min-w-0">
                <div className="flex items-center justify-between">
                  <p className={`text-sm font-medium ${colorClass}`}>{entry.status}</p>
                  <p className="text-xs text-muted-foreground">
                    {format(new Date(entry.changedAt), 'MMM dd, yyyy HH:mm')}
                  </p>
                </div>
                <p className="text-xs text-muted-foreground mt-1">Changed by: {entry.changedBy}</p>
                {entry.comments && (
                  <p className="text-sm text-muted-foreground mt-2 italic">"{entry.comments}"</p>
                )}
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}
