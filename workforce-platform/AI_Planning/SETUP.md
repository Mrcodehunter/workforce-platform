# Setup Guide

Complete setup instructions for the Workforce Management Platform.

## Prerequisites

- **Docker** (version 24+) and **Docker Compose** (version 2.20+)
- **Git**
- **Node.js 18+** (for local frontend development)
- **.NET 10 SDK** (for local backend development)

## Quick Start (Docker Compose)

### 1. Clone Repository
```bash
git clone https://github.com/YOUR_USERNAME/workforce-platform.git
cd workforce-platform
```

### 2. Start All Services
```bash
docker compose up --build
```

This will:
- Build all Docker images
- Start PostgreSQL, MongoDB, and RabbitMQ
- Start the backend API
- Start the frontend
- Start both worker services
- Run database migrations
- Seed initial data

### 3. Access Services
- **Frontend**: http://localhost:3000
- **Backend API**: http://localhost:5000
- **API Documentation**: http://localhost:5000 (Scalar UI)
- **RabbitMQ Management**: http://localhost:15672 (guest/guest)

**Note**: First startup takes 3-5 minutes for initial builds.

## Development Setup

### Backend Development

#### Standalone Backend
```bash
cd backend/WorkforceAPI
dotnet restore
dotnet run
```

Backend will run on:
- HTTP: http://localhost:63890
- HTTPS: https://localhost:63889

#### With Hot Reload
```bash
cd backend/WorkforceAPI
dotnet watch run
```

### Frontend Development

#### Standalone Frontend
```bash
cd frontend
npm install
npm run dev
```

Frontend will run on: http://localhost:3000

#### Configure API URL
Create `.env` file in `frontend/` directory:

**For Docker backend:**
```env
VITE_API_URL=http://localhost:5000/api
```

**For standalone backend:**
```env
VITE_API_URL=http://localhost:63890/api
```

### Database Setup

#### PostgreSQL
- **Host**: localhost (Docker) or postgres (container)
- **Port**: 5432
- **Database**: workforce_db
- **Username**: admin
- **Password**: changeme (change in production!)

#### MongoDB
- **Host**: localhost (Docker) or mongodb (container)
- **Port**: 27017
- **Database**: workforce_db
- **Username**: admin
- **Password**: changeme (change in production!)

#### Running Migrations
```bash
cd backend/WorkforceAPI
dotnet ef database update
```

#### Seeding Data
Data is automatically seeded on first startup. To reset:
```bash
docker compose down -v
docker compose up --build
```

## Environment Variables

### Docker Compose
Create `.env` file in project root:
```env
POSTGRES_DB=workforce_db
POSTGRES_USER=admin
POSTGRES_PASSWORD=changeme

MONGO_DB=workforce_db
MONGO_USER=admin
MONGO_PASSWORD=changeme

RABBITMQ_USER=guest
RABBITMQ_PASSWORD=guest
```

### Frontend
Create `.env` file in `frontend/` directory:
```env
VITE_API_URL=http://localhost:5000/api
```

### Backend
Configuration in `appsettings.json`:
- Connection strings
- RabbitMQ settings
- CORS origins
- Serilog configuration

## Troubleshooting

### Port Conflicts
If ports are already in use, update `docker-compose.yml` or `.env`:
```yaml
ports:
  - "3001:80"  # Change frontend port
  - "5001:5000"  # Change API port
```

### Containers Not Starting
```bash
# Check logs
docker compose logs api
docker compose logs frontend

# Restart specific service
docker compose restart api
```

### Database Connection Issues
```bash
# Check database health
docker compose ps

# Restart databases
docker compose restart postgres mongodb
```

### Frontend API Connection
See [frontend/API_CONNECTION.md](./frontend/API_CONNECTION.md) for detailed troubleshooting.

### Clear Everything
```bash
docker compose down -v  # Remove containers and volumes
docker system prune -a  # Clean Docker cache (optional)
docker compose up --build
```

## Verification

### Check All Services
```bash
docker compose ps
```

All services should show "Up" status.

### Test API
```bash
curl http://localhost:5000/health
```

Should return: `{"status":"healthy","timestamp":"..."}`

### Test Frontend
Open http://localhost:3000 in browser. Should see the dashboard.

### Test Database Connections
```bash
# PostgreSQL
docker compose exec postgres psql -U admin -d workforce_db -c "SELECT COUNT(*) FROM \"Employees\";"

# MongoDB
docker compose exec mongodb mongosh -u admin -p changeme --eval "db.LeaveRequests.countDocuments()"
```

## Production Considerations

⚠️ **This setup is for development only!**

For production:
1. Change all default passwords
2. Use environment variables for secrets
3. Enable HTTPS
4. Configure proper CORS
5. Set up database backups
6. Use secrets management
7. Enable rate limiting
8. Configure logging aggregation
9. Set up monitoring and alerts

---

For more details, see:
- [README.md](./README.md) - Project overview
- [frontend/README.md](./frontend/README.md) - Frontend setup
- [frontend/API_CONNECTION.md](./frontend/API_CONNECTION.md) - API troubleshooting
