using Microsoft.AspNetCore.Mvc;
using Kogase.Engine.Models;

namespace Kogase.Engine.Extensions;

/// <summary>
/// Extensions for API controllers to standardize responses
/// </summary>
public static class ControllerExtensions
{
    /// <summary>
    /// Returns a standardized successful response
    /// </summary>
    /// <typeparam name="T">Type of data being returned</typeparam>
    /// <param name="controller">The controller instance</param>
    /// <param name="data">The data to return</param>
    /// <returns>An OK result with standardized response format</returns>
    public static IActionResult ApiOk<T>(this ControllerBase controller, T data)
    {
        return controller.Ok(ApiResponse<T>.Ok(data));
    }

    /// <summary>
    /// Returns a standardized error response
    /// </summary>
    /// <typeparam name="T">Type of data being returned (usually object)</typeparam>
    /// <param name="controller">The controller instance</param>
    /// <param name="message">The error message</param>
    /// <param name="statusCode">HTTP status code (default: 400 Bad Request)</param>
    /// <returns>A result with the specified status code and error message</returns>
    public static IActionResult ApiError<T>(this ControllerBase controller, string message, int statusCode = StatusCodes.Status400BadRequest)
    {
        return controller.StatusCode(statusCode, ApiResponse<T>.Error(message));
    }
} 