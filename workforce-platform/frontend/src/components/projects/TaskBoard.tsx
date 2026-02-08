import { useProjectTasks, useUpdateTaskStatus } from '../../hooks/useTasks';
import { Loading } from '../common/Loading';
import { Error } from '../common/Error';
import { TaskCard } from './TaskCard';
import { Plus } from 'lucide-react';
import { Button } from '../common/Button';
import type { TaskStatus } from '../../types';

interface TaskBoardProps {
  projectId: string;
  onCreateTask: () => void;
}

const statusColumns: { status: TaskStatus; label: string; color: string }[] = [
  { status: 'ToDo', label: 'To Do', color: 'bg-gray-100' },
  { status: 'InProgress', label: 'In Progress', color: 'bg-blue-100' },
  { status: 'InReview', label: 'In Review', color: 'bg-yellow-100' },
  { status: 'Done', label: 'Done', color: 'bg-green-100' },
  { status: 'Cancelled', label: 'Cancelled', color: 'bg-red-100' },
];

export function TaskBoard({ projectId, onCreateTask }: TaskBoardProps) {
  const { data: tasks, isLoading, error, refetch } = useProjectTasks(projectId);
  const updateStatus = useUpdateTaskStatus();

  if (isLoading) return <Loading />;
  if (error) return <Error message="Failed to load tasks" onRetry={refetch} />;

  const tasksByStatus = (status: TaskStatus) => {
    return (tasks || []).filter((task) => task.status === status);
  };

  const handleStatusChange = async (taskId: string, newStatus: TaskStatus) => {
    try {
      await updateStatus.mutateAsync({ 
        id: taskId, 
        status: newStatus,
        projectId: projectId 
      });
    } catch (error) {
      console.error('Error updating task status:', error);
    }
  };

  return (
    <div className="space-y-4">
      <div className="flex justify-between items-center">
        <h2 className="text-2xl font-bold">Task Board</h2>
        <Button onClick={onCreateTask}>
          <Plus className="h-4 w-4 mr-2" />
          Create Task
        </Button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-5 gap-4 overflow-x-auto">
        {statusColumns.map((column) => {
          const columnTasks = tasksByStatus(column.status);
          return (
            <div
              key={column.status}
              className={`${column.color} rounded-lg p-4 min-h-[400px]`}
            >
              <div className="flex justify-between items-center mb-4">
                <h3 className="font-semibold">{column.label}</h3>
                <span className="text-sm text-gray-600 bg-white px-2 py-1 rounded-full">
                  {columnTasks.length}
                </span>
              </div>
              <div className="space-y-2">
                {columnTasks.length === 0 ? (
                  <div className="text-sm text-gray-500 text-center py-8">
                    No tasks
                  </div>
                ) : (
                  columnTasks.map((task) => (
                    <TaskCard
                      key={task.id}
                      task={task}
                      onStatusChange={handleStatusChange}
                      availableStatuses={statusColumns.map((c) => c.status)}
                    />
                  ))
                )}
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}
