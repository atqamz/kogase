using UnityEngine;
using UnityEditor;
using Kogase.Core;

namespace Kogase.Editor
{
    /// <summary>
    /// Custom editor for the KogaseSDK component
    /// </summary>
    [CustomEditor(typeof(KogaseSDK))]
    public class KogaseSDKEditor : UnityEditor.Editor
    {
        private SerializedProperty apiUrlProperty;
        private SerializedProperty apiKeyProperty;
        private SerializedProperty enableDebugLoggingProperty;
        private SerializedProperty autoTrackSessionsProperty;
        private SerializedProperty enableOfflineCacheProperty;

        private void OnEnable()
        {
            apiUrlProperty = serializedObject.FindProperty("apiUrl");
            apiKeyProperty = serializedObject.FindProperty("apiKey");
            enableDebugLoggingProperty = serializedObject.FindProperty("enableDebugLogging");
            autoTrackSessionsProperty = serializedObject.FindProperty("autoTrackSessions");
            enableOfflineCacheProperty = serializedObject.FindProperty("enableOfflineCache");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Kogase SDK Configuration", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.PropertyField(apiUrlProperty, new GUIContent("API URL", "The base URL of the Kogase API server"));

            EditorGUILayout.PropertyField(apiKeyProperty, new GUIContent("API Key", "Your Kogase API key"));

            if (string.IsNullOrEmpty(apiKeyProperty.stringValue))
            {
                EditorGUILayout.HelpBox("An API key is required to use the Kogase SDK. Please enter your API key.", MessageType.Warning);
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.PropertyField(enableDebugLoggingProperty, new GUIContent("Enable Debug Logging", "Enable logging for debugging"));
            EditorGUILayout.PropertyField(autoTrackSessionsProperty, new GUIContent("Auto-Track Sessions", "Automatically track user sessions"));
            EditorGUILayout.PropertyField(enableOfflineCacheProperty, new GUIContent("Enable Offline Cache", "Cache events when offline and send them later"));

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox(
                "The Kogase SDK will initialize automatically when your game starts. " +
                "To manually initialize it, call KogaseSDK.Instance.Initialize() in your code.", 
                MessageType.Info);

            if (GUILayout.Button("Add Auto-Initialize Script"))
            {
                CreateAutoInitScript();
            }
        }

        private void CreateAutoInitScript()
        {
            // Check if the script already exists
            string autoInitPath = "Assets/Scripts/KogaseAutoInit.cs";
            bool fileExists = System.IO.File.Exists(autoInitPath);

            // Create the directory if it doesn't exist
            string dirPath = System.IO.Path.GetDirectoryName(autoInitPath);
            if (!System.IO.Directory.Exists(dirPath))
            {
                System.IO.Directory.CreateDirectory(dirPath);
            }

            // Create the script if it doesn't exist
            if (!fileExists)
            {
                string script = @"
using UnityEngine;
using Kogase.Core;

/// <summary>
/// Automatically initializes the Kogase SDK on application start
/// </summary>
public class KogaseAutoInit : MonoBehaviour
{
    private void Awake()
    {
        // Initialize the SDK
        KogaseSDK.Instance.Initialize();
        
        // This GameObject is no longer needed
        Destroy(gameObject);
    }
}
                ";
                System.IO.File.WriteAllText(autoInitPath, script);
                AssetDatabase.Refresh();
                Debug.Log("Auto-initialize script created at " + autoInitPath);
            }
            else
            {
                Debug.Log("Auto-initialize script already exists at " + autoInitPath);
            }

            // Create a GameObject with the auto-init script in the current scene
            GameObject go = new GameObject("KogaseAutoInit");
            go.AddComponent<MonoBehaviour>(); // This will be replaced with KogaseAutoInit once compiled
            Selection.activeGameObject = go;
            EditorGUIUtility.PingObject(go);

            Debug.Log("Added KogaseAutoInit GameObject to the scene. The SDK will initialize automatically when the scene starts.");
        }
    }
} 