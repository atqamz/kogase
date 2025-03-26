import { useState, useCallback } from 'react';
import { projectsApi, Project, CreateProjectRequest, UpdateProjectRequest } from '@/lib/api/projects';

export function useProjects() {
  const [projects, setProjects] = useState<Project[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchProjects = useCallback(async () => {
    try {
      setLoading(true);
      const data = await projectsApi.getProjects();
      setProjects(data);
      setError(null);
    } catch (err) {
      console.error('Failed to fetch projects:', err);
      setError('Failed to load projects. Please try again.');
    } finally {
      setLoading(false);
    }
  }, []);

  const fetchProject = useCallback(async (id: string) => {
    try {
      setLoading(true);
      const data = await projectsApi.getProject(id);
      setError(null);
      return data;
    } catch (err) {
      console.error(`Failed to fetch project ${id}:`, err);
      setError('Failed to load project. Please try again.');
      throw err;
    } finally {
      setLoading(false);
    }
  }, []);

  const createProject = useCallback(async (data: CreateProjectRequest) => {
    try {
      setLoading(true);
      const newProject = await projectsApi.createProject(data);
      setProjects(prev => [...prev, newProject]);
      setError(null);
      return newProject;
    } catch (err) {
      console.error('Failed to create project:', err);
      setError('Failed to create project. Please try again.');
      throw err;
    } finally {
      setLoading(false);
    }
  }, []);

  const updateProject = useCallback(async (id: string, data: UpdateProjectRequest) => {
    try {
      setLoading(true);
      const updatedProject = await projectsApi.updateProject(id, data);
      setProjects(prev => 
        prev.map(project => project.id === id ? updatedProject : project)
      );
      setError(null);
      return updatedProject;
    } catch (err) {
      console.error(`Failed to update project ${id}:`, err);
      setError('Failed to update project. Please try again.');
      throw err;
    } finally {
      setLoading(false);
    }
  }, []);

  const deleteProject = useCallback(async (id: string) => {
    try {
      setLoading(true);
      await projectsApi.deleteProject(id);
      setProjects(prev => prev.filter(project => project.id !== id));
      setError(null);
    } catch (err) {
      console.error(`Failed to delete project ${id}:`, err);
      setError('Failed to delete project. Please try again.');
      throw err;
    } finally {
      setLoading(false);
    }
  }, []);

  const regenerateApiKey = useCallback(async (id: string) => {
    try {
      setLoading(true);
      const { apiKey } = await projectsApi.regenerateApiKey(id);
      setProjects(prev => 
        prev.map(project => project.id === id ? { ...project, apiKey } : project)
      );
      setError(null);
      return apiKey;
    } catch (err) {
      console.error(`Failed to regenerate API key for project ${id}:`, err);
      setError('Failed to regenerate API key. Please try again.');
      throw err;
    } finally {
      setLoading(false);
    }
  }, []);

  return {
    projects,
    loading,
    error,
    fetchProjects,
    fetchProject,
    createProject,
    updateProject,
    deleteProject,
    regenerateApiKey,
  };
} 