import { Card, CardContent, CardHeader, CardTitle } from '../../../components/common/Card';
import { format } from 'date-fns';
import type { Project } from '../../../types';

interface ProjectInfoProps {
  project: Project;
}

export function ProjectInfo({ project }: ProjectInfoProps) {
  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Active':
        return 'bg-green-100 text-green-800';
      case 'Completed':
        return 'bg-blue-100 text-blue-800';
      case 'OnHold':
        return 'bg-yellow-100 text-yellow-800';
      case 'Cancelled':
        return 'bg-red-100 text-red-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle>Project Information</CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        <div>
          <label className="text-sm font-medium text-muted-foreground">Status</label>
          <p>
            <span className={`px-2 py-1 rounded-full text-sm ${getStatusColor(project.status)}`}>
              {project.status}
            </span>
          </p>
        </div>
        <div>
          <label className="text-sm font-medium text-muted-foreground">Start Date</label>
          <p>{format(new Date(project.startDate), 'PPP')}</p>
        </div>
        {project.endDate && (
          <div>
            <label className="text-sm font-medium text-muted-foreground">End Date</label>
            <p>{format(new Date(project.endDate), 'PPP')}</p>
          </div>
        )}
        <div>
          <label className="text-sm font-medium text-muted-foreground">Description</label>
          <p className="text-sm">{project.description || 'No description provided'}</p>
        </div>
      </CardContent>
    </Card>
  );
}
