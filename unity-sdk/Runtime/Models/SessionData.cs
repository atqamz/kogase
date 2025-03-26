using System;

namespace Kogase.Models
{
    /// <summary>
    /// Holds information about a user session
    /// </summary>
    [Serializable]
    public class SessionData
    {
        /// <summary>
        /// The ID of the session
        /// </summary>
        public string session_id;

        /// <summary>
        /// Session start time
        /// </summary>
        public DateTime start_time;

        /// <summary>
        /// Session end time
        /// </summary>
        public DateTime? end_time;

        /// <summary>
        /// Session duration in seconds
        /// </summary>
        public float duration;

        /// <summary>
        /// Whether the session is currently active
        /// </summary>
        public bool is_active;
    }
} 