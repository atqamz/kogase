"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import {
  LucideBarChart2,
  LucideUsers,
  LucideActivity,
  LucidePlusCircle,
} from "lucide-react";
import { toast } from "sonner";

import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog";
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from "@/components/ui/form";
import { Input } from "@/components/ui/input";

import { useProjects } from "@/lib/hooks/use-projects";
import { useEvents } from "@/lib/hooks/use-events";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { createProjectSchema, type CreateProjectFormData } from "@/lib/validators/projects";
import { Skeleton } from "@/components/ui/skeleton";
import { format } from "date-fns";

export default function Dashboard() {
  const router = useRouter();
  const { projects, loading: projectsLoading, fetchProjects, createProject } = useProjects();
  const { fetchDeviceStats } = useEvents();
  const [selectedProject, setSelectedProject] = useState<string | null>(null);
  const [deviceStats, setDeviceStats] = useState<{
    totalDevices: number;
    newDevicesToday: number;
    activeDevicesToday: number;
  } | null>(null);
  const [statsLoading, setStatsLoading] = useState(false);
  const [openDialog, setOpenDialog] = useState(false);

  const form = useForm<CreateProjectFormData>({
    resolver: zodResolver(createProjectSchema),
    defaultValues: {
      name: "",
      description: "",
    },
  });

  useEffect(() => {
    fetchProjects();
  }, [fetchProjects]);

  useEffect(() => {
    if (projects.length > 0 && !selectedProject) {
      setSelectedProject(projects[0].id);
    }
  }, [projects, selectedProject]);

  useEffect(() => {
    async function loadDeviceStats() {
      if (selectedProject) {
        setStatsLoading(true);
        try {
          const stats = await fetchDeviceStats(selectedProject);
          setDeviceStats(stats);
        } catch (error) {
          console.error("Failed to load device stats:", error);
          setDeviceStats({
            totalDevices: 0,
            newDevicesToday: 0,
            activeDevicesToday: 0,
          });
        } finally {
          setStatsLoading(false);
        }
      }
    }

    loadDeviceStats();
  }, [selectedProject, fetchDeviceStats]);

  const handleProjectChange = (value: string) => {
    setSelectedProject(value);
  };

  const onSubmit = async (data: CreateProjectFormData) => {
    try {
      const newProject = await createProject({
        name: data.name,
        description: data.description || ""
      });
      toast.success("Project created successfully!");
      setSelectedProject(newProject.id);
      setOpenDialog(false);
      form.reset();
    } catch (error) {
      console.error("Failed to create project:", error);
      toast.error("Failed to create project. Please try again.");
    }
  };

  const goToAnalytics = () => {
    if (selectedProject) {
      router.push(`/dashboard/analytics?projectId=${selectedProject}`);
    }
  };

  const goToEvents = () => {
    if (selectedProject) {
      router.push(`/dashboard/events?projectId=${selectedProject}`);
    }
  };

  const currentProject = selectedProject ? projects.find(p => p.id === selectedProject) : null;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold tracking-tight">Dashboard</h1>
        <Dialog open={openDialog} onOpenChange={setOpenDialog}>
          <DialogTrigger asChild>
            <Button>
              <LucidePlusCircle className="mr-2 h-4 w-4" />
              New Project
            </Button>
          </DialogTrigger>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>Create a new project</DialogTitle>
              <DialogDescription>
                Create a new project to start tracking events for your game.
              </DialogDescription>
            </DialogHeader>
            <Form {...form}>
              <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
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
                <DialogFooter>
                  <Button type="submit" disabled={form.formState.isSubmitting}>
                    {form.formState.isSubmitting ? "Creating..." : "Create Project"}
                  </Button>
                </DialogFooter>
              </form>
            </Form>
          </DialogContent>
        </Dialog>
      </div>

      {projectsLoading ? (
        <Skeleton className="h-10 w-full max-w-xs" />
      ) : projects.length === 0 ? (
        <div className="rounded-lg border p-8 text-center">
          <h2 className="text-lg font-semibold">No projects found</h2>
          <p className="mt-2 text-sm text-muted-foreground">
            Get started by creating your first project.
          </p>
          <Button className="mt-4" onClick={() => setOpenDialog(true)}>
            <LucidePlusCircle className="mr-2 h-4 w-4" />
            Create Project
          </Button>
        </div>
      ) : (
        <div className="flex items-center space-x-4">
          <div className="w-full max-w-xs">
            <Select value={selectedProject || ""} onValueChange={handleProjectChange}>
              <SelectTrigger>
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
          </div>
        </div>
      )}

      {selectedProject && currentProject && (
        <>
          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">Total Devices</CardTitle>
                <LucideUsers className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                {statsLoading ? (
                  <Skeleton className="h-8 w-20" />
                ) : (
                  <div className="text-2xl font-bold">{deviceStats?.totalDevices || 0}</div>
                )}
              </CardContent>
            </Card>
            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">New Devices Today</CardTitle>
                <LucideUsers className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                {statsLoading ? (
                  <Skeleton className="h-8 w-20" />
                ) : (
                  <div className="text-2xl font-bold">{deviceStats?.newDevicesToday || 0}</div>
                )}
              </CardContent>
            </Card>
            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">Active Devices Today</CardTitle>
                <LucideActivity className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                {statsLoading ? (
                  <Skeleton className="h-8 w-20" />
                ) : (
                  <div className="text-2xl font-bold">{deviceStats?.activeDevicesToday || 0}</div>
                )}
              </CardContent>
            </Card>
          </div>

          <div className="mt-6 grid gap-4 md:grid-cols-2">
            <Card>
              <CardHeader>
                <CardTitle>Project Details</CardTitle>
                <CardDescription>
                  Information about your selected project
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-2">
                <div>
                  <span className="font-medium">Name:</span> {currentProject.name}
                </div>
                {currentProject.description && (
                  <div>
                    <span className="font-medium">Description:</span> {currentProject.description}
                  </div>
                )}
                <div>
                  <span className="font-medium">Created:</span>{" "}
                  {format(new Date(currentProject.createdAt), "PPP")}
                </div>
                <div className="pt-2">
                  <span className="font-medium block mb-1">API Key:</span>
                  <code className="block rounded bg-muted p-2 text-sm">
                    {currentProject.apiKey}
                  </code>
                </div>
              </CardContent>
              <CardFooter className="flex justify-between border-t pt-5">
                <Button variant="outline" onClick={goToAnalytics}>
                  <LucideBarChart2 className="mr-2 h-4 w-4" />
                  View Analytics
                </Button>
                <Button variant="outline" onClick={goToEvents}>
                  <LucideActivity className="mr-2 h-4 w-4" />
                  Explore Events
                </Button>
              </CardFooter>
            </Card>
            <Card>
              <CardHeader>
                <CardTitle>Quick Tips</CardTitle>
                <CardDescription>
                  Getting started with Kogase
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="rounded-lg bg-muted p-4">
                  <h3 className="font-semibold">1. Integrate the SDK</h3>
                  <p className="mt-1 text-sm">
                    Use our Unity SDK to start tracking events in your game.
                  </p>
                </div>
                <div className="rounded-lg bg-muted p-4">
                  <h3 className="font-semibold">2. Track Key Events</h3>
                  <p className="mt-1 text-sm">
                    Track game starts, level completions, and in-app purchases.
                  </p>
                </div>
                <div className="rounded-lg bg-muted p-4">
                  <h3 className="font-semibold">3. Analyze Your Data</h3>
                  <p className="mt-1 text-sm">
                    Use the dashboard to gain insights about player behavior.
                  </p>
                </div>
              </CardContent>
              <CardFooter className="border-t pt-5">
                <Button className="w-full" variant="outline">
                  View Documentation
                </Button>
              </CardFooter>
            </Card>
          </div>
        </>
      )}
    </div>
  );
} 