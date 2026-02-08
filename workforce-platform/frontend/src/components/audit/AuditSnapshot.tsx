import { useState } from 'react';
import { ChevronDown, ChevronUp } from 'lucide-react';
import { Button } from '../common/Button';

interface AuditSnapshotProps {
  before?: any;
  after?: any;
  className?: string;
}

export function AuditSnapshot({ before, after, className }: AuditSnapshotProps) {
  const [expanded, setExpanded] = useState(false);

  const formatValue = (value: any): string => {
    if (value === null || value === undefined) return 'null';
    if (typeof value === 'object') {
      return JSON.stringify(value, null, 2);
    }
    return String(value);
  };

  const hasChanges = before !== undefined || after !== undefined;

  if (!hasChanges) {
    return null;
  }

  return (
    <div className={className}>
      <Button
        variant="ghost"
        size="sm"
        onClick={() => setExpanded(!expanded)}
        className="flex items-center space-x-2 mb-2"
      >
        <span className="text-sm font-medium">
          {expanded ? 'Hide' : 'Show'} Changes
        </span>
        {expanded ? (
          <ChevronUp className="h-4 w-4" />
        ) : (
          <ChevronDown className="h-4 w-4" />
        )}
      </Button>

      {expanded && (
        <div className="space-y-3 mt-2">
          {before !== undefined && (
            <div>
              <h5 className="text-xs font-semibold text-muted-foreground mb-1">Before</h5>
              <pre className="text-xs bg-red-50 border border-red-200 rounded p-2 overflow-x-auto">
                {formatValue(before)}
              </pre>
            </div>
          )}
          {after !== undefined && (
            <div>
              <h5 className="text-xs font-semibold text-muted-foreground mb-1">After</h5>
              <pre className="text-xs bg-green-50 border border-green-200 rounded p-2 overflow-x-auto">
                {formatValue(after)}
              </pre>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
