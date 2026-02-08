import { Link, useLocation } from 'react-router-dom';
import { Users, FolderKanban, Calendar, BarChart3, Activity } from 'lucide-react';
import { cn } from '../../utils/cn';

const navigation = [
  { name: 'Dashboard', href: '/', icon: BarChart3 },
  { name: 'Employees', href: '/employees', icon: Users },
  { name: 'Projects', href: '/projects', icon: FolderKanban },
  { name: 'Leave Requests', href: '/leave-requests', icon: Calendar },
  { name: 'Audit Trail', href: '/audit', icon: Activity },
];

export function Navbar() {
  const location = useLocation();

  return (
    <nav className="border-b bg-background">
      <div className="container mx-auto px-4">
        <div className="flex h-16 items-center justify-between">
          <div className="flex items-center space-x-8">
            <Link to="/" className="text-xl font-bold">
              Workforce Platform
            </Link>
            <div className="flex space-x-1">
              {navigation.map((item) => {
                const Icon = item.icon;
                const isActive = location.pathname === item.href || 
                  (item.href !== '/' && location.pathname.startsWith(item.href));
                return (
                  <Link
                    key={item.name}
                    to={item.href}
                    className={cn(
                      'flex items-center space-x-2 px-3 py-2 rounded-md text-sm font-medium transition-colors',
                      isActive
                        ? 'bg-primary text-primary-foreground'
                        : 'text-muted-foreground hover:bg-accent hover:text-accent-foreground'
                    )}
                  >
                    <Icon className="h-4 w-4" />
                    <span>{item.name}</span>
                  </Link>
                );
              })}
            </div>
          </div>
        </div>
      </div>
    </nav>
  );
}
