using UnityEngine;
using Kogase.Core;

namespace Kogase
{
    public class KogaseManager : MonoBehaviour
    {
        [SerializeField]
        private string apiUrl = "http://localhost:8080";
        
        [SerializeField]
        private string apiKey = "";

        private void Awake()
        {
            // Ensure the GameObject persists across scenes
            DontDestroyOnLoad(gameObject);

            // Initialize the SDK
            KogaseSDK.Instance.Initialize(apiUrl, apiKey);
        }

        private void Update()
        {
            // Call the SDK's update method to handle automatic event flushing
            KogaseSDK.Instance.Update();
        }

        private void OnApplicationQuit()
        {
            // Ensure all remaining events are sent before quitting
            KogaseSDK.Instance.FlushEvents();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                // Flush events when the application is paused (e.g., mobile app going to background)
                KogaseSDK.Instance.FlushEvents();
            }
        }
    }
} 