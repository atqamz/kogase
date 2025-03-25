using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Kogase.Engine.Models.DTOs
{
    public class EventBatchRequest
    {
        public List<EventDto> Events { get; set; } = new();
    }

    public class EventDto
    {
        public string EventType { get; set; } = null!;
        public string EventName { get; set; } = null!;
        public DateTime Timestamp { get; set; }
        public string? Parameters { get; set; }
        public string? SessionId { get; set; }
    }

    public class SessionStartRequest
    {
        public string DeviceId { get; set; } = null!;
        public string Platform { get; set; } = null!;
        public string OsVersion { get; set; } = null!;
        public string AppVersion { get; set; } = null!;
        public string? Country { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class SessionEndRequest
    {
        public string SessionId { get; set; } = null!;
        public DateTime Timestamp { get; set; }
        public int DurationSeconds { get; set; }
    }

    public class DeviceRequest
    {
        public string DeviceId { get; set; } = null!;
        public string Platform { get; set; } = null!;
        public string OsVersion { get; set; } = null!;
        public string AppVersion { get; set; } = null!;
        public string? Country { get; set; }
        public DateTime Timestamp { get; set; }
    }
} 