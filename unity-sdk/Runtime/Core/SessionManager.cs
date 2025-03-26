using System;
using System.Collections;
using UnityEngine;
using Kogase.Models;
using Kogase.Utils;

namespace Kogase.Core
{
    /// <summary>
    /// Manages user sessions for the Kogase SDK
    /// </summary>
    public class SessionManager
    {
        private KogaseTelemetry telemetry;
        private SessionData currentSession;
        private MonoBehaviour coroutineRunner;
        private KogaseConfig config;
        private Coroutine sessionTimeoutCoroutine;
        private bool isApplicationPaused = false;

        // Constants
        private const string SESSION_ID_KEY = "kogase_session_id";
        private const string SESSION_START_KEY = "kogase_session_start";

        /// <summary>
        /// Occurs when a new session begins
        /// </summary>
        public event Action<SessionData> OnSessionStarted;

        /// <summary>
        /// Occurs when the current session ends
        /// </summary>
        public event Action<SessionData> OnSessionEnded;

        /// <summary>
        /// Creates a new instance of the SessionManager
        /// </summary>
        /// <param name="telemetry">Reference to the KogaseTelemetry instance</param>
        /// <param name="config">SDK configuration</param>
        /// <param name="coroutineRunner">MonoBehaviour to run coroutines on</param>
        public SessionManager(KogaseTelemetry telemetry, KogaseConfig config, MonoBehaviour coroutineRunner)
        {
            this.telemetry = telemetry;
            this.config = config;
            this.coroutineRunner = coroutineRunner;
            
            // Register for application lifecycle events
            Application.focusChanged += OnApplicationFocusChanged;
            Application.quitting += OnApplicationQuit;
        }

        /// <summary>
        /// Initialize the session manager
        /// </summary>
        public void Initialize()
        {
            // Try to restore session from PlayerPrefs
            if (TryRestoreSession())
            {
                KogaseLogger.Log("Session restored from previous state");
                
                // Check if the session timed out while the app was closed
                if (HasSessionTimedOut())
                {
                    KogaseLogger.Log("Previous session timed out, starting a new one");
                    EndCurrentSession();
                    StartNewSession();
                }
                else
                {
                    // Resume the existing session
                    RefreshSessionTimeout();
                }
            }
            else
            {
                // Start a new session
                StartNewSession();
            }
        }

        /// <summary>
        /// Gets the current session data
        /// </summary>
        /// <returns>The current session, or null if no session is active</returns>
        public SessionData GetCurrentSession()
        {
            return currentSession;
        }

        /// <summary>
        /// Starts a new user session
        /// </summary>
        public void StartNewSession()
        {
            // Generate a new session ID
            string sessionId = Guid.NewGuid().ToString();
            DateTime now = DateTime.UtcNow;
            
            // Create new session
            currentSession = new SessionData
            {
                session_id = sessionId,
                start_time = now,
                end_time = null,
                duration = 0,
                is_active = true
            };
            
            // Save session to PlayerPrefs
            SaveSessionToPrefs();
            
            // Start the session timeout
            RefreshSessionTimeout();
            
            // Notify listeners
            OnSessionStarted?.Invoke(currentSession);
            
            // Send session start event to server
            SendSessionStartEvent();
        }

        /// <summary>
        /// Ends the current user session
        /// </summary>
        public void EndCurrentSession()
        {
            if (currentSession == null || !currentSession.is_active)
            {
                return;
            }
            
            // Update session data
            currentSession.end_time = DateTime.UtcNow;
            currentSession.duration = (float)(currentSession.end_time.Value - currentSession.start_time).TotalSeconds;
            currentSession.is_active = false;
            
            // Clear session from PlayerPrefs
            PlayerPrefs.DeleteKey(SESSION_ID_KEY);
            PlayerPrefs.DeleteKey(SESSION_START_KEY);
            PlayerPrefs.Save();
            
            // Cancel timeout coroutine
            if (sessionTimeoutCoroutine != null)
            {
                coroutineRunner.StopCoroutine(sessionTimeoutCoroutine);
                sessionTimeoutCoroutine = null;
            }
            
            // Notify listeners
            OnSessionEnded?.Invoke(currentSession);
            
            // Send session end event to server
            SendSessionEndEvent();
        }

        /// <summary>
        /// Refresh the session timeout timer
        /// </summary>
        public void RefreshSessionTimeout()
        {
            if (currentSession == null || !currentSession.is_active)
            {
                return;
            }
            
            // Cancel existing timeout
            if (sessionTimeoutCoroutine != null)
            {
                coroutineRunner.StopCoroutine(sessionTimeoutCoroutine);
            }
            
            // Start new timeout
            sessionTimeoutCoroutine = coroutineRunner.StartCoroutine(SessionTimeoutCoroutine());
        }

