import { AxiosError, InternalAxiosRequestConfig, AxiosResponse } from 'axios';
import type { ApiError } from '../../types';

/**
 * Request Interceptor
 * Handles outgoing requests before they are sent
 */
export const requestInterceptor = (config: InternalAxiosRequestConfig): InternalAxiosRequestConfig => {
  // Add auth token if available
  const token = localStorage.getItem('authToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  // Log request in development
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  if ((import.meta as any).env?.DEV) {
    console.log(`[API Request] ${config.method?.toUpperCase()} ${config.url}`, {
      data: config.data,
      params: config.params,
    });
  }

  return config;
};

/**
 * Request Error Interceptor
 * Handles request errors
 */
export const requestErrorInterceptor = (error: any): Promise<never> => {
  console.error('[API Request Error]', error);
  return Promise.reject(error);
};

/**
 * Response Interceptor
 * Handles successful responses
 */
export const responseInterceptor = (response: AxiosResponse): AxiosResponse => {
  // Log response in development
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  if ((import.meta as any).env?.DEV) {
    console.log(
      `[API Response] ${response.config.method?.toUpperCase()} ${response.config.url}`,
      response.data
    );
  }
  return response;
};

/**
 * Response Error Interceptor
 * Transforms error responses to ApiError format
 */
export const responseErrorInterceptor = (error: AxiosError): Promise<never> => {
  // Transform error to ApiError format
  const apiError: ApiError = {
    message: (error.response?.data as any)?.message || error.message || 'An error occurred',
    errors: (error.response?.data as any)?.errors,
  };

  // Log error
  console.error('[API Error]', {
    url: error.config?.url,
    method: error.config?.method,
    status: error.response?.status,
    statusText: error.response?.statusText,
    data: error.response?.data,
    message: apiError.message,
  });

  return Promise.reject(apiError);
};
