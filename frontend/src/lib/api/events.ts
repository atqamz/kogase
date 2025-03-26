import apiClient from './client';

export interface Event {
  id: string;
  projectId: string;
  deviceId: string;
  eventType: string;
  eventName: string;
  data: Record<string, unknown>;
  timestamp: string;
  createdAt: string;
}

export interface EventQueryParams {
  projectId: string;
  startDate?: string;
  endDate?: string;
  eventType?: string;
  eventName?: string;
  deviceId?: string;
  limit?: number;
  offset?: number;
}

export interface EventsResponse {
  events: Event[];
  total: number;
  offset: number;
  limit: number;
}

export interface EventsCountByType {
  eventType: string;
  count: number;
}

export interface EventsCountByDay {
  date: string;
  count: number;
}

export interface DeviceStats {
  totalDevices: number;
  newDevicesToday: number;
  activeDevicesToday: number;
  devicesCountByPlatform: { platform: string; count: number }[];
}

export const eventsApi = {
  getEvents: async (params: EventQueryParams): Promise<EventsResponse> => {
    const response = await apiClient.get<EventsResponse>('/events', { params });
    return response.data;
  },
  
  getEventById: async (id: string): Promise<Event> => {
    const response = await apiClient.get<Event>(`/events/${id}`);
    return response.data;
  },
  
  getEventCountByType: async (projectId: string, startDate?: string, endDate?: string): Promise<EventsCountByType[]> => {
    const params = { projectId, startDate, endDate };
    const response = await apiClient.get<EventsCountByType[]>('/events/count-by-type', { params });
    return response.data;
  },
  
  getEventCountByDay: async (projectId: string, startDate?: string, endDate?: string): Promise<EventsCountByDay[]> => {
    const params = { projectId, startDate, endDate };
    const response = await apiClient.get<EventsCountByDay[]>('/events/count-by-day', { params });
    return response.data;
  },
  
  getDeviceStats: async (projectId: string): Promise<DeviceStats> => {
    const response = await apiClient.get<DeviceStats>(`/projects/${projectId}/device-stats`);
    return response.data;
  }
}; 