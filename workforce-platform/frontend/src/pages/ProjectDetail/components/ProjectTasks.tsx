import { useState } from 'react';
import { Card, CardContent } from '../../../components/common/Card';
import { TaskBoard } from '../../../components/projects/TaskBoard';
import { TaskCreateModal } from '../../../components/projects/TaskCreateModal';

interface ProjectTasksProps {
  projectId: string;
}

export function ProjectTasks({ projectId }: ProjectTasksProps) {
  const [showCreateTask, setShowCreateTask] = useState(false);

  return (
    <>
      <Card>
        <CardContent className="pt-6">
          <TaskBoard projectId={projectId} onCreateTask={() => setShowCreateTask(true)} />
        </CardContent>
      </Card>

      {showCreateTask && (
        <TaskCreateModal projectId={projectId} onClose={() => setShowCreateTask(false)} />
      )}
    </>
  );
}
