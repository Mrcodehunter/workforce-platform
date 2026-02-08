import axios, { AxiosInstance } from 'axios';
import {
  requestInterceptor,
  requestErrorInterceptor,
  responseInterceptor,
  responseErrorInterceptor,
} from './interceptors';
import { DEFAULT_API_URL, API_TIMEOUT } from '../constants';

/**
 * Axios client configuration
 * Centralized configuration for all API requests
 */
class AxiosConfig {
  private client: AxiosInstance;

  constructor() {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    const baseURL = ((import.meta as any).env?.VITE_API_URL as string) || DEFAULT_API_URL;

    this.client = axios.create({
      baseURL,
      headers: {
        'Content-Type': 'application/json',
      },
      timeout: API_TIMEOUT,
    });

    this.setupInterceptors();
  }

  /**
   * Setup request and response interceptors
   */
  private setupInterceptors(): void {
    // Request interceptors
    this.client.interceptors.request.use(requestInterceptor, requestErrorInterceptor);

    // Response interceptors
    this.client.interceptors.response.use(responseInterceptor, responseErrorInterceptor);
  }

  /**
   * Get the configured axios instance
   */
  getClient(): AxiosInstance {
    return this.client;
  }
}

// Export singleton instance
export const axiosConfig = new AxiosConfig();
export const apiClient = axiosConfig.getClient();
