import { useParams, Link, useNavigate } from 'react-router-dom';
import { useProject, useDeleteProject } from '../../hooks/useProjects';
import { useProjectTasks } from '../../hooks/useTasks';
import { Loading } from '../../components/common/Loading';
import { Error } from '../../components/common/Error';
import { Button } from '../../components/common/Button';
import { ArrowLeft, Edit, Trash2 } from 'lucide-react';
import { ProjectOverview } from './components/ProjectOverview';
import { ProjectInfo } from './components/ProjectInfo';
import { ProjectMembers } from './components/ProjectMembers';
import { ProjectTasks } from './components/ProjectTasks';

export function ProjectDetail() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const deleteProject = useDeleteProject();
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

      <ProjectOverview project={project} tasks={tasks} />

      <div className="grid gap-6 md:grid-cols-2">
        <ProjectInfo project={project} />
        <ProjectMembers project={project} />
      </div>

      <ProjectTasks projectId={id!} />
    </div>
  );
}
