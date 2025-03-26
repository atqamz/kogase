using System;
using UnityEngine;

namespace Kogase.Utils
{
    /// <summary>
    /// Utility class for managing device identification
    /// </summary>
    public static class DeviceIdUtil
    {
        // Keys for PlayerPrefs
        private const string DEVICE_ID_KEY = "kogase_device_id";
        
        /// <summary>
        /// Gets or generates a unique device ID
        /// </summary>
        /// <returns>The unique device ID</returns>
        public static string GetDeviceId()
        {
            // Check if we already have a stored ID
            string deviceId = PlayerPrefs.GetString(DEVICE_ID_KEY, null);
            
            // If not, generate a new one
            if (string.IsNullOrEmpty(deviceId))
            {
                deviceId = GenerateDeviceId();
                PlayerPrefs.SetString(DEVICE_ID_KEY, deviceId);
                PlayerPrefs.Save();
            }
            
            return deviceId;
        }

        /// <summary>
        /// Generates a new unique device ID
        /// </summary>
        /// <returns>A new unique device ID</returns>
        private static string GenerateDeviceId()
        {
            string id = Guid.NewGuid().ToString();

            // On platforms that support it, mix in some device-specific identifiers
            #if UNITY_IOS
                id = $"{id}-{SystemInfo.deviceUniqueIdentifier.Substring(0, 8)}";
            #elif UNITY_ANDROID
                id = $"{id}-{SystemInfo.deviceUniqueIdentifier.Substring(0, 8)}";
            #endif

            return id;
        }

        /// <summary>
        /// Gets information about the current platform
        /// </summary>
        /// <returns>A string representing the platform</returns>
        public static string GetPlatform()
        {
            // Return the platform name based on Application.platform
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    return "Android";
                case RuntimePlatform.IPhonePlayer:
                    return "iOS";
                case RuntimePlatform.WebGLPlayer:
                    return "WebGL";
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    return "Windows";
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.OSXEditor:
                    return "macOS";
                case RuntimePlatform.LinuxPlayer:
                case RuntimePlatform.LinuxEditor:
                    return "Linux";
                default:
                    return Application.platform.ToString();
            }
        }

        /// <summary>
        /// Gets the OS version
        /// </summary>
        /// <returns>The operating system version</returns>
        public static string GetOSVersion()
        {
            return SystemInfo.operatingSystem;
        }

        /// <summary>
        /// Gets the application version
        /// </summary>
        /// <returns>The application version</returns>
        public static string GetAppVersion()
        {
            return Application.version;
        }
    }
} 