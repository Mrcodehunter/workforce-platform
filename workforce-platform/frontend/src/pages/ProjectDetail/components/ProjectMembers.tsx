import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '../../../components/common/Card';
import { Button } from '../../../components/common/Button';
import { AddProjectMemberModal } from '../../../components/projects/AddProjectMemberModal';
import { useRemoveProjectMember } from '../../../hooks/useProjects';
import { Users, X } from 'lucide-react';
import { format } from 'date-fns';
import type { Project } from '../../../types';

interface ProjectMembersProps {
  project: Project;
}

export function ProjectMembers({ project }: ProjectMembersProps) {
  const [showAddModal, setShowAddModal] = useState(false);
  const removeMember = useRemoveProjectMember();

  const existingMemberIds = (project.projectMembers || []).map((pm) => pm.employeeId);

  const handleRemove = async (employeeId: string) => {
    if (
      !window.confirm(
        'Are you sure you want to remove this member from the project? This action cannot be undone.'
      )
    ) {
      return;
    }

    try {
      await removeMember.mutateAsync({
        projectId: project.id,
        employeeId,
      });
    } catch (error) {
      console.error('Error removing member:', error);
      alert('Failed to remove member. Please try again.');
    }
  };

  return (
    <>
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardTitle className="flex items-center">
              <Users className="h-5 w-5 mr-2" />
              Team Members
            </CardTitle>
            <Button
              variant="outline"
              size="sm"
              onClick={() => setShowAddModal(true)}
              disabled={removeMember.isPending}
            >
              Add Member
            </Button>
          </div>
        </CardHeader>
        <CardContent>
          {project.projectMembers && project.projectMembers.length > 0 ? (
            <div className="space-y-2">
              {project.projectMembers.map((pm) => (
                <div
                  key={`${pm.projectId}-${pm.employeeId}`}
                  className="p-3 border rounded-lg flex items-start justify-between group"
                >
                  <div className="flex-1">
                    <p className="font-medium">
                      {pm.employee?.firstName} {pm.employee?.lastName}
                    </p>
                    <p className="text-sm text-muted-foreground">Role: {pm.role || 'Member'}</p>
                    <p className="text-xs text-muted-foreground">
                      Joined: {format(new Date(pm.joinedAt), 'MMM dd, yyyy')}
                    </p>
                  </div>
                  <button
                    onClick={() => handleRemove(pm.employeeId)}
                    disabled={removeMember.isPending}
                    className="opacity-0 group-hover:opacity-100 transition-opacity text-muted-foreground hover:text-red-600 p-1"
                    title="Remove member"
                  >
                    <X className="h-4 w-4" />
                  </button>
                </div>
              ))}
            </div>
          ) : (
            <div className="text-center py-8">
              <p className="text-muted-foreground mb-4">No team members assigned</p>
              <Button variant="outline" size="sm" onClick={() => setShowAddModal(true)}>
                Add First Member
              </Button>
            </div>
          )}
        </CardContent>
      </Card>

      {showAddModal && (
        <AddProjectMemberModal
          projectId={project.id}
          existingMemberIds={existingMemberIds}
          onClose={() => setShowAddModal(false)}
        />
      )}
    </>
  );
}
