# Changelog

All notable changes to the Workforce Management Platform project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Complete backend API with RESTful controllers for all entities
- Frontend React + TypeScript application with modern UI
- API layer with organized structure (config, endpoints, interceptors)
- Dashboard with aggregated data from both databases
- Employee management views (List, Detail)
- Project management views (List)
- Comprehensive error handling and null safety in frontend
- Scalar API documentation (replaces Swagger)
- Dependency injection organization (DependencyInjection.cs)
- Database seeding with initial data
- Docker Compose orchestration for all services
- CI/CD pipeline with GitHub Actions

### Changed
- Migrated from .NET 8 to .NET 10
- Replaced Swagger with Scalar for API documentation
- Reorganized API layer from `services/` to `api/` with better structure
- Separated interceptors into dedicated file
- Updated API URL defaults to match Docker configuration

### Fixed
- MongoDB healthcheck syntax in docker-compose.yml
- RabbitMQ healthcheck format
- npm package installation issues in Dockerfiles
- Missing dependencies (tailwindcss-animate)
- Task naming conflicts (renamed to TaskItem)
- Package version conflicts after .NET 10 migration
- Frontend undefined array errors with null safety
- API connection configuration (Docker vs standalone)
- DashboardService to properly aggregate data from both databases

### Security
- Added CORS configuration for frontend
- Environment variable support for sensitive data

---

## [1.0.0] - 2026-02-08

### Initial Release
- Complete workforce management platform
- Docker Compose setup
- Backend API with PostgreSQL and MongoDB
- Frontend React application
- Worker services for audit logging and report generation
- Event-driven architecture with RabbitMQ
- Comprehensive documentation
