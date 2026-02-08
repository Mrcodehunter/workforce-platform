import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useEmployees } from '../hooks/useEmployees';
import { useQuery } from '@tanstack/react-query';
import { departmentsApi } from '../api';
import { Loading } from '../components/common/Loading';
import { Error } from '../components/common/Error';
import { EmptyState } from '../components/common/EmptyState';
import { Button } from '../components/common/Button';
import { Input } from '../components/common/Input';
import { Card, CardContent, CardHeader, CardTitle } from '../components/common/Card';
import { Users, Plus, Search } from 'lucide-react';
import type { Employee } from '../types';

export function EmployeeList() {
  const [searchTerm, setSearchTerm] = useState('');
  const [departmentFilter, setDepartmentFilter] = useState<string>('all');
  const [statusFilter, setStatusFilter] = useState<string>('all');
  const [sortBy, setSortBy] = useState<keyof Employee>('firstName');
  const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('asc');

  const { data: employees, isLoading, error, refetch } = useEmployees();
  const { data: departments } = useQuery({
    queryKey: ['departments'],
    queryFn: departmentsApi.getAll,
  });

  if (isLoading) return <Loading />;
  if (error) return <Error message="Failed to load employees" onRetry={refetch} />;

  // Filtering and sorting
  let filteredEmployees = employees || [];

  if (searchTerm) {
    filteredEmployees = filteredEmployees.filter(
      (emp) =>
        emp.firstName.toLowerCase().includes(searchTerm.toLowerCase()) ||
        emp.lastName.toLowerCase().includes(searchTerm.toLowerCase()) ||
        emp.email.toLowerCase().includes(searchTerm.toLowerCase())
    );
  }

  if (departmentFilter !== 'all') {
    filteredEmployees = filteredEmployees.filter((emp) => emp.departmentId === departmentFilter);
  }

  if (statusFilter !== 'all') {
    filteredEmployees = filteredEmployees.filter(
      (emp) => emp.isActive === (statusFilter === 'active')
    );
  }

  // Sorting
  filteredEmployees = [...filteredEmployees].sort((a, b) => {
    const aVal = a[sortBy] as string | number | undefined;
    const bVal = b[sortBy] as string | number | undefined;
    if (aVal === undefined || bVal === undefined) return 0;
    if (aVal < bVal) return sortOrder === 'asc' ? -1 : 1;
    if (aVal > bVal) return sortOrder === 'asc' ? 1 : -1;
    return 0;
  });

  const handleSort = (column: keyof Employee) => {
    if (sortBy === column) {
      setSortOrder(sortOrder === 'asc' ? 'desc' : 'asc');
    } else {
      setSortBy(column);
      setSortOrder('asc');
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold">Employees</h1>
          <p className="text-muted-foreground">Manage employee records and information</p>
        </div>
        <Link to="/employees/new">
          <Button>
            <Plus className="h-4 w-4 mr-2" />
            Add Employee
          </Button>
        </Link>
      </div>

      {/* Filters */}
      <Card>
        <CardContent className="pt-6">
          <div className="grid gap-4 md:grid-cols-4">
            <div className="relative">
              <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
              <Input
                placeholder="Search employees..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-10"
              />
            </div>
            <select
              value={departmentFilter}
              onChange={(e) => setDepartmentFilter(e.target.value)}
              className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
            >
              <option value="all">All Departments</option>
              {departments?.map((dept) => (
                <option key={dept.id} value={dept.id}>
                  {dept.name}
                </option>
              ))}
            </select>
            <select
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value)}
              className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
            >
              <option value="all">All Status</option>
              <option value="active">Active</option>
              <option value="inactive">Inactive</option>
            </select>
          </div>
        </CardContent>
      </Card>

      {/* Table */}
      {filteredEmployees.length === 0 ? (
        <EmptyState
          icon={Users}
          title="No employees found"
          description="Try adjusting your search or filters"
        />
      ) : (
        <Card>
          <CardHeader>
            <CardTitle>Employee List ({filteredEmployees.length})</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead>
                  <tr className="border-b">
                    <th
                      className="text-left p-2 cursor-pointer hover:bg-accent"
                      onClick={() => handleSort('firstName')}
                    >
                      Name {sortBy === 'firstName' && (sortOrder === 'asc' ? '↑' : '↓')}
                    </th>
                    <th
                      className="text-left p-2 cursor-pointer hover:bg-accent"
                      onClick={() => handleSort('email')}
                    >
                      Email {sortBy === 'email' && (sortOrder === 'asc' ? '↑' : '↓')}
                    </th>
                    <th className="text-left p-2">Department</th>
                    <th className="text-left p-2">Status</th>
                    <th className="text-left p-2">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {filteredEmployees.map((employee) => (
                    <tr key={employee.id} className="border-b hover:bg-accent/50">
                      <td className="p-2">
                        {employee.firstName} {employee.lastName}
                      </td>
                      <td className="p-2">{employee.email}</td>
                      <td className="p-2">
                        {employee.department?.name || departments?.find((d) => d.id === employee.departmentId)?.name || 'N/A'}
                      </td>
                      <td className="p-2">
                        <span
                          className={`px-2 py-1 rounded-full text-xs ${
                            employee.isActive
                              ? 'bg-green-100 text-green-800'
                              : 'bg-gray-100 text-gray-800'
                          }`}
                        >
                          {employee.isActive ? 'Active' : 'Inactive'}
                        </span>
                      </td>
                      <td className="p-2">
                        <Link to={`/employees/${employee.id}`}>
                          <Button variant="outline" size="sm">
                            View
                          </Button>
                        </Link>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
