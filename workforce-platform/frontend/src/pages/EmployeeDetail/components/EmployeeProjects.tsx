import { Card, CardContent, CardHeader, CardTitle } from '../../../components/common/Card';
import { Link } from 'react-router-dom';
import type { Employee } from '../../../types';

interface EmployeeProjectsProps {
  employee: Employee;
}

export function EmployeeProjects({ employee }: EmployeeProjectsProps) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Assigned Projects</CardTitle>
      </CardHeader>
      <CardContent>
        {employee.projectMembers && employee.projectMembers.length > 0 ? (
          <div className="space-y-2">
            {employee.projectMembers.map((pm) => (
              <Link
                key={pm.projectId}
                to={`/projects/${pm.projectId}`}
                className="block p-3 border rounded-lg hover:bg-accent transition-colors"
              >
                <p className="font-medium">{pm.project?.name || 'Unknown Project'}</p>
                <p className="text-sm text-muted-foreground">Role: {pm.role || 'Member'}</p>
              </Link>
            ))}
          </div>
        ) : (
          <p className="text-muted-foreground">No projects assigned</p>
        )}
      </CardContent>
    </Card>
  );
}
