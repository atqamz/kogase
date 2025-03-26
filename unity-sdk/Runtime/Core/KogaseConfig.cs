using System;
using UnityEngine;

namespace Kogase.Core
{
    /// <summary>
    /// Configuration for the Kogase SDK
    /// </summary>
    [Serializable]
    public class KogaseConfig
    {
        /// <summary>
        /// The base URL of the Kogase API server
        /// </summary>
        public string ApiUrl = "http://localhost:8080";

        /// <summary>
        /// API key for the project
        /// </summary>
        public string ApiKey = "";

        /// <summary>
        /// API version path
        /// </summary>
        public string ApiVersion = "v1";

        /// <summary>
        /// Maximum number of cached events before auto-sending
        /// </summary>
        public int MaxCachedEvents = 20;

        /// <summary>
        /// Whether to enable logging for debug purposes
        /// </summary>
        public bool EnableDebugLogging = false;

        /// <summary>
        /// Whether to automatically track sessions
        /// </summary>
        public bool AutoTrackSessions = true;

        /// <summary>
        /// Whether to automatically cache events when offline and send when back online
        /// </summary>
        public bool EnableOfflineCache = true;

        /// <summary>
        /// Maximum age (in seconds) of cached events before they're discarded
        /// </summary>
        public int MaxEventAgeSeconds = 7 * 24 * 60 * 60; // 1 week

        /// <summary>
        /// Automatic session timeout (in seconds) - after this time of inactivity, 
        /// a new session will be started
        /// </summary>
        public int SessionTimeoutSeconds = 30 * 60; // 30 minutes

        /// <summary>
        /// Gets the full API URL for a specific endpoint
        /// </summary>
        /// <param name="endpoint">The API endpoint path</param>
        /// <returns>The full URL for the endpoint</returns>
        public string GetApiUrl(string endpoint)
        {
            return $"{ApiUrl}/api/{ApiVersion}/{endpoint}";
        }

        /// <summary>
        /// Clone the configuration
        /// </summary>
        /// <returns>A new instance with the same values</returns>
        public KogaseConfig Clone()
        {
            return new KogaseConfig
            {
                ApiUrl = this.ApiUrl,
                ApiKey = this.ApiKey,
                ApiVersion = this.ApiVersion,
                MaxCachedEvents = this.MaxCachedEvents,
                EnableDebugLogging = this.EnableDebugLogging,
                AutoTrackSessions = this.AutoTrackSessions,
                EnableOfflineCache = this.EnableOfflineCache,
                MaxEventAgeSeconds = this.MaxEventAgeSeconds,
                SessionTimeoutSeconds = this.SessionTimeoutSeconds
            };
        }
    }
} 