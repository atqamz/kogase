using System;

namespace Kogase.Models
{
    /// <summary>
    /// Holds information about the device
    /// </summary>
    [Serializable]
    public class DeviceInfo
    {
        /// <summary>
        /// Unique identifier for the device
        /// </summary>
        public string device_id;

        /// <summary>
        /// Platform the device is running on (iOS, Android, Windows, etc.)
        /// </summary>
        public string platform;

        /// <summary>
        /// OS version of the device
        /// </summary>
        public string os_version;

        /// <summary>
        /// Game version installed on the device
        /// </summary>
        public string app_version;

        /// <summary>
        /// When the app was first installed/run
        /// </summary>
        public string first_seen;

        /// <summary>
        /// Properties associated with this installation
        /// </summary>
        public object properties;
    }

    /// <summary>
    /// Installation event data
    /// </summary>
    [Serializable]
    public class InstallationData
    {
        /// <summary>
        /// Unique identifier for the device
        /// </summary>
        public string device_id;

        /// <summary>
        /// Platform the device is running on (iOS, Android, Windows, etc.)
        /// </summary>
        public string platform;

        /// <summary>
        /// OS version of the device
        /// </summary>
        public string os_version;

        /// <summary>
        /// Game version installed on the device
        /// </summary>
        public string app_version;

        /// <summary>
        /// Properties associated with this installation
        /// </summary>
        public object properties;
    }
} 