using System;
using System.Collections.Generic;
using UnityEngine;
using Kogase.Api;
using Kogase.Utils;
using Kogase.Models;

namespace Kogase.Core
{
    /// <summary>
    /// Main SDK class that provides access to all Kogase functionality
    /// </summary>
    [AddComponentMenu("Kogase/Kogase SDK")]
    public class KogaseSDK : MonoBehaviour
    {
        #region Singleton Implementation
        
        private static KogaseSDK _instance;
        
        /// <summary>
        /// Gets the singleton instance of the KogaseSDK
        /// </summary>
        public static KogaseSDK Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject obj = new GameObject("KogaseSDK");
                    _instance = obj.AddComponent<KogaseSDK>();
                    DontDestroyOnLoad(obj);
                }
                
                return _instance;
            }
        }
        
        #endregion
        
        [Header("Configuration")]
        [Tooltip("The base URL of the Kogase API server")]
        [SerializeField] 
        private string apiUrl = "http://localhost:8080";
        
        [Tooltip("Your Kogase API key")]
        [SerializeField] 
        private string apiKey = "";
        
        [Tooltip("Enable debug logging")]
        [SerializeField] 
        private bool enableDebugLogging = false;
        
        [Tooltip("Automatically track user sessions")]
        [SerializeField] 
        private bool autoTrackSessions = true;
        
        [Tooltip("Enable offline caching of events")]
        [SerializeField] 
        private bool enableOfflineCache = true;
        
        // Internal components
        private KogaseConfig config;
        private KogaseApiClient apiClient;
        private KogaseTelemetry telemetry;
        private SessionManager sessionManager;
        
        // Initialization flag
        private bool isInitialized = false;
        
        /// <summary>
        /// Gets the telemetry component
        /// </summary>
        public KogaseTelemetry Telemetry => telemetry;
        
        /// <summary>
        /// Gets the session manager
        /// </summary>
        public SessionManager SessionManager => sessionManager;
        
        /// <summary>
        /// Gets whether the SDK is initialized
        /// </summary>
        public bool IsInitialized => isInitialized;
        
        /// <summary>
        /// Gets the current SDK configuration
        /// </summary>
        public KogaseConfig Config => config;

        /// <summary>
        /// Initialize the SDK with the provided configuration
        /// </summary>
        /// <param name="apiUrl">Base URL of the Kogase API server</param>
        /// <param name="apiKey">Your Kogase API key</param>
        /// <param name="enableDebugLogging">Enable debug logging</param>
        /// <param name="autoTrackSessions">Automatically track user sessions</param>
        /// <param name="enableOfflineCache">Enable offline caching of events</param>
        public void Initialize(string apiUrl, string apiKey, bool enableDebugLogging = false, 
                              bool autoTrackSessions = true, bool enableOfflineCache = true)
        {
            if (isInitialized)
            {
                Debug.LogWarning("Kogase SDK is already initialized");
                return;
            }

            // Create configuration
            config = new KogaseConfig
            {
                ApiUrl = apiUrl,
                ApiKey = apiKey,
                EnableDebugLogging = enableDebugLogging,
                AutoTrackSessions = autoTrackSessions,
                EnableOfflineCache = enableOfflineCache
            };
            
            InitializeWithConfig(config);
        }

        /// <summary>
        /// Initialize the SDK with the configuration from the MonoBehaviour
        /// </summary>
        public void Initialize()
        {
            if (isInitialized)
            {
                Debug.LogWarning("Kogase SDK is already initialized");
                return;
            }

            // Create configuration from serialized fields
            config = new KogaseConfig
            {
                ApiUrl = apiUrl,
                ApiKey = apiKey,
                EnableDebugLogging = enableDebugLogging,
                AutoTrackSessions = autoTrackSessions,
                EnableOfflineCache = enableOfflineCache
            };
            
            InitializeWithConfig(config);
        }

        /// <summary>
        /// Common initialization with a configuration object
        /// </summary>
        /// <param name="config">The configuration to use</param>
        private void InitializeWithConfig(KogaseConfig config)
        {
            // Create components
            apiClient = new KogaseApiClient(config, this);
            telemetry = new KogaseTelemetry(config, this, apiClient);
            
            // Initialize telemetry
            telemetry.Initialize();
            
            // Create and initialize session manager if auto-tracking is enabled
            if (config.AutoTrackSessions)
            {
                sessionManager = new SessionManager(telemetry, config, this);
                sessionManager.Initialize();
            }
            
            // Record installation (first run)
            if (IsFirstRun())
            {
                RecordFirstRun();
            }
            
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
                Debug.LogWarning("Kogase SDK is not initialized. Use KogaseSDK.Instance.Initialize() first.");
                return;
            }
            
            telemetry.RecordEvent(eventName, parameters);
        }

        /// <summary>
        /// Manually flush events to the server
        /// </summary>
        public void Flush()
        {
            if (!isInitialized)
            {
                Debug.LogWarning("Kogase SDK is not initialized. Use KogaseSDK.Instance.Initialize() first.");
                return;
            }
            
            telemetry.Flush();
        }

        #region Private Methods

        private void Awake()
        {
            // Implement singleton pattern
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            // Clean up if this is the instance
            if (_instance == this)
            {
                _instance = null;
            }
        }

        /// <summary>
        /// Checks if this is the first run of the application
        /// </summary>
        /// <returns>True if this is the first run, false otherwise</returns>
        private bool IsFirstRun()
        {
            const string FIRST_RUN_KEY = "kogase_first_run";
            bool isFirstRun = !PlayerPrefs.HasKey(FIRST_RUN_KEY);
            
            if (isFirstRun)
            {
                PlayerPrefs.SetInt(FIRST_RUN_KEY, 1);
                PlayerPrefs.Save();
            }
            
            return isFirstRun;
        }

        /// <summary>
        /// Records first run/installation event
        /// </summary>
        private void RecordFirstRun()
        {
            Dictionary<string, object> properties = new Dictionary<string, object>
            {
                { "device_model", SystemInfo.deviceModel },
                { "device_name", SystemInfo.deviceName },
                { "processor_type", SystemInfo.processorType },
                { "system_memory_size", SystemInfo.systemMemorySize },
                { "graphics_device_name", SystemInfo.graphicsDeviceName },
                { "graphics_memory_size", SystemInfo.graphicsMemorySize },
                { "screen_resolution", $"{Screen.width}x{Screen.height}" }
            };
            
            telemetry.RecordInstallation(properties);
        }

        #endregion
    }
} 