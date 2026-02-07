import { BrowserRouter as Router, Routes, Route } from 'react-router-dom'

function App() {
  return (
    <Router>
      <div className="min-h-screen bg-background">
        <header className="border-b">
          <div className="container mx-auto px-4 py-4">
            <h1 className="text-2xl font-bold">Workforce Management Platform</h1>
          </div>
        </header>
        
        <main className="container mx-auto px-4 py-8">
          <Routes>
            <Route path="/" element={<HomePage />} />
          </Routes>
        </main>
      </div>
    </Router>
  )
}

function HomePage() {
  return (
    <div className="space-y-6">
      <div className="rounded-lg border p-6">
        <h2 className="text-xl font-semibold mb-4">Welcome to Workforce Management</h2>
        <p className="text-muted-foreground">
          The system is running! Start building your features.
        </p>
      </div>
      
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <FeatureCard 
          title="Employees" 
          description="Manage employee records and information"
          icon="👥"
        />
        <FeatureCard 
          title="Projects" 
          description="Track projects and team assignments"
          icon="📊"
        />
        <FeatureCard 
          title="Tasks" 
          description="Manage tasks and workflows"
          icon="✅"
        />
        <FeatureCard 
          title="Leave Requests" 
          description="Handle time-off requests and approvals"
          icon="📅"
        />
      </div>
    </div>
  )
}

interface FeatureCardProps {
  title: string
  description: string
  icon: string
}

function FeatureCard({ title, description, icon }: FeatureCardProps) {
  return (
    <div className="rounded-lg border p-6 hover:shadow-lg transition-shadow">
      <div className="text-4xl mb-4">{icon}</div>
      <h3 className="font-semibold mb-2">{title}</h3>
      <p className="text-sm text-muted-foreground">{description}</p>
    </div>
  )
}

export default App
