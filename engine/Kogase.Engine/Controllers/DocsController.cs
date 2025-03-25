using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Kogase.Engine.Models;
using Kogase.Engine.Extensions;

namespace Kogase.Engine.Controllers;

/// <summary>
/// API documentation examples
/// </summary>
[ApiController]
[Route("api/docs")]
[Produces("application/json")]
public class DocsController : ControllerBase
{
    /// <summary>
    /// Gets API documentation information
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/docs
    ///
    /// </remarks>
    /// <returns>API information message</returns>
    /// <response code="200">Returns the API info message</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public IActionResult GetApiInfo()
    {
        return this.ApiOk(new { Message = "Welcome to Kogase Engine API. Use Swagger UI for full documentation." });
    }

    /// <summary>
    /// Example of an authenticated endpoint
    /// </summary>
    /// <remarks>
    /// This endpoint requires authentication with JWT Bearer token.
    /// 
    /// Sample request:
    ///
    ///     GET /api/docs/authenticated
    ///
    /// </remarks>
    /// <returns>A message confirming authentication</returns>
    /// <response code="200">Returns confirmation message</response>
    /// <response code="401">Unauthorized - if token is invalid or missing</response>
    [HttpGet("authenticated")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetAuthenticatedInfo()
    {
        return this.ApiOk(new { Message = "You are authenticated!" });
    }

    /// <summary>
    /// Example of an API key authenticated endpoint
    /// </summary>
    /// <remarks>
    /// This endpoint requires an API key in the X-API-Key header.
    /// 
    /// Sample request:
    ///
    ///     GET /api/docs/apikey
    ///
    /// </remarks>
    /// <returns>A message confirming API key authentication</returns>
    /// <response code="200">Returns confirmation message</response>
    /// <response code="401">Unauthorized - if API key is invalid or missing</response>
    [HttpGet("apikey")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetApiKeyInfo()
    {
        // The X-API-Key header is validated by the ApiKeyAuthenticationMiddleware
        // If we reach this point, the API key is valid
        return this.ApiOk(new { Message = "Valid API key provided!" });
    }

    /// <summary>
    /// Example of an error response
    /// </summary>
    /// <remarks>
    /// This endpoint always returns an error to demonstrate the error response format.
    /// 
    /// Sample request:
    ///
    ///     GET /api/docs/error
    ///
    /// </remarks>
    /// <returns>An error message</returns>
    /// <response code="400">Returns an error message</response>
    [HttpGet("error")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public IActionResult GetErrorExample()
    {
        return this.ApiError<object>("This is an example error message");
    }
} 