import { apiClient } from '../config/axios.config';
import type { Project, ProjectListItem } from '../../types';

/**
 * Project API endpoints
 */
export const projectsApi = {
  /**
   * Get all projects
   */
  getAll: async (): Promise<ProjectListItem[]> => {
    const response = await apiClient.get<ProjectListItem[]>('/projects');
    return response.data;
  },

  /**
   * Get project by ID
   */
  getById: async (id: string): Promise<Project> => {
    const response = await apiClient.get<Project>(`/projects/${id}`);
    return response.data;
  },

  /**
   * Create a new project
   */
  create: async (project: Partial<Project>): Promise<Project> => {
    const response = await apiClient.post<Project>('/projects', project);
    return response.data;
  },

  /**
   * Update an existing project
   */
  update: async (id: string, project: Partial<Project>): Promise<Project> => {
    const response = await apiClient.put<Project>(`/projects/${id}`, { ...project, id });
    return response.data;
  },

  /**
   * Delete a project
   */
  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`/projects/${id}`);
  },

  /**
   * Add a member to a project
   */
  addMember: async (projectId: string, employeeId: string, role?: string): Promise<Project> => {
    const response = await apiClient.post<Project>(`/projects/${projectId}/members`, {
      employeeId,
      role,
    });
    return response.data;
  },

  /**
   * Remove a member from a project
   */
  removeMember: async (projectId: string, employeeId: string): Promise<Project> => {
    const response = await apiClient.delete<Project>(`/projects/${projectId}/members/${employeeId}`);
    return response.data;
  },
};
