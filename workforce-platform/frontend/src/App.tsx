import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { Layout } from './components/layout/Layout';
import { Dashboard } from './pages/Dashboard';
import { EmployeeList } from './pages/EmployeeList';
import { EmployeeDetail } from './pages/EmployeeDetail';
import { EmployeeCreate } from './pages/EmployeeCreate';
import { EmployeeEdit } from './pages/EmployeeEdit';
import { ProjectList } from './pages/ProjectList';
import { ProjectDetail } from './pages/ProjectDetail';
import { ProjectCreate } from './pages/ProjectCreate';
import { ProjectEdit } from './pages/ProjectEdit';
import { LeaveRequestList } from './pages/LeaveRequestList';
import { LeaveRequestCreate } from './pages/LeaveRequestCreate';
import { AuditTrail } from './pages/AuditTrail';

function App() {
  return (
    <Router>
      <Layout>
        <Routes>
          <Route path="/" element={<Dashboard />} />
          <Route path="/employees" element={<EmployeeList />} />
          <Route path="/employees/new" element={<EmployeeCreate />} />
          <Route path="/employees/:id" element={<EmployeeDetail />} />
          <Route path="/employees/:id/edit" element={<EmployeeEdit />} />
          <Route path="/projects" element={<ProjectList />} />
          <Route path="/projects/new" element={<ProjectCreate />} />
          <Route path="/projects/:id" element={<ProjectDetail />} />
          <Route path="/projects/:id/edit" element={<ProjectEdit />} />
          <Route path="/leave-requests" element={<LeaveRequestList />} />
          <Route path="/leave-requests/new" element={<LeaveRequestCreate />} />
          <Route path="/audit" element={<AuditTrail />} />
        </Routes>
      </Layout>
    </Router>
  );
}

export default App;
