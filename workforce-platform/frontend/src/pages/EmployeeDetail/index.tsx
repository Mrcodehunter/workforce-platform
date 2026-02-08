import { useParams, Link, useNavigate } from 'react-router-dom';
import { useEmployee, useDeleteEmployee } from '../../hooks/useEmployees';
import { Loading } from '../../components/common/Loading';
import { Error } from '../../components/common/Error';
import { Button } from '../../components/common/Button';
import { EntityAuditLog } from '../../components/audit/EntityAuditLog';
import { ArrowLeft, Edit, Trash2 } from 'lucide-react';
import { EmployeeProfile } from './components/EmployeeProfile';
import { EmployeeProjects } from './components/EmployeeProjects';
import { EmployeeLeaveRequests } from './components/EmployeeLeaveRequests';

export function EmployeeDetail() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const deleteEmployee = useDeleteEmployee();

  const { data: employee, isLoading, error, refetch } = useEmployee(id!);

  const handleDelete = async () => {
    if (
      !id ||
      !window.confirm('Are you sure you want to delete this employee? This action cannot be undone.')
    ) {
      return;
    }

    try {
      await deleteEmployee.mutateAsync(id);
      navigate('/employees');
    } catch (error) {
      console.error('Error deleting employee:', error);
      alert('Failed to delete employee. Please try again.');
    }
  };

  if (isLoading) return <Loading />;
  if (error) return <Error message="Failed to load employee" onRetry={refetch} />;
  if (!employee) return null;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-4">
          <Link to="/employees">
            <Button variant="outline" size="icon">
              <ArrowLeft className="h-4 w-4" />
            </Button>
          </Link>
          <div>
            <h1 className="text-3xl font-bold">
              {employee.firstName} {employee.lastName}
            </h1>
            <p className="text-muted-foreground">{employee.email}</p>
          </div>
        </div>
        <div className="flex space-x-2">
          <Button variant="outline" onClick={() => navigate(`/employees/${id}/edit`)}>
            <Edit className="h-4 w-4 mr-2" />
            Edit
          </Button>
          <Button variant="destructive" onClick={handleDelete} disabled={deleteEmployee.isPending}>
            <Trash2 className="h-4 w-4 mr-2" />
            {deleteEmployee.isPending ? 'Deleting...' : 'Delete'}
          </Button>
        </div>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        <EmployeeProfile employee={employee} />
        <EmployeeProjects employee={employee} />
      </div>

      <EmployeeLeaveRequests employeeId={id!} />

      {id && <EntityAuditLog entityType="Employee" entityId={id} />}
    </div>
  );
}
