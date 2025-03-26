"use client";

import { useState, useEffect } from "react";
import { useSearchParams } from "next/navigation";
import { format, parseISO } from "date-fns";
import {
  flexRender,
  getCoreRowModel,
  getPaginationRowModel,
  getSortedRowModel,
  SortingState,
  useReactTable,
  ColumnDef,
} from "@tanstack/react-table";
import { LucideSearch, LucideChevronDown } from "lucide-react";

import { Event } from "@/lib/api/events";
import { useProjects } from "@/lib/hooks/use-projects";
import { useEvents } from "@/lib/hooks/use-events";
import { Input } from "@/components/ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";

export default function Events() {
  const searchParams = useSearchParams();
  const projectId = searchParams.get("projectId");
  const { projects, loading: projectsLoading, fetchProjects } = useProjects();
  const { events, total, loading: eventsLoading, fetchEvents } = useEvents();

  const [selectedProjectId, setSelectedProjectId] = useState<string | null>(projectId);
  const [searchTerm, setSearchTerm] = useState("");
  const [eventType, setEventType] = useState<string>("");
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(10);
  const [sorting, setSorting] = useState<SortingState>([{ id: "timestamp", desc: true }]);

  const columns: ColumnDef<Event>[] = [
    {
      accessorKey: "eventName",
      header: "Event Name",
    },
    {
      accessorKey: "eventType",
      header: "Type",
    },
    {
      accessorKey: "deviceId",
      header: "Device ID",
      cell: ({ row }) => {
        const deviceId = row.original.deviceId;
        return (
          <div className="max-w-[150px] truncate" title={deviceId}>
            {deviceId}
          </div>
        );
      },
    },
    {
      accessorKey: "timestamp",
      header: "Time",
      cell: ({ row }) => {
        return format(parseISO(row.original.timestamp), "PPp");
      },
    },
    {
      id: "data",
      header: "Data",
      cell: ({ row }) => {
        const data = row.original.data;
        return (
          <Popover>
            <PopoverTrigger asChild>
              <Button variant="outline" size="sm">
                View Data
                <LucideChevronDown className="ml-2 h-4 w-4" />
              </Button>
            </PopoverTrigger>
            <PopoverContent className="max-h-96 overflow-auto">
              <pre className="text-xs">{JSON.stringify(data, null, 2)}</pre>
            </PopoverContent>
          </Popover>
        );
      },
    },
  ];

  const table = useReactTable({
    data: events,
    columns,
    getCoreRowModel: getCoreRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
    getSortedRowModel: getSortedRowModel(),
    onSortingChange: setSorting,
    state: {
      sorting,
      pagination: {
        pageIndex: page,
        pageSize,
      },
    },
    pageCount: Math.ceil(total / pageSize),
    manualPagination: true,
    onPaginationChange: (updater) => {
      const newPagination = 
        typeof updater === 'function' 
          ? updater({ pageIndex: page, pageSize }) 
          : updater;
      
      setPage(newPagination.pageIndex);
      setPageSize(newPagination.pageSize);
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
    if (!selectedProjectId) return;

    const fetchEventData = async () => {
      await fetchEvents({
        projectId: selectedProjectId,
        eventName: searchTerm || undefined,
        eventType: eventType || undefined,
        limit: pageSize,
        offset: page * pageSize,
      });
    };

    fetchEventData();
  }, [selectedProjectId, searchTerm, eventType, page, pageSize, fetchEvents]);

  const handleProjectChange = (value: string) => {
    setSelectedProjectId(value);
    setPage(0);
  };

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    setPage(0);
  };

  const handleEventTypeChange = (value: string) => {
    setEventType(value);
    setPage(0);
  };

  const uniqueEventTypes = Array.from(
    new Set(events.map((event) => event.eventType))
  );

  const totalPages = Math.ceil(total / pageSize);

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
        <h1 className="text-3xl font-bold tracking-tight">Events Explorer</h1>
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

      <Card>
        <CardHeader>
          <CardTitle>Event Filters</CardTitle>
          <CardDescription>
            Filter events by name or type
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="flex flex-col gap-4 sm:flex-row">
            <div className="w-full sm:w-2/3">
              <form
                onSubmit={handleSearch}
                className="flex items-center space-x-2"
              >
                <Input
                  type="text"
                  placeholder="Search event names..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="w-full"
                />
                <Button type="submit">
                  <LucideSearch className="h-4 w-4" />
                </Button>
              </form>
            </div>
            <div className="w-full sm:w-1/3">
              <Select value={eventType} onValueChange={handleEventTypeChange}>
                <SelectTrigger>
                  <SelectValue placeholder="Filter by type" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="">All types</SelectItem>
                  {uniqueEventTypes.map((type) => (
                    <SelectItem key={type} value={type}>
                      {type}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Events</CardTitle>
          <CardDescription>
            {eventsLoading
              ? "Loading events..."
              : `${total} events found. Showing page ${page + 1} of ${
                  totalPages || 1
                }`}
          </CardDescription>
        </CardHeader>
        <CardContent>
          {eventsLoading ? (
            <div className="space-y-2">
              <Skeleton className="h-8 w-full" />
              <Skeleton className="h-12 w-full" />
              <Skeleton className="h-12 w-full" />
              <Skeleton className="h-12 w-full" />
              <Skeleton className="h-12 w-full" />
              <Skeleton className="h-12 w-full" />
            </div>
          ) : (
            <>
              <div className="rounded-md border">
                <Table>
                  <TableHeader>
                    {table.getHeaderGroups().map((headerGroup) => (
                      <TableRow key={headerGroup.id}>
                        {headerGroup.headers.map((header) => (
                          <TableHead key={header.id}>
                            {header.isPlaceholder
                              ? null
                              : flexRender(
                                  header.column.columnDef.header,
                                  header.getContext()
                                )}
                          </TableHead>
                        ))}
                      </TableRow>
                    ))}
                  </TableHeader>
                  <TableBody>
                    {table.getRowModel().rows?.length ? (
                      table.getRowModel().rows.map((row) => (
                        <TableRow
                          key={row.id}
                          data-state={row.getIsSelected() && "selected"}
                        >
                          {row.getVisibleCells().map((cell) => (
                            <TableCell key={cell.id}>
                              {flexRender(
                                cell.column.columnDef.cell,
                                cell.getContext()
                              )}
                            </TableCell>
                          ))}
                        </TableRow>
                      ))
                    ) : (
                      <TableRow>
                        <TableCell
                          colSpan={columns.length}
                          className="h-24 text-center"
                        >
                          No events found
                        </TableCell>
                      </TableRow>
                    )}
                  </TableBody>
                </Table>
              </div>
              <div className="flex items-center justify-end space-x-2 py-4">
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => table.previousPage()}
                  disabled={!table.getCanPreviousPage()}
                >
                  Previous
                </Button>
                <span className="text-sm text-muted-foreground">
                  Page {page + 1} of {totalPages || 1}
                </span>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => table.nextPage()}
                  disabled={!table.getCanNextPage()}
                >
                  Next
                </Button>
              </div>
            </>
          )}
        </CardContent>
      </Card>
    </div>
  );
} 