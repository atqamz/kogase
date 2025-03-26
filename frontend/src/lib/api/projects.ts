import apiClient from './client';

export interface Project {
  id: string;
  name: string;
  description: string;
  apiKey: string;
  userId: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateProjectRequest {
  name: string;
  description: string;
}

export interface UpdateProjectRequest {
  name?: string;
  description?: string;
}

export const projectsApi = {
  getProjects: async (): Promise<Project[]> => {
    const response = await apiClient.get<Project[]>('/projects');
    return response.data;
  },
  
  getProject: async (id: string): Promise<Project> => {
    const response = await apiClient.get<Project>(`/projects/${id}`);
    return response.data;
  },
  
  createProject: async (data: CreateProjectRequest): Promise<Project> => {
    const response = await apiClient.post<Project>('/projects', data);
    return response.data;
  },
  
  updateProject: async (id: string, data: UpdateProjectRequest): Promise<Project> => {
    const response = await apiClient.put<Project>(`/projects/${id}`, data);
    return response.data;
  },
  
  deleteProject: async (id: string): Promise<void> => {
    await apiClient.delete(`/projects/${id}`);
  },
  
  regenerateApiKey: async (id: string): Promise<{ apiKey: string }> => {
    const response = await apiClient.post<{ apiKey: string }>(`/projects/${id}/regenerate-api-key`);
    return response.data;
  }
}; 