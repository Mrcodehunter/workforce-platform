import { useDashboard } from '../hooks/useDashboard';
import { Loading } from '../components/common/Loading';
import { Error } from '../components/common/Error';
import { Card, CardContent, CardHeader, CardTitle } from '../components/common/Card';
import { Users, FolderKanban, CheckCircle2, Clock } from 'lucide-react';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, PieChart, Pie, Cell } from 'recharts';

const COLORS = ['#3b82f6', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6'];

export function Dashboard() {
  const { data, isLoading, error, refetch } = useDashboard();

  if (isLoading) return <Loading />;
  if (error) return <Error message="Failed to load dashboard data" onRetry={refetch} />;
  if (!data) return null;

  // Safely handle potentially undefined arrays
  const departmentData = (data.departmentHeadcount || []).map((d) => ({
    name: d.departmentName,
    count: d.count,
  }));

  const leaveTypeData = Object.entries(data.leaveStatistics?.byType || {}).map(([type, count]) => ({
    name: type,
    value: count,
  }));

  const projectProgress = data.projectProgress || [];

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Dashboard</h1>
        <p className="text-muted-foreground">Overview of your workforce management system</p>
      </div>

      {/* Stats Grid */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Employees</CardTitle>
            <Users className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{data.totalEmployees || 0}</div>
            <p className="text-xs text-muted-foreground">
              {data.activeEmployees || 0} active
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Active Projects</CardTitle>
            <FolderKanban className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{data.activeProjects || 0}</div>
            <p className="text-xs text-muted-foreground">
              {data.totalProjects || 0} total projects
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Tasks Completed</CardTitle>
            <CheckCircle2 className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{data.completedTasks || 0}</div>
            <p className="text-xs text-muted-foreground">
              {data.totalTasks || 0} total tasks
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Pending Leaves</CardTitle>
            <Clock className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{data.pendingLeaveRequests || 0}</div>
            <p className="text-xs text-muted-foreground">
              {data.leaveStatistics?.totalRequests || 0} total requests
            </p>
          </CardContent>
        </Card>
      </div>

      {/* Charts */}
      <div className="grid gap-4 md:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Department Headcount</CardTitle>
          </CardHeader>
          <CardContent>
            {departmentData.length === 0 ? (
              <p className="text-muted-foreground text-center py-8">No department data available</p>
            ) : (
              <ResponsiveContainer width="100%" height={300}>
                <BarChart data={departmentData}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="name" />
                  <YAxis />
                  <Tooltip />
                  <Bar dataKey="count" fill="#3b82f6" />
                </BarChart>
              </ResponsiveContainer>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Leave Requests by Type</CardTitle>
          </CardHeader>
          <CardContent>
            {leaveTypeData.length === 0 ? (
              <p className="text-muted-foreground text-center py-8">No leave request data available</p>
            ) : (
              <ResponsiveContainer width="100%" height={300}>
                <PieChart>
                  <Pie
                    data={leaveTypeData}
                    cx="50%"
                    cy="50%"
                    labelLine={false}
                    label={({ name, percent }) => `${name} ${(percent * 100).toFixed(0)}%`}
                    outerRadius={80}
                    fill="#8884d8"
                    dataKey="value"
                  >
                    {leaveTypeData.map((_, index) => (
                      <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                    ))}
                  </Pie>
                  <Tooltip />
                </PieChart>
              </ResponsiveContainer>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Project Progress */}
      <Card>
        <CardHeader>
          <CardTitle>Project Progress</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            {projectProgress.length === 0 ? (
              <p className="text-muted-foreground">No project progress data available</p>
            ) : (
              projectProgress.map((project) => (
              <div key={project.projectId} className="space-y-2">
                <div className="flex justify-between text-sm">
                  <span className="font-medium">{project.projectName}</span>
                  <span className="text-muted-foreground">
                    {project.completedTasks}/{project.totalTasks} tasks
                  </span>
                </div>
                <div className="w-full bg-secondary rounded-full h-2">
                  <div
                    className="bg-primary h-2 rounded-full transition-all"
                    style={{ width: `${project.progressPercentage}%` }}
                  />
                </div>
              </div>
              ))
            )}
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
