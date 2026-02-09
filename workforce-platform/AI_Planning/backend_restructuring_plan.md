# Backend Restructuring Plan

## Overview
Restructure backend to improve Architecture & Design (20%) and Decision Making (15%) scores by:
- Centralizing dependency injection configuration
- Adding environment-based configuration
- Improving separation of concerns
- Maintaining all existing functionality

## Current Issues

1. **Configuration Scattered**: Redis and RabbitMQ config in multiple places
2. **No Environment Separation**: Same config for dev/prod
3. **DI Not Centralized**: Some DI in Program.cs, some in extensions
4. **Missing Configuration Options**: No structured config classes

## Restructuring Plan

### 1. Create Centralized DI Configuration
**File**: `Workforce.Shared/DependencyInjection/ServiceCollectionExtensions.cs`
- Consolidate all shared DI registrations
- Add environment-aware configuration
- Include health checks
- Add configuration validation

### 2. Create Configuration Options Classes
**Files**: 
- `Workforce.Shared/Configuration/RedisOptions.cs`
- `Workforce.Shared/Configuration/RabbitMqOptions.cs`
- `Workforce.Shared/Configuration/DatabaseOptions.cs`

### 3. Enhance Existing Extensions
- Update `RedisExtensions.cs` to use configuration options
- Update `RabbitMqExtensions.cs` to use configuration options
- Add environment-based defaults

### 4. Refactor Program.cs Files
- Use centralized DI methods
- Add environment-based configuration
- Improve error handling

### 5. Add Environment-Specific Configuration
- `appsettings.Development.json`
- `appsettings.Production.json`
- Environment variable support

### 6. Update Documentation
- Update main README.md
- Update AI-WORKFLOW.md
- Document architecture decisions

## Implementation Steps

1. Create configuration option classes
2. Enhance DI extensions with options pattern
3. Create centralized ServiceCollectionExtensions
4. Update Program.cs files
5. Add environment-specific appsettings
6. Test all functionality
7. Update documentation
