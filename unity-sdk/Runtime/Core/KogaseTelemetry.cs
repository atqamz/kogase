using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kogase.Api;
using Kogase.Models;
using Kogase.Utils;

namespace Kogase.Core
{
    /// <summary>
    /// Main telemetry class for the Kogase SDK
    /// </summary>
    public class KogaseTelemetry
    {
        private KogaseConfig config;
        private KogaseApiClient apiClient;
        private List<EventData> eventQueue = new List<EventData>();
        private MonoBehaviour coroutineRunner;
        private bool isInitialized = false;
        private bool isFlushPending = false;

        /// <summary>
        /// Creates a new instance of the KogaseTelemetry class
        /// </summary>
        /// <param name="config">Configuration for the SDK</param>
        /// <param name="coroutineRunner">MonoBehaviour to run coroutines on</param>
        /// <param name="apiClient">API client for server communication</param>
        public KogaseTelemetry(KogaseConfig config, MonoBehaviour coroutineRunner, KogaseApiClient apiClient)
        {
            this.config = config;
            this.coroutineRunner = coroutineRunner;
            this.apiClient = apiClient;
        }

        /// <summary>
        /// Initializes the telemetry system
        /// </summary>
        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            KogaseLogger.SetLoggingEnabled(config.EnableDebugLogging);
            KogaseLogger.Log("Initializing Kogase Telemetry SDK");

            // If offline caching is enabled, attempt to load and process cached events
            if (config.EnableOfflineCache)
            {
                coroutineRunner.StartCoroutine(LoadAndProcessCachedEvents());
            }

            // Register application lifecycle events for auto-flushing
            Application.focusChanged += OnApplicationFocusChanged;
            Application.quitting += OnApplicationQuit;

            isInitialized = true;
        }

        /// <summary>
        /// Records a custom event
        /// </summary>
        /// <param name="eventName">Name of the event</param>
        /// <param name="parameters">Optional parameters for the event</param>
        public void RecordEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            if (!isInitialized)
            {
                KogaseLogger.LogWarning("Kogase Telemetry is not initialized. Event will not be recorded.");
                return;
            }

            EventData eventData = new EventData
            {
                device_id = DeviceIdUtil.GetDeviceId(),
                event_type = "custom",
                event_name = eventName,
                parameters = parameters ?? new Dictionary<string, object>(),
                timestamp = DateTime.UtcNow.ToString("o"),
                platform = DeviceIdUtil.GetPlatform(),
                os_version = DeviceIdUtil.GetOSVersion(),
                app_version = DeviceIdUtil.GetAppVersion()
            };

