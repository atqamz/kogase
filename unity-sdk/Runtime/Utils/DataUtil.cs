using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Kogase.Utils
{
    /// <summary>
    /// Utility class for data operations like JSON conversion and local storage
    /// </summary>
    public static class DataUtil
    {
        // Folder name for cached data
        private const string CACHE_FOLDER = "KogaseCache";
        private const string EVENTS_CACHE_FILE = "events.json";

        /// <summary>
        /// Converts an object to JSON string
        /// </summary>
        /// <param name="obj">The object to convert</param>
        /// <returns>JSON string representation</returns>
        public static string ToJson(object obj)
        {
            return JsonUtility.ToJson(obj);
        }

        /// <summary>
        /// Converts JSON string back to an object
        /// </summary>
        /// <typeparam name="T">The type to convert to</typeparam>
        /// <param name="json">The JSON string</param>
        /// <returns>The deserialized object</returns>
        public static T FromJson<T>(string json)
        {
            return JsonUtility.FromJson<T>(json);
        }

        /// <summary>
        /// Saves events to local cache
        /// </summary>
        /// <param name="events">List of events to cache</param>
        /// <returns>True if successful, false otherwise</returns>
        public static bool SaveEventsToCache(List<object> events)
        {
            try
            {
                // Create the cache directory if it doesn't exist
                string cachePath = GetCacheDirectory();
                Directory.CreateDirectory(cachePath);

                // Serialize the events to JSON and write to file
                string filePath = Path.Combine(cachePath, EVENTS_CACHE_FILE);
                string json = ToJson(new { cachedEvents = events, timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() });
                File.WriteAllText(filePath, json);
                
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save events to cache: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Retrieves cached events from local storage
        /// </summary>
        /// <returns>List of cached events, or empty list if none found</returns>
        public static List<object> LoadEventsFromCache()
        {
            try
            {
                string filePath = Path.Combine(GetCacheDirectory(), EVENTS_CACHE_FILE);
                
                // Check if the cache file exists
                if (!File.Exists(filePath))
                {
                    return new List<object>();
                }

                // Read and deserialize the events
                string json = File.ReadAllText(filePath);
                var cachedData = JsonUtility.FromJson<CachedEvents>(json);
                
                return cachedData?.cachedEvents ?? new List<object>();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load events from cache: {ex.Message}");
                return new List<object>();
            }
        }

        /// <summary>
        /// Clears the cached events
        /// </summary>
        public static void ClearEventCache()
        {
            try
            {
                string filePath = Path.Combine(GetCacheDirectory(), EVENTS_CACHE_FILE);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to clear event cache: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the path to the cache directory
        /// </summary>
        /// <returns>The full path to the cache directory</returns>
        private static string GetCacheDirectory()
        {
            return Path.Combine(Application.persistentDataPath, CACHE_FOLDER);
        }

        // Helper class for caching events
        [Serializable]
        private class CachedEvents
        {
            public List<object> cachedEvents;
            public long timestamp;
        }
    }
} 