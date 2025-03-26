using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;

namespace Kogase.Core
{
    public class KogaseSDK
    {
        private static KogaseSDK _instance;
        private string _apiUrl;
        private string _apiKey;
        private string _deviceId;
        private Queue<Dictionary<string, object>> _eventQueue;
        private bool _isInitialized;
        private const int MAX_QUEUE_SIZE = 100;
        private const float AUTO_FLUSH_INTERVAL = 60f; // Seconds
        private float _lastFlushTime;

        public static KogaseSDK Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new KogaseSDK();
                }
                return _instance;
            }
        }

        private KogaseSDK()
        {
            _eventQueue = new Queue<Dictionary<string, object>>();
            _isInitialized = false;
        }

        public void Initialize(string apiUrl, string apiKey)
        {
            _apiUrl = apiUrl.TrimEnd('/');
            _apiKey = apiKey;
            _deviceId = SystemInfo.deviceUniqueIdentifier;
            _isInitialized = true;
            
            Debug.Log($"Kogase SDK initialized with API URL: {_apiUrl}");
            
            // Record installation event
            RecordInstallation();
        }

        private void CheckInitialization()
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("Kogase SDK is not initialized. Call Initialize() first.");
            }
        }

        public void RecordEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            CheckInitialization();

            var eventData = new Dictionary<string, object>
            {
                { "event_name", eventName },
                { "timestamp", DateTime.UtcNow.ToString("o") },
                { "device_id", _deviceId },
                { "platform", Application.platform.ToString() },
                { "os_version", SystemInfo.operatingSystem },
                { "sdk_version", "1.0.0" }
            };

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    eventData[$"params_{param.Key}"] = param.Value;
                }
            }

            _eventQueue.Enqueue(eventData);

            if (_eventQueue.Count >= MAX_QUEUE_SIZE)
            {
                FlushEvents();
            }
        }

        private void RecordInstallation()
        {
            var installParams = new Dictionary<string, object>
            {
                { "app_version", Application.version },
                { "device_model", SystemInfo.deviceModel },
                { "device_type", SystemInfo.deviceType.ToString() },
                { "processor_type", SystemInfo.processorType },
                { "system_memory_size", SystemInfo.systemMemorySize },
                { "graphics_device_name", SystemInfo.graphicsDeviceName },
                { "graphics_memory_size", SystemInfo.graphicsMemorySize }
            };

            RecordEvent("app_install", installParams);
        }

        public async void FlushEvents()
        {
            CheckInitialization();

            if (_eventQueue.Count == 0)
            {
                return;
            }

            var events = new List<Dictionary<string, object>>();
            while (_eventQueue.Count > 0)
            {
                events.Add(_eventQueue.Dequeue());
            }

            var json = JsonUtility.ToJson(new { events = events });
            var request = new UnityWebRequest($"{_apiUrl}/api/v1/events", "POST");
            
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("X-API-Key", _apiKey);

            try
            {
                var operation = request.SendWebRequest();
                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"Successfully sent {events.Count} events to Kogase server");
                }
                else
                {
                    Debug.LogError($"Failed to send events to Kogase server: {request.error}");
                    // Re-queue failed events
                    foreach (var evt in events)
                    {
                        _eventQueue.Enqueue(evt);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception while sending events to Kogase server: {ex.Message}");
                // Re-queue failed events
                foreach (var evt in events)
                {
                    _eventQueue.Enqueue(evt);
                }
            }
            finally
            {
                request.Dispose();
            }
        }

        public void Update()
        {
            if (!_isInitialized)
            {
                return;
            }

            float currentTime = Time.realtimeSinceStartup;
            if (currentTime - _lastFlushTime >= AUTO_FLUSH_INTERVAL)
            {
                FlushEvents();
                _lastFlushTime = currentTime;
            }
        }
    }
} 