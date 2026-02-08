import { Link } from 'react-router-dom';
import { useProjects } from '../hooks/useProjects';
import { Loading } from '../components/common/Loading';
import { Error } from '../components/common/Error';
import { EmptyState } from '../components/common/EmptyState';
import { Button } from '../components/common/Button';
import { Card, CardContent, CardHeader, CardTitle } from '../components/common/Card';
import { FolderKanban, Plus, Users, CheckCircle2 } from 'lucide-react';
import { format } from 'date-fns';

export function ProjectList() {
  const { data: projects, isLoading, error, refetch } = useProjects();

  if (isLoading) return <Loading />;
  if (error) return <Error message="Failed to load projects" onRetry={refetch} />;

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
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold">Projects</h1>
          <p className="text-muted-foreground">Track projects and team assignments</p>
        </div>
        <Link to="/projects/new">
          <Button>
            <Plus className="h-4 w-4 mr-2" />
            New Project
          </Button>
        </Link>
      </div>

      {!projects || projects.length === 0 ? (
        <EmptyState
          icon={FolderKanban}
          title="No projects found"
          description="Create your first project to get started"
        />
      ) : (
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
          {projects.map((project) => (
            <Card key={project.id} className="hover:shadow-lg transition-shadow">
              <CardHeader>
                <div className="flex justify-between items-start">
                  <CardTitle className="text-lg">{project.name}</CardTitle>
                  <span
                    className={`px-2 py-1 rounded-full text-xs ${getStatusColor(project.status)}`}
                  >
                    {project.status}
                  </span>
                </div>
              </CardHeader>
              <CardContent className="space-y-4">
                {project.description && (
                  <p className="text-sm text-muted-foreground line-clamp-2">
                    {project.description}
                  </p>
                )}
                <div className="flex items-center space-x-4 text-sm text-muted-foreground">
                  <div className="flex items-center">
                    <Users className="h-4 w-4 mr-1" />
                    {project.memberCount || 0} members
                  </div>
                  <div className="flex items-center">
                    <CheckCircle2 className="h-4 w-4 mr-1" />
                    {project.taskCount || 0} tasks
                  </div>
                </div>
                <div className="text-sm text-muted-foreground">
                  <p>Start: {format(new Date(project.startDate), 'MMM dd, yyyy')}</p>
                  {project.endDate && (
                    <p>End: {format(new Date(project.endDate), 'MMM dd, yyyy')}</p>
                  )}
                </div>
                <Link to={`/projects/${project.id}`}>
                  <Button variant="outline" className="w-full">
                    View Details
                  </Button>
                </Link>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}
