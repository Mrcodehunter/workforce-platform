import { useParams, Link, useNavigate } from 'react-router-dom';
import { useEmployee, useDeleteEmployee } from '../hooks/useEmployees';
import { useQuery } from '@tanstack/react-query';
import { leaveRequestsApi } from '../api';
import { Loading } from '../components/common/Loading';
import { Error } from '../components/common/Error';
import { Card, CardContent, CardHeader, CardTitle } from '../components/common/Card';
import { Button } from '../components/common/Button';
import { ArrowLeft, Edit, Trash2, Calendar, Activity } from 'lucide-react';
import { format } from 'date-fns';
import type { LeaveRequest } from '../types';

export function EmployeeDetail() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const deleteEmployee = useDeleteEmployee();

  const { data: employee, isLoading, error, refetch } = useEmployee(id!);
  const { data: leaveRequests } = useQuery({
    queryKey: ['leaveRequests', id],
    queryFn: leaveRequestsApi.getAll,
    enabled: !!id,
    select: (data) => data.filter((lr: LeaveRequest) => lr.employeeId === id),
  });

  const handleDelete = async () => {
    if (!id || !window.confirm('Are you sure you want to delete this employee? This action cannot be undone.')) {
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
        {/* Profile Information */}
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
              <p>
                {[employee.city, employee.country].filter(Boolean).join(', ') || 'N/A'}
              </p>
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

        {/* Assigned Projects */}
        <Card>
          <CardHeader>
            <CardTitle>Assigned Projects</CardTitle>
          </CardHeader>
          <CardContent>
            {employee.projectMembers && employee.projectMembers.length > 0 ? (
              <div className="space-y-2">
                {employee.projectMembers.map((pm) => (
                  <div key={pm.projectId} className="p-3 border rounded-lg">
                    <p className="font-medium">{pm.project?.name || 'Unknown Project'}</p>
                    <p className="text-sm text-muted-foreground">Role: {pm.role || 'Member'}</p>
                  </div>
                ))}
              </div>
            ) : (
              <p className="text-muted-foreground">No projects assigned</p>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Leave Request History */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center">
            <Calendar className="h-5 w-5 mr-2" />
            Leave Request History
          </CardTitle>
        </CardHeader>
        <CardContent>
          {leaveRequests && leaveRequests.length > 0 ? (
            <div className="space-y-4">
              {leaveRequests.map((lr: LeaveRequest) => (
                <div key={lr.id} className="p-4 border rounded-lg">
                  <div className="flex justify-between items-start">
                    <div>
                      <p className="font-medium">{lr.leaveType} Leave</p>
                      <p className="text-sm text-muted-foreground">
                        {format(new Date(lr.startDate), 'PPP')} -{' '}
                        {format(new Date(lr.endDate), 'PPP')}
                      </p>
                      {lr.reason && <p className="text-sm mt-1">{lr.reason}</p>}
                    </div>
                    <span
                      className={`px-2 py-1 rounded-full text-xs ${
                        lr.status === 'Approved'
                          ? 'bg-green-100 text-green-800'
                          : lr.status === 'Rejected'
                          ? 'bg-red-100 text-red-800'
                          : 'bg-yellow-100 text-yellow-800'
                      }`}
                    >
                      {lr.status}
                    </span>
                  </div>
                  {lr.approvalHistory && Array.isArray(lr.approvalHistory) && lr.approvalHistory.length > 0 && (
                    <div className="mt-3 pt-3 border-t">
                      <p className="text-sm font-medium mb-2">Approval History</p>
                      <div className="space-y-1">
                        {lr.approvalHistory.map((entry, idx) => (
                          <div key={idx} className="text-sm text-muted-foreground">
                            <span className="font-medium">{entry.changedBy}</span> - {entry.status}{' '}
                            on {format(new Date(entry.changedAt), 'PPP')}
                            {entry.comments && <span>: {entry.comments}</span>}
                          </div>
                        ))}
                      </div>
                    </div>
                  )}
                </div>
              ))}
            </div>
          ) : (
            <p className="text-muted-foreground">No leave requests</p>
          )}
        </CardContent>
      </Card>

      {/* Audit Trail */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center">
            <Activity className="h-5 w-5 mr-2" />
            Audit Trail
          </CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-muted-foreground">
            Audit trail for this employee will be displayed here. (Requires audit log API endpoint)
          </p>
        </CardContent>
      </Card>
    </div>
  );
}
