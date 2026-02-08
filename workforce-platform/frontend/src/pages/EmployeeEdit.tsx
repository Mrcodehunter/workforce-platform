import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useEmployee, useUpdateEmployee } from '../hooks/useEmployees';
import { useQuery } from '@tanstack/react-query';
import { departmentsApi, designationsApi } from '../api';
import { Loading } from '../components/common/Loading';
import { Error } from '../components/common/Error';
import { Button } from '../components/common/Button';
import { Input } from '../components/common/Input';
import { Card, CardContent, CardHeader, CardTitle } from '../components/common/Card';
import { ArrowLeft, X } from 'lucide-react';
import type { Employee } from '../types';

export function EmployeeEdit() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const updateEmployee = useUpdateEmployee();

  const { data: employee, isLoading, error, refetch } = useEmployee(id!);

  const { data: departments, isLoading: deptsLoading } = useQuery({
    queryKey: ['departments'],
    queryFn: departmentsApi.getAll,
  });

  const { data: designations, isLoading: desigsLoading } = useQuery({
    queryKey: ['designations'],
    queryFn: designationsApi.getAll,
  });

  const [formData, setFormData] = useState<Partial<Employee>>({});
  const [skillInput, setSkillInput] = useState('');
  const [errors, setErrors] = useState<Record<string, string>>({});

  useEffect(() => {
    if (employee) {
      setFormData({
        firstName: employee.firstName,
        lastName: employee.lastName,
        email: employee.email,
        phone: employee.phone,
        departmentId: employee.departmentId,
        designationId: employee.designationId,
        salary: employee.salary,
        joiningDate: employee.joiningDate.split('T')[0],
        address: employee.address,
        city: employee.city,
        country: employee.country,
        skills: employee.skills || [],
        isActive: employee.isActive,
      });
    }
  }, [employee]);

  const validate = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (!formData.firstName?.trim()) newErrors.firstName = 'First name is required';
    if (!formData.lastName?.trim()) newErrors.lastName = 'Last name is required';
    if (!formData.email?.trim()) {
      newErrors.email = 'Email is required';
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.email)) {
      newErrors.email = 'Invalid email format';
    }
    if (!formData.departmentId) newErrors.departmentId = 'Department is required';
    if (!formData.designationId) newErrors.designationId = 'Designation is required';
    if (!formData.salary || formData.salary <= 0) newErrors.salary = 'Salary must be greater than 0';
    if (!formData.joiningDate) newErrors.joiningDate = 'Joining date is required';

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validate() || !id) return;

    try {
      // Prepare data for API - ensure proper types
      const employeeData: Partial<Employee> = {
        firstName: formData.firstName,
        lastName: formData.lastName,
        email: formData.email,
        phone: formData.phone || undefined,
        departmentId: formData.departmentId as string,
        designationId: formData.designationId as string,
        salary: formData.salary || 0,
        joiningDate: formData.joiningDate ? new Date(formData.joiningDate).toISOString() : employee?.joiningDate,
        address: formData.address || undefined,
        city: formData.city || undefined,
        country: formData.country || undefined,
        skills: formData.skills || [],
        isActive: formData.isActive ?? true,
      };

      await updateEmployee.mutateAsync({ id, employee: employeeData });
      navigate(`/employees/${id}`);
    } catch (error) {
      console.error('Error updating employee:', error);
    }
  };

  const handleAddSkill = () => {
    if (skillInput.trim() && !formData.skills?.includes(skillInput.trim())) {
      setFormData({
        ...formData,
        skills: [...(formData.skills || []), skillInput.trim()],
      });
      setSkillInput('');
    }
  };

  const handleRemoveSkill = (skill: string) => {
    setFormData({
      ...formData,
      skills: formData.skills?.filter((s) => s !== skill) || [],
    });
  };

  if (isLoading || deptsLoading || desigsLoading) return <Loading />;
  if (error) return <Error message="Failed to load employee" onRetry={refetch} />;
  if (!employee) return null;

  return (
    <div className="space-y-6">
      <div className="flex items-center space-x-4">
        <Button variant="outline" size="icon" onClick={() => navigate(`/employees/${id}`)}>
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <div>
          <h1 className="text-3xl font-bold">Edit Employee</h1>
          <p className="text-muted-foreground">Update employee information</p>
        </div>
      </div>

      <form onSubmit={handleSubmit}>
        <Card>
          <CardHeader>
            <CardTitle>Employee Information</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid gap-4 md:grid-cols-2">
              <div>
                <label className="text-sm font-medium mb-2 block">
                  First Name <span className="text-red-500">*</span>
                </label>
                <Input
                  value={formData.firstName || ''}
                  onChange={(e) => setFormData({ ...formData, firstName: e.target.value })}
                  placeholder="John"
                  className={errors.firstName ? 'border-red-500' : ''}
                />
                {errors.firstName && <p className="text-sm text-red-500 mt-1">{errors.firstName}</p>}
              </div>

              <div>
                <label className="text-sm font-medium mb-2 block">
                  Last Name <span className="text-red-500">*</span>
                </label>
                <Input
                  value={formData.lastName || ''}
                  onChange={(e) => setFormData({ ...formData, lastName: e.target.value })}
                  placeholder="Doe"
                  className={errors.lastName ? 'border-red-500' : ''}
                />
                {errors.lastName && <p className="text-sm text-red-500 mt-1">{errors.lastName}</p>}
              </div>

              <div>
                <label className="text-sm font-medium mb-2 block">
                  Email <span className="text-red-500">*</span>
                </label>
                <Input
                  type="email"
                  value={formData.email || ''}
                  onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                  placeholder="john.doe@example.com"
                  className={errors.email ? 'border-red-500' : ''}
                />
                {errors.email && <p className="text-sm text-red-500 mt-1">{errors.email}</p>}
              </div>

              <div>
                <label className="text-sm font-medium mb-2 block">Phone</label>
                <Input
                  type="tel"
                  value={formData.phone || ''}
                  onChange={(e) => setFormData({ ...formData, phone: e.target.value })}
                  placeholder="+1 234 567 8900"
                />
              </div>

              <div>
                <label className="text-sm font-medium mb-2 block">
                  Department <span className="text-red-500">*</span>
                </label>
                <select
                  value={formData.departmentId || ''}
                  onChange={(e) => setFormData({ ...formData, departmentId: e.target.value })}
                  className={`flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ${
                    errors.departmentId ? 'border-red-500' : ''
                  }`}
                >
                  <option value="">Select Department</option>
                  {departments?.map((dept) => (
                    <option key={dept.id} value={dept.id}>
                      {dept.name}
                    </option>
                  ))}
                </select>
                {errors.departmentId && (
                  <p className="text-sm text-red-500 mt-1">{errors.departmentId}</p>
                )}
              </div>

              <div>
                <label className="text-sm font-medium mb-2 block">
                  Designation <span className="text-red-500">*</span>
                </label>
                <select
                  value={formData.designationId || ''}
                  onChange={(e) => setFormData({ ...formData, designationId: e.target.value })}
                  className={`flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ${
                    errors.designationId ? 'border-red-500' : ''
                  }`}
                >
                  <option value="">Select Designation</option>
                  {designations?.map((desig) => (
                    <option key={desig.id} value={desig.id}>
                      {desig.title}
                    </option>
                  ))}
                </select>
                {errors.designationId && (
                  <p className="text-sm text-red-500 mt-1">{errors.designationId}</p>
                )}
              </div>

              <div>
                <label className="text-sm font-medium mb-2 block">
                  Salary <span className="text-red-500">*</span>
                </label>
                <Input
                  type="number"
                  step="0.01"
                  min="0"
                  value={formData.salary || ''}
                  onChange={(e) =>
                    setFormData({ ...formData, salary: parseFloat(e.target.value) || 0 })
                  }
                  placeholder="50000"
                  className={errors.salary ? 'border-red-500' : ''}
                />
                {errors.salary && <p className="text-sm text-red-500 mt-1">{errors.salary}</p>}
              </div>

              <div>
                <label className="text-sm font-medium mb-2 block">
                  Joining Date <span className="text-red-500">*</span>
                </label>
                <Input
                  type="date"
                  value={formData.joiningDate || ''}
                  onChange={(e) => setFormData({ ...formData, joiningDate: e.target.value })}
                  className={errors.joiningDate ? 'border-red-500' : ''}
                />
                {errors.joiningDate && <p className="text-sm text-red-500 mt-1">{errors.joiningDate}</p>}
              </div>

              <div>
                <label className="text-sm font-medium mb-2 block">Address</label>
                <Input
                  value={formData.address || ''}
                  onChange={(e) => setFormData({ ...formData, address: e.target.value })}
                  placeholder="123 Main St"
                />
              </div>

              <div>
                <label className="text-sm font-medium mb-2 block">City</label>
                <Input
                  value={formData.city || ''}
                  onChange={(e) => setFormData({ ...formData, city: e.target.value })}
                  placeholder="New York"
                />
              </div>

              <div>
                <label className="text-sm font-medium mb-2 block">Country</label>
                <Input
                  value={formData.country || ''}
                  onChange={(e) => setFormData({ ...formData, country: e.target.value })}
                  placeholder="USA"
                />
              </div>
            </div>

            <div>
              <label className="text-sm font-medium mb-2 block">Skills</label>
              <div className="flex gap-2 mb-2">
                <Input
                  value={skillInput}
                  onChange={(e) => setSkillInput(e.target.value)}
                  onKeyPress={(e) => {
                    if (e.key === 'Enter') {
                      e.preventDefault();
                      handleAddSkill();
                    }
                  }}
                  placeholder="Add a skill and press Enter"
                />
                <Button type="button" onClick={handleAddSkill}>
                  Add
                </Button>
              </div>
              <div className="flex flex-wrap gap-2">
                {formData.skills?.map((skill) => (
                  <span
                    key={skill}
                    className="px-3 py-1 bg-secondary text-secondary-foreground rounded-md text-sm flex items-center gap-2"
                  >
                    {skill}
                    <button
                      type="button"
                      onClick={() => handleRemoveSkill(skill)}
                      className="hover:text-destructive"
                    >
                      <X className="h-3 w-3" />
                    </button>
                  </span>
                ))}
              </div>
            </div>

            <div className="flex items-center space-x-2">
              <input
                type="checkbox"
                id="isActive"
                checked={formData.isActive || false}
                onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
                className="h-4 w-4"
              />
              <label htmlFor="isActive" className="text-sm font-medium">
                Active Employee
              </label>
            </div>
          </CardContent>
        </Card>

        <div className="flex justify-end space-x-4 mt-6">
          <Button type="button" variant="outline" onClick={() => navigate(`/employees/${id}`)}>
            Cancel
          </Button>
          <Button type="submit" disabled={updateEmployee.isPending}>
            {updateEmployee.isPending ? 'Updating...' : 'Update Employee'}
          </Button>
        </div>
      </form>

      {updateEmployee.isError && (
        <Error message="Failed to update employee. Please try again." />
      )}
    </div>
  );
}
