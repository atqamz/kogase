"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { toast } from "sonner";
import { LucideKey, LucideUserCog, LucideTrash2, LucideRefreshCw } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from "@/components/ui/form";
import { Skeleton } from "@/components/ui/skeleton";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";

import { useProjects } from "@/lib/hooks/use-projects";
import { updateProjectSchema, type UpdateProjectFormData } from "@/lib/validators/projects";

export default function Settings() {
  const router = useRouter();
  const { projects, loading: projectsLoading, fetchProjects, updateProject, deleteProject, regenerateApiKey } = useProjects();
  
  const [selectedProjectId, setSelectedProjectId] = useState<string | null>(null);
  const [confirmDeleteOpen, setConfirmDeleteOpen] = useState(false);
  const [confirmRegenerateOpen, setConfirmRegenerateOpen] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const form = useForm<UpdateProjectFormData>({
    resolver: zodResolver(updateProjectSchema),
    defaultValues: {
      name: "",
      description: "",
    },
  });

  useEffect(() => {
    fetchProjects();
  }, [fetchProjects]);

  useEffect(() => {
    if (projects.length > 0 && !selectedProjectId) {
      setSelectedProjectId(projects[0].id);
    }
  }, [projects, selectedProjectId]);

  useEffect(() => {
    if (selectedProjectId) {
      const project = projects.find((p) => p.id === selectedProjectId);
      if (project) {
        form.reset({
          name: project.name,
          description: project.description,
        });
      }
    }
  }, [selectedProjectId, projects, form]);

  const handleProjectChange = (value: string) => {
    setSelectedProjectId(value);
  };

  const onSubmit = async (data: UpdateProjectFormData) => {
    if (!selectedProjectId) return;

    setIsSubmitting(true);
    try {
      await updateProject(selectedProjectId, data);
      toast.success("Project updated successfully");
    } catch (error) {
      console.error("Failed to update project:", error);
      toast.error("Failed to update project");
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDeleteProject = async () => {
    if (!selectedProjectId) return;

    setIsSubmitting(true);
    try {
      await deleteProject(selectedProjectId);
      setConfirmDeleteOpen(false);
      toast.success("Project deleted successfully");
      
      if (projects.length > 1) {
        // Find another project to select
        const newSelectedId = projects.find(p => p.id !== selectedProjectId)?.id;
        setSelectedProjectId(newSelectedId || null);
      } else {
        // No more projects, go back to dashboard
        router.push("/dashboard");
      }
    } catch (error) {
      console.error("Failed to delete project:", error);
      toast.error("Failed to delete project");
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleRegenerateApiKey = async () => {
    if (!selectedProjectId) return;

    setIsSubmitting(true);
    try {
      await regenerateApiKey(selectedProjectId);
      setConfirmRegenerateOpen(false);
      toast.success("API key regenerated successfully");
    } catch (error) {
      console.error("Failed to regenerate API key:", error);
      toast.error("Failed to regenerate API key");
    } finally {
      setIsSubmitting(false);
    }
  };

  const currentProject = selectedProjectId
    ? projects.find((p) => p.id === selectedProjectId)
    : null;

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
        <h1 className="text-3xl font-bold tracking-tight">Settings</h1>
        <div className="sm:w-auto">
          {projectsLoading ? (
            <Skeleton className="h-10 w-full max-w-xs" />
          ) : (
            <Select
              value={selectedProjectId || ""}
              onValueChange={handleProjectChange}
            >
              <SelectTrigger className="w-full max-w-xs">
                <SelectValue placeholder="Select a project" />
              </SelectTrigger>
              <SelectContent>
                {projects.map((project) => (
                  <SelectItem key={project.id} value={project.id}>
                    {project.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          )}
        </div>
      </div>

      {currentProject ? (
        <div className="grid gap-6 md:grid-cols-2">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center">
                <LucideUserCog className="mr-2 h-5 w-5" />
                Project Details
              </CardTitle>
              <CardDescription>Update your project information</CardDescription>
            </CardHeader>
            <CardContent>
              <Form {...form}>
                <form
                  onSubmit={form.handleSubmit(onSubmit)}
                  className="space-y-4"
                >
                  <FormField
                    control={form.control}
                    name="name"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Project Name</FormLabel>
                        <FormControl>
                          <Input placeholder="My Game" {...field} />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                  <FormField
                    control={form.control}
                    name="description"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Description</FormLabel>
                        <FormControl>
                          <Input placeholder="A description of your game" {...field} />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                  <Button type="submit" disabled={isSubmitting}>
                    {isSubmitting ? "Saving..." : "Save Changes"}
                  </Button>
                </form>
              </Form>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle className="flex items-center">
                <LucideKey className="mr-2 h-5 w-5" />
                API Key
              </CardTitle>
              <CardDescription>
                Manage the API key for this project
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div>
                <div className="text-sm font-medium">Current API Key</div>
                <div className="mt-1 rounded-md bg-muted p-3 text-sm font-mono">
                  {currentProject.apiKey}
                </div>
              </div>

              <div>
                <p className="text-sm text-muted-foreground">
                  Use this API key to authenticate your SDK. Keep it secure and
                  never expose it in client-side code.
                </p>
              </div>

              <Dialog
                open={confirmRegenerateOpen}
                onOpenChange={setConfirmRegenerateOpen}
              >
                <DialogTrigger asChild>
                  <Button variant="outline" className="w-full">
                    <LucideRefreshCw className="mr-2 h-4 w-4" />
                    Regenerate API Key
                  </Button>
                </DialogTrigger>
                <DialogContent>
                  <DialogHeader>
                    <DialogTitle>Regenerate API Key</DialogTitle>
                    <DialogDescription>
                      Are you sure you want to regenerate the API key? The current
                      key will be invalidated and you will need to update your
                      integration.
                    </DialogDescription>
                  </DialogHeader>
                  <DialogFooter>
                    <Button
                      variant="outline"
                      onClick={() => setConfirmRegenerateOpen(false)}
                    >
                      Cancel
                    </Button>
                    <Button
                      variant="destructive"
                      onClick={handleRegenerateApiKey}
                      disabled={isSubmitting}
                    >
                      {isSubmitting ? "Regenerating..." : "Regenerate"}
                    </Button>
                  </DialogFooter>
                </DialogContent>
              </Dialog>
            </CardContent>
          </Card>

          <Card className="md:col-span-2">
            <CardHeader>
              <CardTitle className="flex items-center text-destructive">
                <LucideTrash2 className="mr-2 h-5 w-5" />
                Danger Zone
              </CardTitle>
              <CardDescription>
                Irreversible actions for your project
              </CardDescription>
            </CardHeader>
            <CardContent>
              <Alert variant="destructive">
                <AlertTitle>Warning</AlertTitle>
                <AlertDescription>
                  Deleting a project will remove all associated data, including
                  events, metrics, and device information. This action cannot be
                  undone.
                </AlertDescription>
              </Alert>
            </CardContent>
            <CardFooter>
              <Dialog
                open={confirmDeleteOpen}
                onOpenChange={setConfirmDeleteOpen}
              >
                <DialogTrigger asChild>
                  <Button variant="destructive">
                    <LucideTrash2 className="mr-2 h-4 w-4" />
                    Delete Project
                  </Button>
                </DialogTrigger>
                <DialogContent>
                  <DialogHeader>
                    <DialogTitle>Delete Project</DialogTitle>
                    <DialogDescription>
                      Are you sure you want to delete the project &quot;
                      {currentProject.name}&quot;? This action cannot be undone.
                    </DialogDescription>
                  </DialogHeader>
                  <DialogFooter>
                    <Button
                      variant="outline"
                      onClick={() => setConfirmDeleteOpen(false)}
                    >
                      Cancel
                    </Button>
                    <Button
                      variant="destructive"
                      onClick={handleDeleteProject}
                      disabled={isSubmitting}
                    >
                      {isSubmitting ? "Deleting..." : "Delete"}
                    </Button>
                  </DialogFooter>
                </DialogContent>
              </Dialog>
            </CardFooter>
          </Card>
        </div>
      ) : (
        <div className="grid gap-4 md:grid-cols-2">
          <Skeleton className="h-[300px]" />
          <Skeleton className="h-[300px]" />
          <Skeleton className="h-[200px] md:col-span-2" />
        </div>
      )}
    </div>
  );
} 