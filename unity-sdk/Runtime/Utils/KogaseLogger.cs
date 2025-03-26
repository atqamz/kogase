using System;
using UnityEngine;

namespace Kogase.Utils
{
    /// <summary>
    /// Logger utility for the Kogase SDK
    /// </summary>
    public static class KogaseLogger
    {
        // Flag to enable/disable logging
        private static bool loggingEnabled = false;

        /// <summary>
        /// Enables or disables logging
        /// </summary>
        /// <param name="enabled">Whether logging should be enabled</param>
        public static void SetLoggingEnabled(bool enabled)
        {
            loggingEnabled = enabled;
        }

        /// <summary>
        /// Logs an informational message
        /// </summary>
        /// <param name="message">The message to log</param>
        public static void Log(string message)
        {
            if (loggingEnabled)
            {
                Debug.Log($"[Kogase] {message}");
            }
        }

        /// <summary>
        /// Logs a warning message
        /// </summary>
        /// <param name="message">The warning message</param>
        public static void LogWarning(string message)
        {
            if (loggingEnabled)
            {
                Debug.LogWarning($"[Kogase] {message}");
            }
        }

        /// <summary>
        /// Logs an error message
        /// </summary>
        /// <param name="message">The error message</param>
        public static void LogError(string message)
        {
            if (loggingEnabled)
            {
                Debug.LogError($"[Kogase] {message}");
            }
        }

        /// <summary>
        /// Logs an exception
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="context">Optional context message</param>
        public static void LogException(Exception exception, string context = null)
        {
            if (loggingEnabled)
            {
                string message = string.IsNullOrEmpty(context) 
                    ? $"[Kogase] Exception: {exception.Message}"
                    : $"[Kogase] Exception in {context}: {exception.Message}";
                
                Debug.LogError(message);
                Debug.LogException(exception);
            }
        }
    }
} 