import { useState, useMemo } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { tasksApi } from '../../api';
import { useProject } from '../../hooks/useProjects';
import { Button } from '../common/Button';
import { Input } from '../common/Input';
import { SearchableSelect } from '../common/SearchableSelect';
import { X } from 'lucide-react';
import type { TaskItem, TaskPriority } from '../../types';

interface TaskCreateModalProps {
  projectId: string;
  onClose: () => void;
}

export function TaskCreateModal({ projectId, onClose }: TaskCreateModalProps) {
  const queryClient = useQueryClient();
  const { data: project, isLoading: projectLoading } = useProject(projectId);
  const [isCreating, setIsCreating] = useState(false);

  const [formData, setFormData] = useState<Partial<TaskItem>>({
    projectId: projectId,
    title: '',
    description: '',
    status: 'ToDo',
    priority: 0,
    assignedToEmployeeId: undefined,
    dueDate: '',
  });

  const [errors, setErrors] = useState<Record<string, string>>({});

  const validate = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (!formData.title?.trim()) newErrors.title = 'Task title is required';
    if (!formData.projectId) newErrors.projectId = 'Project is required';

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validate()) return;

    try {
      setIsCreating(true);
      const taskData: Partial<TaskItem> = {
        ...formData,
        projectId: projectId,
        dueDate: formData.dueDate ? new Date(formData.dueDate).toISOString() : undefined,
        priority: (formData.priority ?? 0) as TaskPriority,
      };

      await tasksApi.createForProject(projectId, taskData);
      
      // Invalidate queries to refresh data
      queryClient.invalidateQueries({ queryKey: ['tasks'] });
      queryClient.invalidateQueries({ queryKey: ['tasks', 'project', projectId] });
      queryClient.invalidateQueries({ queryKey: ['projects', projectId] });
      
      onClose();
    } catch (error) {
      console.error('Error creating task:', error);
    } finally {
      setIsCreating(false);
    }
  };

  const priorityOptions: { value: TaskPriority; label: string }[] = [
    { value: 0, label: 'Low' },
    { value: 1, label: 'Medium' },
    { value: 2, label: 'High' },
    { value: 3, label: 'Critical' },
  ];

  // Only show project members for task assignment
  const projectMembers = project?.projectMembers || [];

  // Prepare options for SearchableSelect
  const assigneeOptions = useMemo(() => {
    return projectMembers.map((pm) => ({
      value: pm.employeeId,
      label: `${pm.employee?.firstName || ''} ${pm.employee?.lastName || ''}${pm.role ? ` (${pm.role})` : ''}`.trim(),
    }));
  }, [projectMembers]);

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg p-6 w-full max-w-2xl max-h-[90vh] overflow-y-auto">
        <div className="flex justify-between items-center mb-4">
          <h2 className="text-2xl font-bold">Create New Task</h2>
          <Button variant="ghost" size="icon" onClick={onClose}>
            <X className="h-4 w-4" />
          </Button>
        </div>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label htmlFor="title" className="block text-sm font-medium mb-2">
              Task Title <span className="text-red-500">*</span>
            </label>
            <Input
              id="title"
              value={formData.title || ''}
              onChange={(e) => setFormData({ ...formData, title: e.target.value })}
              placeholder="Enter task title"
              className={errors.title ? 'border-red-500' : ''}
            />
            {errors.title && <p className="text-sm text-red-500 mt-1">{errors.title}</p>}
          </div>

          <div>
            <label htmlFor="description" className="block text-sm font-medium mb-2">
              Description
            </label>
            <textarea
              id="description"
              value={formData.description || ''}
              onChange={(e) => setFormData({ ...formData, description: e.target.value })}
              placeholder="Enter task description"
              rows={4}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label htmlFor="priority" className="block text-sm font-medium mb-2">
                Priority
              </label>
              <select
                id="priority"
                value={formData.priority ?? 0}
                onChange={(e) => setFormData({ ...formData, priority: parseInt(e.target.value, 10) as TaskPriority })}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              >
                {priorityOptions.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </select>
            </div>

            <div>
              <label htmlFor="dueDate" className="block text-sm font-medium mb-2">
                Due Date
              </label>
              <Input
                id="dueDate"
                type="date"
                value={formData.dueDate || ''}
                onChange={(e) => setFormData({ ...formData, dueDate: e.target.value })}
                min={new Date().toISOString().split('T')[0]}
              />
            </div>
          </div>

          <div>
            <label htmlFor="assignedTo" className="block text-sm font-medium mb-2">
              Assign To
            </label>
            {projectLoading ? (
              <div className="w-full px-3 py-2 border border-gray-300 rounded-md bg-gray-50 text-muted-foreground">
                Loading project members...
              </div>
            ) : projectMembers.length === 0 ? (
              <div>
                <div className="w-full px-3 py-2 border border-gray-300 rounded-md bg-gray-50 text-muted-foreground">
                  No project members available
                </div>
                <p className="text-xs text-muted-foreground mt-1">
                  Add team members to the project first to assign tasks
                </p>
              </div>
            ) : (
              <SearchableSelect
                options={[
                  { value: '', label: 'Unassigned' },
                  ...assigneeOptions,
                ]}
                value={formData.assignedToEmployeeId || ''}
                onChange={(value) => setFormData({ ...formData, assignedToEmployeeId: value || undefined })}
                placeholder="Select team member..."
                emptyMessage="No matching team members found"
              />
            )}
          </div>

          <div className="flex justify-end gap-4 pt-4">
            <Button type="button" variant="outline" onClick={onClose} disabled={isCreating}>
              Cancel
            </Button>
            <Button type="submit" disabled={isCreating}>
              {isCreating ? 'Creating...' : 'Create Task'}
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
}
