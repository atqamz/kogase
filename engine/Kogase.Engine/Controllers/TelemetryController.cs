using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Kogase.Engine.Data;
using Kogase.Engine.Models;
using Kogase.Engine.Models.DTOs;

namespace Kogase.Engine.Controllers
{
    [ApiController]
    [Route("api/v1/telemetry")]
    public class TelemetryController : ControllerBase
    {
        private readonly KogaseDbContext _context;
        private readonly ILogger<TelemetryController> _logger;

        public TelemetryController(
            KogaseDbContext context,
            ILogger<TelemetryController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("events")]
        public async Task<IActionResult> LogEvents([FromBody] EventBatchRequest request)
        {
            // Get project ID from API key middleware
            if (!HttpContext.Items.TryGetValue("ProjectId", out var projectIdObj) || projectIdObj is not int projectId)
            {
                return Unauthorized(new { error = "Invalid API key" });
            }

            var events = new List<Event>();

            foreach (var eventDto in request.Events)
            {
                var newEvent = new Event
                {
                    ProjectId = projectId,
                    EventType = eventDto.EventType,
                    EventName = eventDto.EventName,
                    Timestamp = eventDto.Timestamp,
                    Parameters = eventDto.Parameters
                };

                // If there's a session ID, try to link this event to a device
                if (!string.IsNullOrEmpty(eventDto.SessionId))
                {
                    var session = await _context.Sessions
                        .Include(s => s.Device)
                        .FirstOrDefaultAsync(s => s.SessionId == eventDto.SessionId);

                    if (session?.Device != null && session.Device.ProjectId == projectId)
                    {
                        newEvent.DeviceId = session.DeviceId;
                    }
                }

                events.Add(newEvent);
            }

            try
            {
                await _context.Events.AddRangeAsync(events);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Logged {Count} events for project {ProjectId}", events.Count, projectId);
                return Ok(new { count = events.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging events for project {ProjectId}", projectId);
                return StatusCode(500, new { error = "Failed to log events" });
            }
        }

        [HttpPost("session/start")]
        public async Task<IActionResult> StartSession([FromBody] SessionStartRequest request)
        {
            // Get project ID from API key middleware
            if (!HttpContext.Items.TryGetValue("ProjectId", out var projectIdObj) || projectIdObj is not int projectId)
            {
                return Unauthorized(new { error = "Invalid API key" });
            }

            try
            {
                // Look for existing device
                var device = await _context.Devices
                    .FirstOrDefaultAsync(d => d.ProjectId == projectId && d.DeviceId == request.DeviceId);

                // If device doesn't exist, create it
                if (device == null)
                {
                    device = new Device
                    {
                        ProjectId = projectId,
                        DeviceId = request.DeviceId,
                        Platform = request.Platform,
                        OsVersion = request.OsVersion,
                        AppVersion = request.AppVersion,
                        Country = request.Country,
                        FirstSeen = request.Timestamp,
                        LastSeen = request.Timestamp
                    };

                    _context.Devices.Add(device);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    // Update existing device
                    device.LastSeen = request.Timestamp;
                    device.OsVersion = request.OsVersion;
                    device.AppVersion = request.AppVersion;
                    if (request.Country != null)
                    {
                        device.Country = request.Country;
                    }

                    _context.Devices.Update(device);
                }

                // Create new session
                var sessionId = Guid.NewGuid().ToString();
                var session = new Session
                {
                    DeviceId = device.Id,
                    SessionId = sessionId,
                    StartTime = request.Timestamp,
                    EndTime = null,
                    Duration = null
                };

                _context.Sessions.Add(session);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Started session {SessionId} for device {DeviceId} in project {ProjectId}", 
                    sessionId, device.DeviceId, projectId);

                return Ok(new { sessionId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting session for project {ProjectId}", projectId);
                return StatusCode(500, new { error = "Failed to start session" });
            }
        }

        [HttpPost("session/end")]
        public async Task<IActionResult> EndSession([FromBody] SessionEndRequest request)
        {
            // Get project ID from API key middleware
            if (!HttpContext.Items.TryGetValue("ProjectId", out var projectIdObj) || projectIdObj is not int projectId)
            {
                return Unauthorized(new { error = "Invalid API key" });
            }

            try
            {
                // Find the session
                var session = await _context.Sessions
                    .Include(s => s.Device)
                    .FirstOrDefaultAsync(s => s.SessionId == request.SessionId);

                if (session == null)
                {
                    return NotFound(new { error = "Session not found" });
                }

                // Check if this session belongs to a device in this project
                if (session.Device == null || session.Device.ProjectId != projectId)
                {
                    return Unauthorized(new { error = "Invalid API key for this session" });
                }

                // Update session
                session.EndTime = request.Timestamp;
                session.Duration = request.DurationSeconds;

                // Update device
                session.Device.LastSeen = request.Timestamp;
                _context.Devices.Update(session.Device);

                _context.Sessions.Update(session);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Ended session {SessionId} for project {ProjectId} with duration {Duration}s", 
                    session.SessionId, projectId, session.Duration);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ending session for project {ProjectId}", projectId);
                return StatusCode(500, new { error = "Failed to end session" });
            }
        }

        [HttpPost("device")]
        public async Task<IActionResult> TrackDevice([FromBody] DeviceRequest request)
        {
            // Get project ID from API key middleware
            if (!HttpContext.Items.TryGetValue("ProjectId", out var projectIdObj) || projectIdObj is not int projectId)
            {
                return Unauthorized(new { error = "Invalid API key" });
            }

            try
            {
                // Look for existing device
                var device = await _context.Devices
                    .FirstOrDefaultAsync(d => d.ProjectId == projectId && d.DeviceId == request.DeviceId);

                // If device doesn't exist, create it
                if (device == null)
                {
                    device = new Device
                    {
                        ProjectId = projectId,
                        DeviceId = request.DeviceId,
                        Platform = request.Platform,
                        OsVersion = request.OsVersion,
                        AppVersion = request.AppVersion,
                        Country = request.Country,
                        FirstSeen = request.Timestamp,
                        LastSeen = request.Timestamp
                    };

                    _context.Devices.Add(device);
                }
                else
                {
                    // Update existing device info
                    device.LastSeen = request.Timestamp;
                    device.OsVersion = request.OsVersion;
                    device.AppVersion = request.AppVersion;
                    if (request.Country != null)
                    {
                        device.Country = request.Country;
                    }

                    _context.Devices.Update(device);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Tracked device {DeviceId} for project {ProjectId}", 
                    device.DeviceId, projectId);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking device for project {ProjectId}", projectId);
                return StatusCode(500, new { error = "Failed to track device" });
            }
        }
    }
} 