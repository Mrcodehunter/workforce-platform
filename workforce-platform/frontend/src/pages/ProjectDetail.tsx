import { useState } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { useProject, useDeleteProject } from '../hooks/useProjects';
import { useProjectTasks } from '../hooks/useTasks';
import { TaskBoard } from '../components/projects/TaskBoard';
import { TaskCreateModal } from '../components/projects/TaskCreateModal';
import { Loading } from '../components/common/Loading';
import { Error } from '../components/common/Error';
import { Card, CardContent, CardHeader, CardTitle } from '../components/common/Card';
import { Button } from '../components/common/Button';
import { ArrowLeft, Edit, Trash2, Users, CheckCircle2 } from 'lucide-react';
import { format } from 'date-fns';

export function ProjectDetail() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const deleteProject = useDeleteProject();
  const [showCreateTask, setShowCreateTask] = useState(false);

  const { data: project, isLoading, error, refetch } = useProject(id!);
  const { data: tasks } = useProjectTasks(id!);

  const handleDelete = async () => {
    if (!id || !window.confirm('Are you sure you want to delete this project? This action cannot be undone.')) {
      return;
    }

    try {
      await deleteProject.mutateAsync(id);
      navigate('/projects');
    } catch (error) {
      console.error('Error deleting project:', error);
      alert('Failed to delete project. Please try again.');
    }
  };

  if (isLoading) return <Loading />;
  if (error) return <Error message="Failed to load project" onRetry={refetch} />;
  if (!project) return null;

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

  const completedTasks = (tasks || []).filter((t) => t.status === 'Done').length;
  const totalTasks = (tasks || []).length;
  const progress = totalTasks > 0 ? Math.round((completedTasks / totalTasks) * 100) : 0;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-4">
          <Link to="/projects">
            <Button variant="outline" size="icon">
              <ArrowLeft className="h-4 w-4" />
            </Button>
          </Link>
          <div>
            <h1 className="text-3xl font-bold">{project.name}</h1>
            <p className="text-muted-foreground">{project.description || 'No description'}</p>
          </div>
        </div>
        <div className="flex space-x-2">
          <Button variant="outline" onClick={() => navigate(`/projects/${id}/edit`)}>
            <Edit className="h-4 w-4 mr-2" />
            Edit
          </Button>
          <Button variant="destructive" onClick={handleDelete} disabled={deleteProject.isPending}>
            <Trash2 className="h-4 w-4 mr-2" />
            {deleteProject.isPending ? 'Deleting...' : 'Delete'}
          </Button>
        </div>
      </div>

      {/* Project Overview */}
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
                <p className="text-2xl font-bold">
                  {project.projectMembers && Array.isArray(project.projectMembers)
                    ? project.projectMembers.length
                    : 0}
                </p>
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

      <div className="grid gap-6 md:grid-cols-2">
        {/* Project Information */}
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

        {/* Team Members */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center">
              <Users className="h-5 w-5 mr-2" />
              Team Members
            </CardTitle>
          </CardHeader>
          <CardContent>
            {project.projectMembers && project.projectMembers.length > 0 ? (
              <div className="space-y-2">
                {project.projectMembers.map((pm) => (
                  <div key={`${pm.projectId}-${pm.employeeId}`} className="p-3 border rounded-lg">
                    <p className="font-medium">
                      {pm.employee?.firstName} {pm.employee?.lastName}
                    </p>
                    <p className="text-sm text-muted-foreground">Role: {pm.role || 'Member'}</p>
                    <p className="text-xs text-muted-foreground">
                      Joined: {format(new Date(pm.joinedAt), 'MMM dd, yyyy')}
                    </p>
                  </div>
                ))}
              </div>
            ) : (
              <p className="text-muted-foreground">No team members assigned</p>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Task Board */}
      <Card>
        <CardContent className="pt-6">
          <TaskBoard
            projectId={id!}
            onCreateTask={() => setShowCreateTask(true)}
          />
        </CardContent>
      </Card>

      {showCreateTask && (
        <TaskCreateModal
          projectId={id!}
          onClose={() => setShowCreateTask(false)}
        />
      )}
    </div>
  );
}
