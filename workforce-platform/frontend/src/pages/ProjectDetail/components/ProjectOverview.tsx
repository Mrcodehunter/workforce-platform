import { Card, CardContent } from '../../../components/common/Card';
import { Users, CheckCircle2 } from 'lucide-react';
import type { Project, TaskItem } from '../../../types';

interface ProjectOverviewProps {
  project: Project;
  tasks?: TaskItem[];
}

export function ProjectOverview({ project, tasks = [] }: ProjectOverviewProps) {
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

  const completedTasks = tasks.filter((t) => t.status === 'Done').length;
  const totalTasks = tasks.length;
  const progress = totalTasks > 0 ? Math.round((completedTasks / totalTasks) * 100) : 0;
  const memberCount = project.projectMembers?.length || 0;

  return (
    <div className="grid gap-4 md:grid-cols-4">
      <Card>
        <CardContent className="pt-6">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-muted-foreground">Status</p>
              <p className="text-2xl font-bold">{project.status}</p>
            </div>
            <span className={`px-3 py-1 rounded-full text-sm ${getStatusColor(project.status)}`}>
              {project.status}
            </span>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardContent className="pt-6">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-muted-foreground">Team Members</p>
              <p className="text-2xl font-bold">{memberCount}</p>
            </div>
            <Users className="h-8 w-8 text-muted-foreground" />
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardContent className="pt-6">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-muted-foreground">Tasks</p>
              <p className="text-2xl font-bold">
                {completedTasks}/{totalTasks}
              </p>
            </div>
            <CheckCircle2 className="h-8 w-8 text-muted-foreground" />
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardContent className="pt-6">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-muted-foreground">Progress</p>
              <p className="text-2xl font-bold">{progress}%</p>
            </div>
            <div className="w-16 h-16">
              <svg className="transform -rotate-90" viewBox="0 0 36 36">
                <circle
                  cx="18"
                  cy="18"
                  r="16"
                  fill="none"
                  stroke="#e5e7eb"
                  strokeWidth="3"
                />
                <circle
                  cx="18"
                  cy="18"
                  r="16"
                  fill="none"
                  stroke="#3b82f6"
                  strokeWidth="3"
                  strokeDasharray={`${progress}, 100`}
                />
              </svg>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
