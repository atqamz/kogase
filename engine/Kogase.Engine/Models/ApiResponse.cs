namespace Kogase.Engine.Models;

/// <summary>
/// Generic API response wrapper
/// </summary>
/// <typeparam name="T">The type of data returned</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Whether the request was successful
    /// </summary>
    /// <example>true</example>
    public bool Success { get; set; }
    
    /// <summary>
    /// The data returned by the API
    /// </summary>
    public T? Data { get; set; }
    
    /// <summary>
    /// Error message, if any
    /// </summary>
    /// <example>null</example>
    public string? Message { get; set; }
    
    /// <summary>
    /// Creates a successful response with data
    /// </summary>
    /// <param name="data">The data to include</param>
    /// <returns>A successful API response</returns>
    public static ApiResponse<T> Ok(T data) => new() { Success = true, Data = data };
    
    /// <summary>
    /// Creates an error response with a message
    /// </summary>
    /// <param name="message">The error message</param>
    /// <returns>An error API response</returns>
    public static ApiResponse<T> Error(string message) => new() { Success = false, Message = message };
} 