import { Card, CardContent, CardHeader, CardTitle } from '../../../components/common/Card';
import { format } from 'date-fns';
import type { Employee } from '../../../types';

interface EmployeeProfileProps {
  employee: Employee;
}

export function EmployeeProfile({ employee }: EmployeeProfileProps) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Profile Information</CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        <div>
          <label className="text-sm font-medium text-muted-foreground">Department</label>
          <p>{employee.department?.name || 'N/A'}</p>
        </div>
        <div>
          <label className="text-sm font-medium text-muted-foreground">Designation</label>
          <p>{employee.designation?.title || 'N/A'}</p>
        </div>
        <div>
          <label className="text-sm font-medium text-muted-foreground">Salary</label>
          <p>${employee.salary.toLocaleString()}</p>
        </div>
        <div>
          <label className="text-sm font-medium text-muted-foreground">Joining Date</label>
          <p>{format(new Date(employee.joiningDate), 'PPP')}</p>
        </div>
        <div>
          <label className="text-sm font-medium text-muted-foreground">Phone</label>
          <p>{employee.phone || 'N/A'}</p>
        </div>
        <div>
          <label className="text-sm font-medium text-muted-foreground">Location</label>
          <p>{[employee.city, employee.country].filter(Boolean).join(', ') || 'N/A'}</p>
        </div>
        <div>
          <label className="text-sm font-medium text-muted-foreground">Skills</label>
          <div className="flex flex-wrap gap-2 mt-2">
            {employee.skills && employee.skills.length > 0 ? (
              employee.skills.map((skill) => (
                <span
                  key={skill}
                  className="px-2 py-1 bg-secondary text-secondary-foreground rounded-md text-sm"
                >
                  {skill}
                </span>
              ))
            ) : (
              <span className="text-muted-foreground text-sm">No skills listed</span>
            )}
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
