using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Kogase.Core;
using Kogase.Models;
using Kogase.Utils;

namespace Kogase.Api
{
    /// <summary>
    /// API client for communicating with the Kogase server
    /// </summary>
    public class KogaseApiClient
    {
        private KogaseConfig config;
        private MonoBehaviour coroutineRunner;

        /// <summary>
        /// Creates a new instance of the KogaseApiClient
        /// </summary>
        /// <param name="config">Configuration for the SDK</param>
        /// <param name="coroutineRunner">MonoBehaviour to run coroutines on</param>
        public KogaseApiClient(KogaseConfig config, MonoBehaviour coroutineRunner)
        {
            this.config = config;
            this.coroutineRunner = coroutineRunner;
        }

        /// <summary>
        /// Sends a single event to the server
        /// </summary>
        /// <param name="eventData">The event data to send</param>
        /// <param name="callback">Callback with the API response</param>
        public void SendEvent(EventData eventData, Action<ApiResponse> callback = null)
        {
            string url = config.GetApiUrl("sdk/event");
            string json = JsonUtility.ToJson(eventData);
            coroutineRunner.StartCoroutine(PostRequest(url, json, callback));
        }

        /// <summary>
        /// Sends multiple events in a batch to the server
        /// </summary>
        /// <param name="events">List of events to send</param>
        /// <param name="callback">Callback with the API response</param>
        public void SendEvents(List<EventData> events, Action<CountResponse> callback = null)
        {
            string url = config.GetApiUrl("sdk/events");
            string json = JsonUtility.ToJson(new EventBatch { events = events });
            coroutineRunner.StartCoroutine(PostRequest<CountResponse>(url, json, callback));
        }

        /// <summary>
        /// Sends a session start event to the server
        /// </summary>
        /// <param name="eventData">The session start event data</param>
        /// <param name="callback">Callback with the API response</param>
        public void StartSession(EventData eventData, Action<ApiResponse> callback = null)
        {
            string url = config.GetApiUrl("sdk/session/start");
            string json = JsonUtility.ToJson(eventData);
            coroutineRunner.StartCoroutine(PostRequest(url, json, callback));
        }

        /// <summary>
        /// Sends a session end event to the server
        /// </summary>
        /// <param name="eventData">The session end event data</param>
        /// <param name="callback">Callback with the API response</param>
        public void EndSession(EventData eventData, Action<ApiResponse> callback = null)
        {
            string url = config.GetApiUrl("sdk/session/end");
            string json = JsonUtility.ToJson(eventData);
            coroutineRunner.StartCoroutine(PostRequest(url, json, callback));
        }

        /// <summary>
        /// Sends an installation event to the server
        /// </summary>
        /// <param name="installData">The installation data</param>
        /// <param name="callback">Callback with the API response</param>
        public void RecordInstallation(InstallationData installData, Action<ApiResponse> callback = null)
        {
            string url = config.GetApiUrl("sdk/installation");
            string json = JsonUtility.ToJson(installData);
            coroutineRunner.StartCoroutine(PostRequest(url, json, callback));
        }

        /// <summary>
        /// Makes a POST request to the API
        /// </summary>
        /// <typeparam name="T">Type of response expected</typeparam>
        /// <param name="url">The URL to send the request to</param>
        /// <param name="jsonData">The JSON data to send</param>
        /// <param name="callback">Callback function for the response</param>
        /// <returns>IEnumerator for coroutine</returns>
        private IEnumerator PostRequest<T>(string url, string jsonData, Action<T> callback = null) where T : ApiResponse, new()
        {
            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("X-Kogase-API-Key", config.ApiKey);

                // Send the request
                yield return request.SendWebRequest();

                T response = new T();
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(request.downloadHandler.text))
                        {
                            // Parse the response
                            response = JsonUtility.FromJson<T>(request.downloadHandler.text);
                        }
                        else
                        {
                            // Empty response but successful request
                            response.status = "success";
                        }
                    }
                    catch (Exception ex)
                    {
                        KogaseLogger.LogException(ex, "parsing API response");
                        response.error = "Failed to parse API response";
                    }
                }
                else
                {
                    // Set error information
                    response.error = request.error;
                    KogaseLogger.LogError($"API request failed: {request.error}");
                }

                // Invoke the callback with the response
                callback?.Invoke(response);
            }
        }

        /// <summary>
        /// Makes a POST request expecting a standard ApiResponse
        /// </summary>
        /// <param name="url">The URL to send the request to</param>
        /// <param name="jsonData">The JSON data to send</param>
        /// <param name="callback">Callback function for the response</param>
        /// <returns>IEnumerator for coroutine</returns>
        private IEnumerator PostRequest(string url, string jsonData, Action<ApiResponse> callback = null)
        {
            yield return PostRequest<ApiResponse>(url, jsonData, callback);
        }
    }
} 