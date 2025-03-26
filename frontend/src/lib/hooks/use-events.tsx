import { useState, useCallback } from 'react';
import { 
  eventsApi, 
  Event, 
  EventQueryParams
} from '@/lib/api/events';

export function useEvents() {
  const [events, setEvents] = useState<Event[]>([]);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchEvents = useCallback(async (params: EventQueryParams) => {
    try {
      setLoading(true);
      const response = await eventsApi.getEvents(params);
      setEvents(response.events);
      setTotal(response.total);
      setError(null);
      return response;
    } catch (err) {
      console.error('Failed to fetch events:', err);
      setError('Failed to load events. Please try again.');
      throw err;
    } finally {
      setLoading(false);
    }
  }, []);

  const fetchEventById = useCallback(async (id: string) => {
    try {
      setLoading(true);
      const event = await eventsApi.getEventById(id);
      setError(null);
      return event;
    } catch (err) {
      console.error(`Failed to fetch event ${id}:`, err);
      setError('Failed to load event. Please try again.');
      throw err;
    } finally {
      setLoading(false);
    }
  }, []);

  const fetchEventCountByType = useCallback(async (projectId: string, startDate?: string, endDate?: string) => {
    try {
      setLoading(true);
      const data = await eventsApi.getEventCountByType(projectId, startDate, endDate);
      setError(null);
      return data;
    } catch (err) {
      console.error('Failed to fetch event count by type:', err);
      setError('Failed to load event statistics. Please try again.');
      throw err;
    } finally {
      setLoading(false);
    }
  }, []);

  const fetchEventCountByDay = useCallback(async (projectId: string, startDate?: string, endDate?: string) => {
    try {
      setLoading(true);
      const data = await eventsApi.getEventCountByDay(projectId, startDate, endDate);
      setError(null);
      return data;
    } catch (err) {
      console.error('Failed to fetch event count by day:', err);
      setError('Failed to load daily event statistics. Please try again.');
      throw err;
    } finally {
      setLoading(false);
    }
  }, []);

  const fetchDeviceStats = useCallback(async (projectId: string) => {
    try {
      setLoading(true);
      const data = await eventsApi.getDeviceStats(projectId);
      setError(null);
      return data;
    } catch (err) {
      console.error('Failed to fetch device stats:', err);
      setError('Failed to load device statistics. Please try again.');
      throw err;
    } finally {
      setLoading(false);
    }
  }, []);

  return {
    events,
    total,
    loading,
    error,
    fetchEvents,
    fetchEventById,
    fetchEventCountByType,
    fetchEventCountByDay,
    fetchDeviceStats,
  };
} 