/**
 * API Layer - Main Export
 * 
 * This is the main entry point for the API layer.
 * Import API functions from here or from specific API files.
 */

// Export all API endpoints
export * from './endpoints';

// Export axios client and config
export { apiClient, axiosConfig } from './config/axios.config';

// Export constants
export * from './constants';
