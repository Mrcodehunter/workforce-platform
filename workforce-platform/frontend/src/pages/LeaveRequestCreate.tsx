import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useCreateLeaveRequest } from '../hooks/useLeaveRequests';
import { useEmployees } from '../hooks/useEmployees';
import { Loading } from '../components/common/Loading';
import { Button } from '../components/common/Button';
import { Input } from '../components/common/Input';
import { Card, CardContent, CardHeader, CardTitle } from '../components/common/Card';
import { ArrowLeft, Calendar } from 'lucide-react';
import { Link } from 'react-router-dom';
import type { LeaveType } from '../types';

export function LeaveRequestCreate() {
  const navigate = useNavigate();
  const createLeaveRequest = useCreateLeaveRequest();
  const { data: employees, isLoading: employeesLoading } = useEmployees();

  const [formData, setFormData] = useState({
    employeeId: '',
    employeeName: '',
    leaveType: '' as LeaveType | '',
    startDate: '',
    endDate: '',
    reason: '',
  });

  const [errors, setErrors] = useState<Record<string, string>>({});

  const validate = () => {
    const newErrors: Record<string, string> = {};

    if (!formData.employeeId) {
      newErrors.employeeId = 'Employee is required';
    }
    if (!formData.leaveType) {
      newErrors.leaveType = 'Leave type is required';
    }
    if (!formData.startDate) {
      newErrors.startDate = 'Start date is required';
    }
    if (!formData.endDate) {
      newErrors.endDate = 'End date is required';
    }
    if (formData.startDate && formData.endDate) {
      const start = new Date(formData.startDate);
      const end = new Date(formData.endDate);
      if (end < start) {
        newErrors.endDate = 'End date must be after start date';
      }
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validate()) {
      return;
    }

    try {
      await createLeaveRequest.mutateAsync({
        employeeId: formData.employeeId,
        employeeName: formData.employeeName,
        leaveType: formData.leaveType as LeaveType,
        startDate: formData.startDate,
        endDate: formData.endDate,
        reason: formData.reason || undefined,
        status: 'Pending',
        approvalHistory: [],
      });

      navigate('/leave-requests');
    } catch (error) {
      console.error('Error creating leave request:', error);
      alert('Failed to create leave request. Please try again.');
    }
  };

  const handleEmployeeChange = (employeeId: string) => {
    const employee = employees?.find((emp) => emp.id === employeeId);
    setFormData({
      ...formData,
      employeeId,
      employeeName: employee ? `${employee.firstName} ${employee.lastName}` : '',
    });
    if (errors.employeeId) {
      setErrors({ ...errors, employeeId: '' });
    }
  };

  if (employeesLoading) return <Loading />;

  return (
    <div className="space-y-6">
      <div className="flex items-center space-x-4">
        <Link to="/leave-requests">
          <Button variant="outline" size="icon">
            <ArrowLeft className="h-4 w-4" />
          </Button>
        </Link>
        <div>
          <h1 className="text-3xl font-bold">Submit Leave Request</h1>
          <p className="text-muted-foreground">Request time off from work</p>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Leave Request Details</CardTitle>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-6">
            <div>
              <label htmlFor="employeeId" className="text-sm font-medium mb-2 block">
                Employee <span className="text-red-500">*</span>
              </label>
              <select
                id="employeeId"
                value={formData.employeeId}
                onChange={(e) => handleEmployeeChange(e.target.value)}
                className={`w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-ring ${
                  errors.employeeId ? 'border-red-500' : ''
                }`}
              >
                <option value="">Select an employee</option>
                {employees
                  ?.filter((emp) => emp.isActive)
                  .map((emp) => (
                    <option key={emp.id} value={emp.id}>
                      {emp.firstName} {emp.lastName} ({emp.email})
                    </option>
                  ))}
              </select>
              {errors.employeeId && (
                <p className="text-sm text-red-500 mt-1">{errors.employeeId}</p>
              )}
            </div>

            <div>
              <label htmlFor="leaveType" className="text-sm font-medium mb-2 block">
                Leave Type <span className="text-red-500">*</span>
              </label>
              <select
                id="leaveType"
                value={formData.leaveType}
                onChange={(e) => {
                  setFormData({ ...formData, leaveType: e.target.value as LeaveType });
                  if (errors.leaveType) {
                    setErrors({ ...errors, leaveType: '' });
                  }
                }}
                className={`w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-ring ${
                  errors.leaveType ? 'border-red-500' : ''
                }`}
              >
                <option value="">Select leave type</option>
                <option value="Sick">Sick</option>
                <option value="Casual">Casual</option>
                <option value="Annual">Annual</option>
                <option value="Unpaid">Unpaid</option>
              </select>
              {errors.leaveType && (
                <p className="text-sm text-red-500 mt-1">{errors.leaveType}</p>
              )}
            </div>

            <div className="grid gap-4 md:grid-cols-2">
              <div>
                <label htmlFor="startDate" className="text-sm font-medium mb-2 block">
                  Start Date <span className="text-red-500">*</span>
                </label>
                <Input
                  id="startDate"
                  type="date"
                  value={formData.startDate}
                  onChange={(e) => {
                    setFormData({ ...formData, startDate: e.target.value });
                    if (errors.startDate) {
                      setErrors({ ...errors, startDate: '' });
                    }
                  }}
                  className={errors.startDate ? 'border-red-500' : ''}
                />
                {errors.startDate && (
                  <p className="text-sm text-red-500 mt-1">{errors.startDate}</p>
                )}
              </div>

              <div>
                <label htmlFor="endDate" className="text-sm font-medium mb-2 block">
                  End Date <span className="text-red-500">*</span>
                </label>
                <Input
                  id="endDate"
                  type="date"
                  value={formData.endDate}
                  min={formData.startDate}
                  onChange={(e) => {
                    setFormData({ ...formData, endDate: e.target.value });
                    if (errors.endDate) {
                      setErrors({ ...errors, endDate: '' });
                    }
                  }}
                  className={errors.endDate ? 'border-red-500' : ''}
                />
                {errors.endDate && (
                  <p className="text-sm text-red-500 mt-1">{errors.endDate}</p>
                )}
              </div>
            </div>

            {formData.startDate && formData.endDate && (
              <div className="flex items-center text-sm text-muted-foreground">
                <Calendar className="h-4 w-4 mr-2" />
                Duration:{' '}
                {Math.ceil(
                  (new Date(formData.endDate).getTime() -
                    new Date(formData.startDate).getTime()) /
                    (1000 * 60 * 60 * 24)
                ) + 1}{' '}
                day(s)
              </div>
            )}

            <div>
              <label htmlFor="reason" className="text-sm font-medium mb-2 block">
                Reason (Optional)
              </label>
              <textarea
                id="reason"
                value={formData.reason}
                onChange={(e) => setFormData({ ...formData, reason: e.target.value })}
                rows={4}
                className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-ring"
                placeholder="Provide a reason for your leave request..."
              />
            </div>

            <div className="flex justify-end space-x-4">
              <Link to="/leave-requests">
                <Button type="button" variant="outline">
                  Cancel
                </Button>
              </Link>
              <Button type="submit" disabled={createLeaveRequest.isPending}>
                {createLeaveRequest.isPending ? 'Submitting...' : 'Submit Request'}
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
