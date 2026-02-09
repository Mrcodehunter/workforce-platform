# Third-Party Libraries

Complete list of third-party libraries with versions and rationale.

## Backend (.NET)

**Core**: Microsoft.AspNetCore.OpenApi (10.0.0), Microsoft.OpenApi (2.0.0), Scalar.AspNetCore (2.12.36)  
*Rationale*: Native .NET 10 OpenAPI support, modern API docs UI

**Database**: Microsoft.EntityFrameworkCore (10.0.0), Npgsql.EntityFrameworkCore.PostgreSQL (10.0.0), MongoDB.Driver (2.24.0)  
*Rationale*: Type-safe LINQ, code-first migrations, official drivers, async support

**Validation/Mapping**: FluentValidation.AspNetCore (11.3.0), AutoMapper.Extensions.Microsoft.DependencyInjection (12.0.1)  
*Rationale*: Expressive validation rules, reduces mapping boilerplate

**Logging**: Serilog.AspNetCore (10.0.0), Serilog.Settings.Configuration (10.0.0), Serilog.Sinks.Console (6.1.1)  
*Rationale*: Structured logging, multiple sinks, flexible configuration

**Messaging/Cache**: RabbitMQ.Client (6.8.1), StackExchange.Redis (2.8.16)  
*Rationale*: Reliable message delivery, high-performance caching, multi-language support

**Workers**: Microsoft.Extensions.Hosting (10.0.0), Microsoft.Extensions.Diagnostics.HealthChecks (10.0.0)  
*Rationale*: Native background service support, health monitoring

## Frontend (React/TypeScript)

**Core**: React (18.2.0), TypeScript (5.3.3), React Router DOM (6.21.3)  
*Rationale*: Required by assignment, type safety, standard routing

**HTTP/State**: Axios (1.6.5), @tanstack/react-query (5.17.19), Zustand (4.5.0)  
*Rationale*: Interceptors, automatic caching, lightweight state management

**Forms/Styling**: React Hook Form (7.49.3), Tailwind CSS (3.4.1), clsx (2.1.0), tailwind-merge (2.2.1)  
*Rationale*: Minimal re-renders, utility-first CSS, conditional classes

**UI/Data**: Lucide React (0.323.0), Recharts (2.10.4), date-fns (3.3.1)  
*Rationale*: Modern icons, React-native charts, lightweight date utilities

**Build**: Vite (5.0.11), @vitejs/plugin-react (4.2.1)  
*Rationale*: Fast HMR, optimized builds, modern tooling

**Quality**: ESLint (8.56.0), @typescript-eslint/* (6.19.0)  
*Rationale*: Code quality, TypeScript-specific linting

## Workers (Node.js)

**Messaging**: amqplib (0.10.3)  
*Rationale*: Official RabbitMQ client, async/await support

**Databases**: mongodb (6.3.0), pg (8.11.3)  
*Rationale*: Official drivers, high performance, connection pooling

**Utilities**: winston (3.11.0), node-cron (3.0.3), dotenv (16.4.1)  
*Rationale*: Structured logging, task scheduling, environment variables

## Infrastructure

**Databases**: PostgreSQL 16, MongoDB 7  
*Rationale*: Advanced features, robust tooling, flexible schema support

**Services**: RabbitMQ 3.12, Redis 7, Nginx (Alpine)  
*Rationale*: Reliable messaging, high-performance caching, efficient web server

## Version Strategy
- Using `^` for automatic patch updates
- Major versions tested before upgrading
- Regular security patch updates

---

**Total**: 18 backend packages, 26 frontend packages, 11 worker packages, 5 infrastructure services
