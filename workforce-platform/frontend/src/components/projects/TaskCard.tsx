import { format } from 'date-fns';
import type { TaskItem, TaskStatus } from '../../types';

interface TaskCardProps {
  task: TaskItem;
  onStatusChange: (taskId: string, newStatus: TaskStatus) => void;
  availableStatuses: TaskStatus[];
}

const priorityColors: Record<number, string> = {
  0: 'bg-gray-200 text-gray-800',
  1: 'bg-blue-200 text-blue-800',
  2: 'bg-orange-200 text-orange-800',
  3: 'bg-red-200 text-red-800',
};

const priorityLabels: Record<number, string> = {
  0: 'Low',
  1: 'Medium',
  2: 'High',
  3: 'Critical',
};

export function TaskCard({ task, onStatusChange, availableStatuses }: TaskCardProps) {
  const isOverdue = task.dueDate && new Date(task.dueDate) < new Date() && task.status !== 'Done';

  return (
    <div className="bg-white rounded-lg p-3 shadow-sm border border-gray-200 hover:shadow-md transition-shadow">
      <div className="space-y-2">
        <div className="flex justify-between items-start">
          <h4 className="font-semibold text-sm line-clamp-2">{task.title}</h4>
          <span
            className={`text-xs px-2 py-1 rounded-full ${priorityColors[task.priority] || priorityColors[0]}`}
          >
            {priorityLabels[task.priority] || 'Low'}
          </span>
        </div>

        {task.description && (
          <p className="text-xs text-gray-600 line-clamp-2">{task.description}</p>
        )}

        {task.assignedToEmployee && (
          <div className="text-xs text-gray-500">
            Assigned to: {task.assignedToEmployee.firstName} {task.assignedToEmployee.lastName}
          </div>
        )}

        {task.dueDate && (
          <div
            className={`text-xs ${
              isOverdue ? 'text-red-600 font-semibold' : 'text-gray-500'
            }`}
          >
            Due: {format(new Date(task.dueDate), 'MMM dd, yyyy')}
            {isOverdue && ' (Overdue)'}
          </div>
        )}

        <div className="pt-2 border-t border-gray-200">
          <select
            value={task.status}
            onChange={(e) => onStatusChange(task.id, e.target.value as TaskStatus)}
            className="w-full text-xs px-2 py-1 border border-gray-300 rounded focus:outline-none focus:ring-1 focus:ring-blue-500"
          >
            {availableStatuses.map((status) => (
              <option key={status} value={status}>
                {status === 'ToDo' ? 'To Do' : status}
              </option>
            ))}
          </select>
        </div>
      </div>
    </div>
  );
}
