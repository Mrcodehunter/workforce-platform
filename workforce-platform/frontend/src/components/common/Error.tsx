import { AlertCircle } from 'lucide-react';
import { Button } from './Button';

interface ErrorProps {
  message?: string;
  onRetry?: () => void;
}

export function Error({ message = 'An error occurred', onRetry }: ErrorProps) {
  return (
    <div className="flex flex-col items-center justify-center p-8 space-y-4">
      <AlertCircle className="h-12 w-12 text-destructive" />
      <p className="text-muted-foreground">{message}</p>
      {onRetry && (
        <Button onClick={onRetry} variant="outline">
          Retry
        </Button>
      )}
    </div>
  );
}