            RecordEvent(eventData);
        }

        /// <summary>
        /// Records an event with the specified data
        /// </summary>
        /// <param name="eventData">The event data to record</param>
        public void RecordEvent(EventData eventData)
        {
            if (!isInitialized)
            {
                KogaseLogger.LogWarning("Kogase Telemetry is not initialized. Event will not be recorded.");
                return;
            }

            // Add to queue
            eventQueue.Add(eventData);
            KogaseLogger.Log($"Event queued: {eventData.event_name}");

            // Automatically flush if we hit the threshold
            if (eventQueue.Count >= config.MaxCachedEvents)
            {
                Flush();
            }
        }

        /// <summary>
        /// Records an installation event
        /// </summary>
        /// <param name="properties">Optional properties to include with the installation event</param>
        public void RecordInstallation(Dictionary<string, object> properties = null)
        {
            if (!isInitialized)
            {
                KogaseLogger.LogWarning("Kogase Telemetry is not initialized. Installation will not be recorded.");
                return;
            }

            InstallationData installData = new InstallationData
            {
                device_id = DeviceIdUtil.GetDeviceId(),
                platform = DeviceIdUtil.GetPlatform(),
                os_version = DeviceIdUtil.GetOSVersion(),
                app_version = DeviceIdUtil.GetAppVersion(),
                properties = properties
            };

            apiClient.RecordInstallation(installData, (response) => {
                if (response.IsSuccess())
                {
                    KogaseLogger.Log("Installation event successfully recorded");
                }
                else
                {
                    KogaseLogger.LogError($"Failed to record installation: {response.error}");
                    
                    // Cache the installation event if offline caching is enabled
                    if (config.EnableOfflineCache)
                    {
                        // Convert installation to event data to cache it
                        EventData eventData = new EventData
                        {
                            device_id = installData.device_id,
                            event_type = "install",
                            event_name = "installation",
                            parameters = properties ?? new Dictionary<string, object>(),
                            timestamp = DateTime.UtcNow.ToString("o"),
                            platform = installData.platform,
                            os_version = installData.os_version,
                            app_version = installData.app_version
                        };
                        
                        CacheEvent(eventData);
                    }
                }
            });
        }

        /// <summary>
        /// Flushes the event queue, sending all queued events to the server
        /// </summary>
        public void Flush()
        {
            if (!isInitialized || eventQueue.Count == 0 || isFlushPending)
            {
                return;
            }

            isFlushPending = true;
            List<EventData> eventsToSend = new List<EventData>(eventQueue);
            eventQueue.Clear();

            KogaseLogger.Log($"Flushing {eventsToSend.Count} events to server");

            apiClient.SendEvents(eventsToSend, (response) => {
                isFlushPending = false;

                if (response.IsSuccess())
                {
                    KogaseLogger.Log($"Successfully sent {response.count} events");
                }
                else
                {
                    KogaseLogger.LogError($"Failed to send events: {response.error}");
                    
                    // If caching is enabled, save the failed events
                    if (config.EnableOfflineCache)
                    {
                        KogaseLogger.Log("Caching failed events for later retry");
                        foreach (var eventData in eventsToSend)
                        {
                            CacheEvent(eventData);
                        }
                    }
                    else
                    {
                        // If caching is disabled, put the events back in the queue
                        foreach (var eventData in eventsToSend)
                        {
                            eventQueue.Add(eventData);
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Handles application focus changes
        /// </summary>
        /// <param name="hasFocus">Whether the application has focus</param>
        private void OnApplicationFocusChanged(bool hasFocus)
        {
            // Flush events when application loses focus
            if (!hasFocus)
            {
                Flush();
            }
            else if (config.EnableOfflineCache)
            {
                // Try to send cached events when application regains focus
                coroutineRunner.StartCoroutine(ProcessCachedEvents());
            }
        }

        /// <summary>
        /// Handles application quit event
        /// </summary>
        private void OnApplicationQuit()
        {
            // Flush events when application quits
            Flush();
        }

        /// <summary>
        /// Caches an event for later sending
        /// </summary>
        /// <param name="eventData">The event to cache</param>
        private void CacheEvent(EventData eventData)
        {
            if (!config.EnableOfflineCache)
            {
                return;
            }

            // Load existing cached events
            List<object> cachedEvents = DataUtil.LoadEventsFromCache() as List<object> ?? new List<object>();
            
            // Add new event
            cachedEvents.Add(eventData);
            
            // Save updated cache
            DataUtil.SaveEventsToCache(cachedEvents);
        }

        /// <summary>
        /// Loads and processes cached events
        /// </summary>
        private IEnumerator LoadAndProcessCachedEvents()
        {
            // Wait a bit to let the app initialize fully
            yield return new WaitForSeconds(2f);
            
            yield return ProcessCachedEvents();
        }

        /// <summary>
        /// Processes cached events, sending them to the server
        /// </summary>
        private IEnumerator ProcessCachedEvents()
        {
            List<object> cachedEvents = DataUtil.LoadEventsFromCache();
            if (cachedEvents.Count == 0)
            {
                yield break;
            }

            KogaseLogger.Log($"Processing {cachedEvents.Count} cached events");

            // Convert to EventData list
            List<EventData> events = new List<EventData>();
            foreach (object eventObj in cachedEvents)
            {
                if (eventObj is EventData eventData)
                {
                    events.Add(eventData);
                }
            }

            if (events.Count > 0)
            {
                bool[] success = new bool[1]; // Use array to allow modification in lambda
                
                apiClient.SendEvents(events, (response) => {
                    if (response.IsSuccess())
                    {
                        KogaseLogger.Log($"Successfully sent {response.count} cached events");
                        DataUtil.ClearEventCache();
                        success[0] = true;
                    }
                    else
                    {
                        KogaseLogger.LogError($"Failed to send cached events: {response.error}");
                        success[0] = false;
                    }
                });
                
                // Wait for the API call to complete
                float timeout = 10f;
                float elapsed = 0f;
                while (elapsed < timeout && success[0] == false)
                {
                    yield return new WaitForSeconds(0.5f);
                    elapsed += 0.5f;
                }
            }
        }
    }
} 