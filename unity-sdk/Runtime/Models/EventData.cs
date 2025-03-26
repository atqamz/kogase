using System;
using System.Collections.Generic;

namespace Kogase.Models
{
    /// <summary>
    /// Represents a telemetry event to be sent to the Kogase backend
    /// </summary>
    [Serializable]
    public class EventData
    {
        /// <summary>
        /// The device ID of the client
        /// </summary>
        public string device_id;

        /// <summary>
        /// The type of event (session_start, session_end, custom, etc.)
        /// </summary>
        public string event_type;

        /// <summary>
        /// The name of the event (particularly for custom events)
        /// </summary>
        public string event_name;

        /// <summary>
        /// Additional parameters/properties for the event
        /// </summary>
        public Dictionary<string, object> parameters;

        /// <summary>
        /// The timestamp when the event occurred (optional, defaults to server time)
        /// </summary>
        public string timestamp;

        /// <summary>
        /// The platform the event occurred on (iOS, Android, Windows, etc.)
        /// </summary>
        public string platform;

        /// <summary>
        /// The OS version of the client
        /// </summary>
        public string os_version;

        /// <summary>
        /// The application/game version
        /// </summary>
        public string app_version;
    }

    /// <summary>
    /// Container for a batch of events
    /// </summary>
    [Serializable]
    public class EventBatch
    {
        /// <summary>
        /// List of events to send in a batch
        /// </summary>
        public List<EventData> events;
    }
} 