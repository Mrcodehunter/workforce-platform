# API Layer

This folder contains all API communication logic for the frontend application.

## Structure

```
api/
├── config/
│   ├── axios.config.ts    # Axios client configuration
│   └── interceptors.ts    # Request and response interceptors
├── endpoints/
│   ├── employees.api.ts   # Employee API endpoints
│   ├── departments.api.ts # Department API endpoints
│   ├── designations.api.ts # Designation API endpoints
│   ├── projects.api.ts    # Project API endpoints
│   ├── tasks.api.ts       # Task API endpoints
│   ├── leaveRequests.api.ts # Leave Request API endpoints
│   ├── dashboard.api.ts   # Dashboard API endpoints
│   ├── auditLogs.api.ts   # Audit Log API endpoints
│   └── index.ts           # Central export for all API endpoints
├── constants.ts           # API constants (endpoints, timeouts, etc.)
└── index.ts               # Main export file
```

## Usage

### Import API functions

```typescript
// Import specific API
import { employeesApi } from '../api';

// Or import from main index
import { employeesApi, projectsApi } from '../api';
```

### Use in React Query hooks

```typescript
import { useQuery } from '@tanstack/react-query';
import { employeesApi } from '../api';

const { data, isLoading } = useQuery({
  queryKey: ['employees'],
  queryFn: employeesApi.getAll,
});
```

### Direct API calls

```typescript
import { employeesApi } from '../api';

// Get all employees
const employees = await employeesApi.getAll();

// Get employee by ID
const employee = await employeesApi.getById('123');

// Create employee
const newEmployee = await employeesApi.create({
  firstName: 'John',
  lastName: 'Doe',
  // ... other fields
});
```

## Configuration

### API Base URL

The API base URL is configured in `config/axios.config.ts` and can be set via:

1. Environment variable: `VITE_API_URL`
2. Default: `http://localhost:5000/api` (Docker) or `http://localhost:63890/api` (standalone)
3. Configured in: `constants.ts`

### Axios Interceptors

Interceptors are defined in `config/interceptors.ts` and include:

- **Request Interceptor (`requestInterceptor`):**
  - Adds authentication token from localStorage
  - Logs requests in development mode

- **Request Error Interceptor (`requestErrorInterceptor`):**
  - Handles request errors
  - Logs request errors

- **Response Interceptor (`responseInterceptor`):**
  - Logs successful responses in development mode
  - Returns response data

- **Response Error Interceptor (`responseErrorInterceptor`):**
  - Transforms errors to `ApiError` format
  - Logs error details
  - Handles error responses consistently

## Adding New API Endpoints

1. Create a new file in `api/endpoints/` folder (e.g., `reports.api.ts`)
2. Import `apiClient` from `../config/axios.config`
3. Export an object with API methods
4. Add export to `api/endpoints/index.ts`
5. Use in hooks or components

Example:

```typescript
// api/api/reports.api.ts
import { apiClient } from '../config/axios.config';
import type { Report } from '../../types';

export const reportsApi = {
  getAll: async (): Promise<Report[]> => {
    const response = await apiClient.get<Report[]>('/reports');
    return response.data;
  },
};
```

## Constants

API constants are defined in `constants.ts`:

- `DEFAULT_API_URL` - Default API base URL
- `API_TIMEOUT` - Request timeout (30 seconds)
- `API_ENDPOINTS` - All endpoint paths
- `HTTP_METHODS` - HTTP method constants

## Error Handling

All API errors are transformed to `ApiError` format:

```typescript
interface ApiError {
  message: string;
  errors?: Record<string, string[]>;
}
```

Errors are automatically logged in development mode and can be caught in try-catch blocks or React Query error handlers.
