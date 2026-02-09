# Workforce Management Platform

[![CI/CD Pipeline](https://github.com/YOUR_USERNAME/workforce-platform/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/YOUR_USERNAME/workforce-platform/actions/workflows/ci-cd.yml)

A distributed workforce management system built with .NET, React, TypeScript, PostgreSQL, MongoDB, and RabbitMQ. This platform handles employee management, project tracking, task management, and leave/time-off requests using an event-driven microservices architecture.

## 🏗️ System Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     Docker Compose                          │
│                                                             │
│  Frontend (React) ──HTTP──> API Server (.NET)              │
│                                  │                          │
│                           ┌──────┴──────┐                   │
│                           │             │                   │
│                      PostgreSQL     MongoDB                 │
│                           │             │                   │
│                           └──────┬──────┘                   │
│                                  │                          │
│                             RabbitMQ                        │
│                              │    │                         │
│                    ┌─────────┘    └─────────┐              │
│                    │                        │              │
│             Worker 1 (.NET)        Worker 2 (Node.js)      │
│            Audit Logger           Report Generator          │
└─────────────────────────────────────────────────────────────┘
```

## 🚀 Quick Start

### Prerequisites

- **Docker** (version 24+) and **Docker Compose** (version 2.20+)
- **Git**

That's it! Docker will handle all other dependencies.

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/YOUR_USERNAME/workforce-platform.git
   cd workforce-platform
   ```

2. **Create environment file**
   ```bash
   cp .env.example .env
   ```
   
   Optional: Edit `.env` to customize database passwords and ports

3. **Start the entire system**
   ```bash
   docker compose up --build
   ```

4. **Access the application**
   - **Frontend**: http://localhost:3000
   - **API**: http://localhost:5000
   - **API Documentation (Scalar)**: 
     - Primary URL: http://localhost:5000/scalar/v1
     - Root redirect: http://localhost:5000/ (redirects to Scalar)
     - OpenAPI JSON: http://localhost:5000/openapi/v1.json
   - **RabbitMQ Management**: http://localhost:15672 (guest/guest)

### First Time Setup

The system will automatically:
- Create database schemas
- Run migrations
- Seed initial data
- Start all services

**Note**: First startup may take 3-5 minutes while Docker builds all images.

## 📦 Project Structure

```
workforce-platform/
├── backend/                    # .NET Backend Services
│   ├── WorkforceAPI/          # REST API Server
│   │   ├── Controllers/       # API endpoints
│   │   ├── Services/          # Business logic layer
│   │   ├── Repositories/      # Data access layer
│   │   ├── Models/            # Domain entities
│   │   ├── DTOs/              # Data transfer objects
│   │   ├── Data/              # Database contexts
│   │   └── DependencyInjection.cs  # DI registration
│   ├── WorkerService.AuditLogger/  # Audit logging worker
│   │   ├── Services/
│   │   └── Models/
│   └── Workforce.Shared/      # Shared infrastructure library
│       ├── Cache/             # Redis cache implementation
│       ├── Configuration/     # Configuration options classes
│       ├── DependencyInjection/  # Centralized DI extensions
│       ├── EventPublisher/    # RabbitMQ publisher
│       └── Events/            # Event type definitions
├── frontend/                   # React Frontend
│   ├── src/
│   │   ├── api/               # API communication layer
│   │   │   ├── config/        # Axios configuration & interceptors
│   │   │   ├── endpoints/     # API endpoint definitions
│   │   │   └── constants.ts   # API constants
│   │   ├── components/        # React components
│   │   │   ├── common/        # Reusable UI components
│   │   │   └── layout/        # Layout components
│   │   ├── pages/             # Page components
│   │   ├── hooks/             # Custom React hooks
│   │   ├── types/             # TypeScript types
│   │   └── utils/             # Utility functions
│   └── public/
├── workers/                    # Background Workers
│   └── report-generator/      # Node.js report worker
│       └── src/
├── .github/
│   └── workflows/             # CI/CD pipelines
├── docker-compose.yml         # Docker orchestration
├── .env.example               # Environment template
└── README.md
```

## 🛠️ Technology Stack

### Backend
- **.NET 10.0** - Web API and Worker Services
- **Entity Framework Core** - PostgreSQL ORM
- **MongoDB Driver** - Document database client
- **RabbitMQ.Client** - Message broker integration
- **StackExchange.Redis** - Redis cache client
- **Serilog** - Structured logging
- **AutoMapper** - Object mapping
- **FluentValidation** - Input validation
- **Scalar** - Modern API documentation (replaces Swagger)

### Architecture Patterns
- **Dependency Injection** - Centralized DI configuration in `Workforce.Shared`
- **Options Pattern** - Environment-aware configuration (Development/Production)
- **Repository Pattern** - Clean separation of data access
- **Service Layer** - Business logic abstraction
- **Event-Driven Architecture** - RabbitMQ for async communication

### Frontend
- **React 18** - UI framework
- **TypeScript** - Type safety
- **Vite** - Build tool
- **React Router** - Client-side routing
- **Tailwind CSS** - Utility-first styling
- **Tailwind CSS Components** - Custom component library
- **Axios** - HTTP client
- **React Query** - Server state management
- **Recharts** - Data visualization

### Infrastructure
- **PostgreSQL 16** - Relational database
- **MongoDB 7** - Document database
- **RabbitMQ 3.12** - Message broker
- **Docker & Docker Compose** - Containerization
- **Nginx** - Frontend web server

### Worker Services
- **.NET BackgroundService** - Audit logging
- **Node.js** - Report generation

## 📊 Database Design

### PostgreSQL (Relational Data)
- **Employees** - Employee profiles and information
- **Departments** - Organizational departments
- **Designations** - Job titles and levels
- **Projects** - Project metadata
- **ProjectMembers** - Many-to-many: Projects ↔ Employees
- **Tasks** - Project tasks with assignments

**Why PostgreSQL?**
- Strong referential integrity for organizational hierarchy
- Complex joins between employees, projects, and tasks
- ACID compliance for critical business data
- Excellent support for indexes and query optimization

### MongoDB (Document Data)
- **LeaveRequests** - Leave requests with embedded approval history
- **AuditLogs** - Immutable audit trail of all system events
- **Reports** - Pre-computed dashboard summaries

**Why MongoDB?**
- Self-contained documents (leave request + full approval history)
- Flexible schema for audit logs (different event types)
- High write throughput for audit logging
- Natural fit for embedded arrays (approval workflow)

## 🏛️ Architecture & Design

### Dependency Injection Structure

The backend uses a centralized dependency injection approach with environment-aware configuration:

**Shared Infrastructure (`Workforce.Shared`)**
- `ServiceCollectionExtensions` - Centralized DI entry point
- `RedisExtensions` - Redis cache configuration with environment defaults
- `RabbitMqExtensions` - RabbitMQ publisher configuration
- Configuration options classes (`RedisOptions`, `RabbitMqOptions`) for type-safe settings

**Environment-Aware Configuration**
- **Development**: Uses `localhost` for all services (Redis, RabbitMQ, MongoDB)
- **Production**: Uses Docker service names (`redis`, `rabbitmq`, `mongodb`)
- Configuration files: `appsettings.json`, `appsettings.Development.json`, `appsettings.Production.json`

**Benefits:**
- Single source of truth for infrastructure configuration
- Easy to switch between local development and Docker environments
- Type-safe configuration with validation
- Graceful degradation when services are unavailable

### Design Decisions

**Why Centralized DI?**
- Reduces code duplication across projects (API and Workers)
- Ensures consistent configuration across services
- Makes it easier to add new services or change infrastructure
- Improves testability with mockable interfaces

**Why Options Pattern?**
- Type-safe configuration access
- Environment-specific defaults
- Validation at startup
- Easy to extend with new settings

## 🔄 Event-Driven Architecture

### Message Broker: RabbitMQ

**Event Types:**
- `employee.*` - Employee CRUD operations
- `project.*` - Project lifecycle events
- `task.*` - Task updates and status changes
- `leave.*` - Leave request workflow
- `department.*` - Department changes

### Workers

**Worker 1: Audit Logger (.NET)**
- Consumes all domain events from RabbitMQ
- Creates immutable audit log entries in MongoDB
- Idempotent processing (prevents duplicates)
- Retry logic with exponential backoff

**Worker 2: Report Generator (Node.js)**
- Scheduled execution (every hour)
- Aggregates data from PostgreSQL and MongoDB
- Generates dashboard summaries
- Stores pre-computed reports in MongoDB

## 🌐 API Endpoints

Base URL: `http://localhost:5000/api`

### Employees
- `GET /api/employees` - List all employees
- `GET /api/employees/{id}` - Get employee details
- `POST /api/employees` - Create employee
- `PUT /api/employees/{id}` - Update employee
- `DELETE /api/employees/{id}` - Delete employee

### Departments
- `GET /api/departments` - List all departments
- `GET /api/departments/{id}` - Get department details
- `POST /api/departments` - Create department
- `PUT /api/departments/{id}` - Update department
- `DELETE /api/departments/{id}` - Delete department

### Designations
- `GET /api/designations` - List all designations
- `GET /api/designations/{id}` - Get designation details
- `POST /api/designations` - Create designation

### Projects
- `GET /api/projects` - List all projects
- `GET /api/projects/{id}` - Get project details
- `POST /api/projects` - Create project
- `PUT /api/projects/{id}` - Update project
- `DELETE /api/projects/{id}` - Delete project

### Tasks
- `GET /api/tasks` - List all tasks
- `GET /api/tasks/{id}` - Get task details
- `POST /api/tasks` - Create task
- `PUT /api/tasks/{id}` - Update task
- `DELETE /api/tasks/{id}` - Delete task

### Leave Requests
- `GET /api/leaverequests` - List all leave requests
- `GET /api/leaverequests/{id}` - Get leave request details

### Dashboard
- `GET /api/dashboard/summary` - Get dashboard summary (aggregated data from both databases)

### Health Check
- `GET /health` - API health status

### API Documentation

**Scalar API Documentation** is available when running in Docker containers:

- **Primary URL**: http://localhost:5000/scalar/v1
- **Root Redirect**: http://localhost:5000/ (automatically redirects to Scalar)
- **OpenAPI JSON**: http://localhost:5000/openapi/v1.json

The Scalar documentation provides:
- Interactive API explorer with all endpoints
- Request/response schemas and examples
- Try-it-out functionality to test endpoints directly
- Modern, user-friendly interface

**Note**: Scalar is available in all environments (Development, Production) when running in Docker containers.

## 🧪 Development

### Running in Development Mode

**Backend (Hot Reload)**
```bash
cd backend/WorkforceAPI
# Set environment to Development (uses localhost for services)
export ASPNETCORE_ENVIRONMENT=Development  # Linux/Mac
# or
$env:ASPNETCORE_ENVIRONMENT="Development"  # Windows PowerShell
dotnet watch run
```

**Frontend (Hot Reload)**
```bash
cd frontend
npm install
npm run dev
```

**Workers**
```bash
# .NET Worker
cd backend/WorkerService.AuditLogger
export DOTNET_ENVIRONMENT=Development  # Linux/Mac
# or
$env:DOTNET_ENVIRONMENT="Development"  # Windows PowerShell
dotnet watch run

# Node.js Worker
cd workers/report-generator
npm install
npm run dev
```

**Note**: When running locally (not in Docker), ensure you have:
- PostgreSQL running on `localhost:5432`
- MongoDB running on `localhost:27017`
- RabbitMQ running on `localhost:5672`
- Redis running on `localhost:6379`

The application will automatically use `localhost` connections when `ASPNETCORE_ENVIRONMENT=Development`.

### Database Migrations

**Create new migration**
```bash
cd backend/WorkforceAPI
dotnet ef migrations add MigrationName
```

**Apply migrations**
```bash
dotnet ef database update
```

### Seed Data

Seed data is automatically loaded on first run. To reset:
```bash
docker compose down -v  # Remove volumes
docker compose up --build
```

## 🧩 Key Features

### ✅ Implemented
- Employee CRUD with pagination and filtering
- Department and designation management
- Project and task management
- Leave request workflow with approval history
- Audit trail for all system changes
- Dashboard with aggregated reports
- Event-driven architecture with RabbitMQ
- Dual database strategy (PostgreSQL + MongoDB)
- Containerized deployment with Docker Compose

### 🚧 Planned (See AI-WORKFLOW.md)
- Authentication and authorization
- Role-based access control
- Real-time notifications
- Full-text search
- E2E tests
- Cloud deployment

## 🔒 Security Notes

⚠️ **This is a development setup**. For production:
- Change all default passwords in `.env`
- Enable HTTPS
- Implement authentication/authorization
- Use secrets management (Azure Key Vault, AWS Secrets Manager)
- Configure CORS properly
- Enable rate limiting
- Set up database backups

## 🐛 Troubleshooting

### Port Conflicts
If ports 3000, 5000, 5432, 27017, or 15672 are already in use:
```bash
# Edit .env file and change ports
# Then restart
docker compose down
docker compose up
```

### Containers Not Starting
```bash
# Check logs
docker compose logs api
docker compose logs frontend
docker compose logs worker-audit
docker compose logs worker-reports

# Restart specific service
docker compose restart api
```

### Database Connection Issues
```bash
# Ensure databases are healthy
docker compose ps

# If unhealthy, restart
docker compose restart postgres mongodb
```

### Clear Everything and Start Fresh
```bash
docker compose down -v  # Remove containers and volumes
docker system prune -a  # Clean Docker cache (optional)
docker compose up --build
```

## 📈 Monitoring

- **RabbitMQ**: http://localhost:15672 - Monitor message queues
- **API Health**: http://localhost:5000/health
- **API Documentation**: http://localhost:5000/scalar/v1 - Interactive API explorer
- **Logs**: `docker compose logs -f [service-name]`

## 🤖 AI-Assisted Development

This project was built using AI coding assistants. See [AI-WORKFLOW.md](./AI-WORKFLOW.md) for details on:
- Tools used (Claude Code, GitHub Copilot, etc.)
- Architecture planning process
- Code generation workflow
- Debugging and iteration
- Lessons learned

## 📝 Documentation

- **[Documentation Index](./DOCUMENTATION.md)** - Complete documentation overview
- **[Setup Guide](./SETUP.md)** - Detailed setup instructions
- **[AI Workflow](./AI-WORKFLOW.md)** - AI-assisted development process
- **[Changelog](./CHANGELOG.md)** - Version history and changes
- **[Frontend Documentation](./frontend/README.md)** - Frontend setup and development
- **[API Connection Guide](./frontend/API_CONNECTION.md)** - Troubleshooting API connections
- **[Planning Documents](./AI_Planning/)** - Architecture and implementation plans
- **[Interaction History](./AI_Interaction_History/)** - AI interaction logs

## 🤝 Contributing

This is an assignment project, but suggestions are welcome!

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'feat: Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is for educational purposes as part of a distributed systems assignment.

## 👤 Author

[Your Name]
- Email: your.email@example.com
- GitHub: [@yourusername](https://github.com/yourusername)

## 🙏 Acknowledgments

- Assignment provided by [Company/Institution Name]
- Built with AI assistance (Claude, GitHub Copilot)
- Inspired by modern microservices architectures

---

**Last Updated**: February 8, 2026
