using System;

namespace Kogase.Models
{
    /// <summary>
    /// Base API response structure
    /// </summary>
    [Serializable]
    public class ApiResponse
    {
        /// <summary>
        /// Success or error message
        /// </summary>
        public string message;

        /// <summary>
        /// Error message if request failed
        /// </summary>
        public string error;

        /// <summary>
        /// Status of the response
        /// </summary>
        public string status;

        /// <summary>
        /// Checks if the response indicates success
        /// </summary>
        /// <returns>True if the response is successful, false otherwise</returns>
        public bool IsSuccess()
        {
            return string.IsNullOrEmpty(error) && 
                  (status == "success" || !string.IsNullOrEmpty(message));
        }
    }

    /// <summary>
    /// API response with count information (for batch operations)
    /// </summary>
    [Serializable]
    public class CountResponse : ApiResponse
    {
        /// <summary>
        /// Number of items processed
        /// </summary>
        public int count;
    }
} 