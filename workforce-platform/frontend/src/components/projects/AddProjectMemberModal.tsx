import { useState, useMemo } from 'react';
import { useEmployees } from '../../hooks/useEmployees';
import { useAddProjectMember } from '../../hooks/useProjects';
import { Button } from '../common/Button';
import { SearchableSelect } from '../common/SearchableSelect';
import { X } from 'lucide-react';

interface AddProjectMemberModalProps {
  projectId: string;
  existingMemberIds: string[];
  onClose: () => void;
}

const ROLE_OPTIONS = [
  'Developer',
  'Lead Developer',
  'QA Engineer',
  'Project Manager',
  'Tech Lead',
  'Architect',
  'Designer',
  'Business Analyst',
];

export function AddProjectMemberModal({
  projectId,
  existingMemberIds,
  onClose,
}: AddProjectMemberModalProps) {
  const { data: employees, isLoading: employeesLoading } = useEmployees();
  const addMember = useAddProjectMember();
  const [selectedEmployeeId, setSelectedEmployeeId] = useState<string>('');
  const [role, setRole] = useState<string>('');
  const [errors, setErrors] = useState<Record<string, string>>({});

  // Filter out employees already assigned to the project
  const availableEmployees = useMemo(() => {
    if (!employees) return [];
    return employees
      .filter((emp) => !existingMemberIds.includes(emp.id))
      .map((emp) => ({
        value: emp.id,
        label: `${emp.firstName} ${emp.lastName} (${emp.email})`,
      }));
  }, [employees, existingMemberIds]);

  const validate = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (!selectedEmployeeId) {
      newErrors.employeeId = 'Please select an employee';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validate()) return;

    try {
      await addMember.mutateAsync({
        projectId,
        employeeId: selectedEmployeeId,
        role: role || undefined,
      });
      onClose();
    } catch (error: any) {
      console.error('Error adding project member:', error);
      setErrors({
        submit: error.response?.data?.message || 'Failed to add member. Please try again.',
      });
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
      <div className="bg-background rounded-lg shadow-lg w-full max-w-md mx-4">
        <div className="flex items-center justify-between p-6 border-b">
          <h2 className="text-xl font-semibold">Add Team Member</h2>
          <button
            onClick={onClose}
            className="text-muted-foreground hover:text-foreground transition-colors"
          >
            <X className="h-5 w-5" />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="p-6 space-y-4">
          {errors.submit && (
            <div className="p-3 text-sm text-red-600 bg-red-50 border border-red-200 rounded-md">
              {errors.submit}
            </div>
          )}

          <div>
            <label className="block text-sm font-medium mb-2">
              Employee <span className="text-red-500">*</span>
            </label>
            <SearchableSelect
              options={availableEmployees}
              value={selectedEmployeeId}
              onChange={setSelectedEmployeeId}
              placeholder={employeesLoading ? 'Loading employees...' : 'Select an employee'}
              disabled={employeesLoading || addMember.isPending}
              emptyMessage="No available employees"
            />
            {errors.employeeId && (
              <p className="mt-1 text-sm text-red-600">{errors.employeeId}</p>
            )}
          </div>

          <div>
            <label className="block text-sm font-medium mb-2">Role (Optional)</label>
            <SearchableSelect
              options={ROLE_OPTIONS.map((r) => ({ value: r, label: r }))}
              value={role}
              onChange={setRole}
              placeholder="Select a role"
              disabled={addMember.isPending}
              emptyMessage="No roles available"
            />
          </div>

          <div className="flex justify-end space-x-2 pt-4">
            <Button type="button" variant="outline" onClick={onClose} disabled={addMember.isPending}>
              Cancel
            </Button>
            <Button type="submit" disabled={addMember.isPending || employeesLoading}>
              {addMember.isPending ? 'Adding...' : 'Add Member'}
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
}
