using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Kogase.Engine.Models.DTOs
{
    public class MetricsRequest
    {
        public string MetricType { get; set; } = null!;
        public string Period { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class MetricResponse
    {
        public long Id { get; set; }
        public string MetricType { get; set; } = null!;
        public string Period { get; set; } = null!;
        public DateTime PeriodStart { get; set; }
        public double Value { get; set; }
        public string? Dimensions { get; set; }
    }

    public class MetricSeriesResponse
    {
        public string MetricType { get; set; } = null!;
        public string Period { get; set; } = null!;
        public List<MetricDataPoint> DataPoints { get; set; } = new List<MetricDataPoint>();
    }

    public class MetricDataPoint
    {
        public DateTime Date { get; set; }
        public double Value { get; set; }
        public Dictionary<string, object>? Dimensions { get; set; }
    }

    public class EventsRequest
    {
        public string? EventType { get; set; }
        public string? EventName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 100;
    }

    public class EventsResponse
    {
        public List<EventItem> Events { get; set; } = new List<EventItem>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class EventItem
    {
        public int Id { get; set; }
        public string EventType { get; set; } = null!;
        public string EventName { get; set; } = null!;
        public string? Parameters { get; set; }
        public DateTime Timestamp { get; set; }
        public string? DeviceId { get; set; }
    }

    public class DevicesRequest
    {
        public string? Platform { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 100;
    }

    public class DevicesResponse
    {
        public List<DeviceItem> Devices { get; set; } = new List<DeviceItem>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class DeviceItem
    {
        public int Id { get; set; }
        public string DeviceId { get; set; } = null!;
        public string Platform { get; set; } = null!;
        public string OsVersion { get; set; } = null!;
        public string AppVersion { get; set; } = null!;
        public string? Country { get; set; }
        public DateTime FirstSeen { get; set; }
        public DateTime LastSeen { get; set; }
    }

    public class TopEventsResponse
    {
        public List<EventSummary> Events { get; set; } = new List<EventSummary>();
    }

    public class EventSummary
    {
        public string EventType { get; set; } = null!;
        public string EventName { get; set; } = null!;
        public int Count { get; set; }
    }
} 