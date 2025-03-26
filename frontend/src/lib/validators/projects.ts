import { z } from 'zod';

export const createProjectSchema = z.object({
  name: z.string().min(3, 'Project name must be at least 3 characters').max(50, 'Project name must be less than 50 characters'),
  description: z.string().max(500, 'Project description must be less than 500 characters').optional(),
});

export type CreateProjectFormData = z.infer<typeof createProjectSchema>;

export const updateProjectSchema = z.object({
  name: z.string().min(3, 'Project name must be at least 3 characters').max(50, 'Project name must be less than 50 characters').optional(),
  description: z.string().max(500, 'Project description must be less than 500 characters').optional(),
});

export type UpdateProjectFormData = z.infer<typeof updateProjectSchema>; 