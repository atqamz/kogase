using System.Collections.Generic;
using UnityEngine;
using Kogase.Core;

namespace Kogase.Samples
{
    /// <summary>
    /// Sample script showing how to use the Kogase SDK
    /// </summary>
    public class KogaseSDKSample : MonoBehaviour
    {
        // API URL and key can be configured in the inspector
        [SerializeField] private string apiUrl = "http://localhost:8080";
        [SerializeField] private string apiKey = "";
        
        // Sample event name for demo
        [SerializeField] private string eventName = "demo_event";
        
        private void Start()
        {
            // Initialize the SDK if not already initialized
            if (!KogaseSDK.Instance.IsInitialized)
            {
                KogaseSDK.Instance.Initialize(apiUrl, apiKey);
                Debug.Log("Kogase SDK initialized");
            }
        }
        
        /// <summary>
        /// Record a simple event with no parameters
        /// </summary>
        public void RecordSimpleEvent()
        {
            KogaseSDK.Instance.RecordEvent(eventName);
            Debug.Log($"Recorded simple event: {eventName}");
        }
        
        /// <summary>
        /// Record an event with parameters
        /// </summary>
        public void RecordEventWithParameters()
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "level", 1 },
                { "score", 100 },
                { "time", Time.time },
                { "difficulty", "easy" }
            };
            
            KogaseSDK.Instance.RecordEvent(eventName, parameters);
            Debug.Log($"Recorded event with parameters: {eventName}");
        }
        
        /// <summary>
        /// Manually flush events to the server
        /// </summary>
        public void FlushEvents()
        {
            KogaseSDK.Instance.Flush();
            Debug.Log("Manually flushed events");
        }
        
        #region UI Event Handlers
        
        // These methods can be connected to UI buttons in the Unity Editor
        
        public void OnButtonClick()
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "button_name", "sample_button" },
                { "screen", "main_menu" }
            };
            
            KogaseSDK.Instance.RecordEvent("button_click", parameters);
        }
        
        public void OnLevelStart(int levelNumber)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "level", levelNumber },
                { "timestamp", System.DateTime.UtcNow.ToString("o") }
            };
            
            KogaseSDK.Instance.RecordEvent("level_start", parameters);
        }
        
        public void OnLevelComplete(int levelNumber, int score, float timeSpent)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "level", levelNumber },
                { "score", score },
                { "time_spent", timeSpent },
                { "timestamp", System.DateTime.UtcNow.ToString("o") }
            };
            
            KogaseSDK.Instance.RecordEvent("level_complete", parameters);
        }
        
        #endregion
    }
} 