        /// <summary>
        /// Handles application focus changes
        /// </summary>
        /// <param name="hasFocus">Whether the application has focus</param>
        private void OnApplicationFocusChanged(bool hasFocus)
        {
            if (hasFocus)
            {
                // Application has regained focus
                if (isApplicationPaused)
                {
                    isApplicationPaused = false;
                    
                    // Check if session timed out
                    if (HasSessionTimedOut())
                    {
                        EndCurrentSession();
                        StartNewSession();
                    }
                    else
                    {
                        RefreshSessionTimeout();
                    }
                }
            }
            else
            {
                // Application has lost focus
                isApplicationPaused = true;
                SaveSessionToPrefs();
            }
        }

        /// <summary>
        /// Handles application quit event
        /// </summary>
        private void OnApplicationQuit()
        {
            // End the session when the application quits
            EndCurrentSession();
        }

        /// <summary>
        /// Checks if the current session has timed out
        /// </summary>
        /// <returns>True if the session has timed out, false otherwise</returns>
        private bool HasSessionTimedOut()
        {
            if (currentSession == null || !currentSession.is_active)
            {
                return false;
            }
            
            float lastActivityTime = PlayerPrefs.GetFloat(SESSION_START_KEY, 0);
            float currentTime = (float)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            float timeSinceLastActivity = currentTime - lastActivityTime;
            
            return timeSinceLastActivity > config.SessionTimeoutSeconds;
        }

        /// <summary>
        /// Saves the current session to PlayerPrefs
        /// </summary>
        private void SaveSessionToPrefs()
        {
            if (currentSession == null || !currentSession.is_active)
            {
                return;
            }
            
            PlayerPrefs.SetString(SESSION_ID_KEY, currentSession.session_id);
            
            // Save the current time as last activity time
            float currentTime = (float)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            PlayerPrefs.SetFloat(SESSION_START_KEY, currentTime);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Tries to restore a session from PlayerPrefs
        /// </summary>
        /// <returns>True if session restored, false otherwise</returns>
        private bool TryRestoreSession()
        {
            string sessionId = PlayerPrefs.GetString(SESSION_ID_KEY, null);
            float sessionStartTime = PlayerPrefs.GetFloat(SESSION_START_KEY, 0);
            
            if (string.IsNullOrEmpty(sessionId) || sessionStartTime <= 0)
            {
                return false;
            }
            
            // Convert timestamp to DateTime
            DateTime startTime = new DateTime(1970, 1, 1).AddSeconds(sessionStartTime);
            
            // Create session object
            currentSession = new SessionData
            {
                session_id = sessionId,
                start_time = startTime,
                end_time = null,
                duration = (float)(DateTime.UtcNow - startTime).TotalSeconds,
                is_active = true
            };
            
            return true;
        }

        /// <summary>
        /// Sends a session start event to the server
        /// </summary>
        private void SendSessionStartEvent()
        {
            if (currentSession == null)
            {
                return;
            }
            
            var eventData = new EventData
            {
                device_id = DeviceIdUtil.GetDeviceId(),
                event_type = "session_start",
                event_name = "session_start",
                parameters = new System.Collections.Generic.Dictionary<string, object>
                {
                    { "session_id", currentSession.session_id }
                },
                timestamp = currentSession.start_time.ToString("o"),
                platform = DeviceIdUtil.GetPlatform(),
                os_version = DeviceIdUtil.GetOSVersion(),
                app_version = DeviceIdUtil.GetAppVersion()
            };
            
            telemetry.RecordEvent(eventData);
        }

        /// <summary>
        /// Sends a session end event to the server
        /// </summary>
        private void SendSessionEndEvent()
        {
            if (currentSession == null || !currentSession.end_time.HasValue)
            {
                return;
            }
            
            var eventData = new EventData
            {
                device_id = DeviceIdUtil.GetDeviceId(),
                event_type = "session_end",
                event_name = "session_end",
                parameters = new System.Collections.Generic.Dictionary<string, object>
                {
                    { "session_id", currentSession.session_id },
                    { "duration", currentSession.duration }
                },
                timestamp = currentSession.end_time.Value.ToString("o"),
                platform = DeviceIdUtil.GetPlatform(),
                os_version = DeviceIdUtil.GetOSVersion(),
                app_version = DeviceIdUtil.GetAppVersion()
            };
            
            telemetry.RecordEvent(eventData);
        }

        /// <summary>
        /// Coroutine that handles session timeout
        /// </summary>
        private IEnumerator SessionTimeoutCoroutine()
        {
            yield return new WaitForSeconds(config.SessionTimeoutSeconds);
            
            KogaseLogger.Log("Session timed out due to inactivity");
            EndCurrentSession();
            StartNewSession();
        }
    }
} 