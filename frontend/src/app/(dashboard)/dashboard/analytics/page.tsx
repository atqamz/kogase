"use client";

import { useEffect, useState } from "react";
import { useSearchParams } from "next/navigation";
import { format, parseISO, subDays } from "date-fns";
import {
  AreaChart,
  Area,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  PieChart,
  Pie,
  Cell,
} from "recharts";
import { LucideCalendar } from "lucide-react";

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Skeleton } from "@/components/ui/skeleton";
import { useProjects } from "@/lib/hooks/use-projects";
import { useEvents } from "@/lib/hooks/use-events";

const COLORS = ["#0088FE", "#00C49F", "#FFBB28", "#FF8042", "#8884D8", "#FF6B6B"];

export default function Analytics() {
  const searchParams = useSearchParams();
  const projectId = searchParams.get("projectId");
  const { projects, loading: projectsLoading, fetchProjects } = useProjects();
  const { fetchEventCountByDay, fetchEventCountByType, fetchDeviceStats } = useEvents();

  const [selectedProjectId, setSelectedProjectId] = useState<string | null>(projectId);
  const [selectedDateRange, setSelectedDateRange] = useState<string>("7");
  const [eventsOverTime, setEventsOverTime] = useState<{ date: string; count: number }[]>([]);
  const [eventsByType, setEventsByType] = useState<{ eventType: string; count: number }[]>([]);
  const [deviceStats, setDeviceStats] = useState<{
    totalDevices: number;
    devicesCountByPlatform: { platform: string; count: number }[];
  } | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchProjects();
  }, [fetchProjects]);

  useEffect(() => {
    if (projects.length > 0 && !selectedProjectId) {
      setSelectedProjectId(projects[0].id);
    }
  }, [projects, selectedProjectId]);

  useEffect(() => {
    if (!selectedProjectId) return;

    async function fetchAnalyticsData() {
      setLoading(true);
      try {
        const endDate = new Date().toISOString();
        const startDate = subDays(
          new Date(),
          parseInt(selectedDateRange)
        ).toISOString();

        const [eventsPerDay, eventTypes, deviceData] = await Promise.all([
          fetchEventCountByDay(selectedProjectId as string, startDate, endDate),
          fetchEventCountByType(selectedProjectId as string, startDate, endDate),
          fetchDeviceStats(selectedProjectId as string),
        ]);

        setEventsOverTime(eventsPerDay);
        setEventsByType(eventTypes);
        setDeviceStats(deviceData);
      } catch (error) {
        console.error("Failed to fetch analytics data:", error);
      } finally {
        setLoading(false);
      }
    }

    fetchAnalyticsData();
  }, [selectedProjectId, selectedDateRange, fetchEventCountByDay, fetchEventCountByType, fetchDeviceStats]);

  const handleProjectChange = (value: string) => {
    setSelectedProjectId(value);
  };

  const handleDateRangeChange = (value: string) => {
    setSelectedDateRange(value);
  };

  const formatEventData = (
    data: { date: string; count: number }[] = []
  ) => {
    return data.map((item) => ({
      date: format(parseISO(item.date), "MMM d"),
      count: item.count,
    }));
  };

  const currentProject = selectedProjectId
    ? projects.find((p) => p.id === selectedProjectId)
    : null;

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
        <h1 className="text-3xl font-bold tracking-tight">Analytics</h1>
        <div className="flex flex-col gap-2 sm:flex-row sm:items-center">
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
          <Select
            value={selectedDateRange}
            onValueChange={handleDateRangeChange}
          >
            <SelectTrigger className="w-full max-w-[180px]">
              <SelectValue placeholder="Date range" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="7">Last 7 days</SelectItem>
              <SelectItem value="14">Last 14 days</SelectItem>
              <SelectItem value="30">Last 30 days</SelectItem>
              <SelectItem value="90">Last 90 days</SelectItem>
            </SelectContent>
          </Select>
        </div>
      </div>

      {!loading && currentProject ? (
        <>
          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">
                  Total Events
                </CardTitle>
                <LucideCalendar className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">
                  {eventsOverTime.reduce((sum, item) => sum + item.count, 0)}
                </div>
                <p className="text-xs text-muted-foreground">
                  Last {selectedDateRange} days
                </p>
              </CardContent>
            </Card>
            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">
                  Unique Devices
                </CardTitle>
                <LucideCalendar className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">
                  {deviceStats?.totalDevices || 0}
                </div>
                <p className="text-xs text-muted-foreground">All time</p>
              </CardContent>
            </Card>
            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">
                  Avg. Events Per Day
                </CardTitle>
                <LucideCalendar className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">
                  {eventsOverTime.length > 0
                    ? Math.round(
                        eventsOverTime.reduce((sum, item) => sum + item.count, 0) /
                          eventsOverTime.length
                      )
                    : 0}
                </div>
                <p className="text-xs text-muted-foreground">
                  Last {selectedDateRange} days
                </p>
              </CardContent>
            </Card>
          </div>

          <div className="grid gap-4 md:grid-cols-2">
            <Card className="col-span-1">
              <CardHeader>
                <CardTitle>Events Over Time</CardTitle>
                <CardDescription>
                  Total events per day over the selected time period
                </CardDescription>
              </CardHeader>
              <CardContent className="h-[300px]">
                <ResponsiveContainer width="100%" height="100%">
                  <AreaChart
                    data={formatEventData(eventsOverTime)}
                    margin={{ top: 10, right: 30, left: 0, bottom: 0 }}
                  >
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="date" />
                    <YAxis />
                    <Tooltip />
                    <Area
                      type="monotone"
                      dataKey="count"
                      stroke="#8884d8"
                      fill="#8884d8"
                    />
                  </AreaChart>
                </ResponsiveContainer>
              </CardContent>
            </Card>

            <Card className="col-span-1">
              <CardHeader>
                <CardTitle>Events by Type</CardTitle>
                <CardDescription>
                  Distribution of event types over the selected time period
                </CardDescription>
              </CardHeader>
              <CardContent className="h-[300px]">
                {eventsByType.length === 0 ? (
                  <div className="flex h-full items-center justify-center">
                    <p className="text-center text-muted-foreground">
                      No events recorded yet
                    </p>
                  </div>
                ) : (
                  <ResponsiveContainer width="100%" height="100%">
                    <BarChart
                      data={eventsByType}
                      margin={{ top: 5, right: 30, left: 20, bottom: 5 }}
                    >
                      <CartesianGrid strokeDasharray="3 3" />
                      <XAxis dataKey="eventType" />
                      <YAxis />
                      <Tooltip />
                      <Bar dataKey="count" fill="#8884d8" />
                    </BarChart>
                  </ResponsiveContainer>
                )}
              </CardContent>
            </Card>
          </div>

          <Card>
            <CardHeader>
              <CardTitle>Device Platforms</CardTitle>
              <CardDescription>
                Distribution of devices by platform
              </CardDescription>
            </CardHeader>
            <CardContent className="h-[300px]">
              {!deviceStats?.devicesCountByPlatform ||
              deviceStats.devicesCountByPlatform.length === 0 ? (
                <div className="flex h-full items-center justify-center">
                  <p className="text-center text-muted-foreground">
                    No device data available
                  </p>
                </div>
              ) : (
                <ResponsiveContainer width="100%" height="100%">
                  <PieChart>
                    <Pie
                      data={deviceStats.devicesCountByPlatform}
                      cx="50%"
                      cy="50%"
                      labelLine={true}
                      outerRadius={100}
                      fill="#8884d8"
                      dataKey="count"
                      nameKey="platform"
                      label={({ platform, count, percent }) => 
                        `${platform}: ${count} (${(percent * 100).toFixed(0)}%)`
                      }
                    >
                      {deviceStats.devicesCountByPlatform.map((entry, index) => (
                        <Cell
                          key={`cell-${index}`}
                          fill={COLORS[index % COLORS.length]}
                        />
                      ))}
                    </Pie>
                    <Tooltip />
                  </PieChart>
                </ResponsiveContainer>
              )}
            </CardContent>
          </Card>
        </>
      ) : (
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
          <Skeleton className="h-[150px]" />
          <Skeleton className="h-[150px]" />
          <Skeleton className="h-[150px]" />
          <Skeleton className="h-[300px] md:col-span-2" />
          <Skeleton className="h-[300px] md:col-span-2" />
          <Skeleton className="h-[300px] md:col-span-4" />
        </div>
      )}
    </div>
  );
} 