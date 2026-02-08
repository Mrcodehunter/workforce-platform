/**
 * API Constants
 * Centralized constants for API configuration
 */

/**
 * Default API base URL
 * 
 * When running with Docker Compose: http://localhost:5000/api
 * When running backend standalone (dotnet run): http://localhost:63890/api
 * 
 * Override with VITE_API_URL environment variable
 */
export const DEFAULT_API_URL = 'http://localhost:5000/api';

/**
 * API timeout in milliseconds
 */
export const API_TIMEOUT = 30000; // 30 seconds

/**
 * API endpoints
 */
export const API_ENDPOINTS = {
  EMPLOYEES: '/employees',
  DEPARTMENTS: '/departments',
  DESIGNATIONS: '/designations',
  PROJECTS: '/projects',
  TASKS: '/tasks',
  LEAVE_REQUESTS: '/leaverequests',
  DASHBOARD: '/dashboard',
  AUDIT_LOGS: '/auditlogs',
  HEALTH: '/health',
} as const;

/**
 * HTTP Methods
 */
export const HTTP_METHODS = {
  GET: 'GET',
  POST: 'POST',
  PUT: 'PUT',
  DELETE: 'DELETE',
  PATCH: 'PATCH',
} as const;
