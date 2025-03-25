using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kogase.Engine.Data;
using Kogase.Engine.Models;

namespace Kogase.Engine.Controllers
{
    [ApiController]
    [Route("api/v1/analytics")]
    [Authorize]
    public class AnalyticsController : ControllerBase
    {
        private readonly KogaseDbContext _context;

        public AnalyticsController(KogaseDbContext context)
        {
            _context = context;
        }

        [HttpGet("metrics")]
        public async Task<IActionResult> GetMetrics([FromQuery] int projectId, [FromQuery] string? metricType = null, [FromQuery] string? period = null, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
        {
            // Check if user has access to this project
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }
            
            int userId = int.Parse(userIdClaim);
            string? userRole = User.FindFirstValue(ClaimTypes.Role);

            bool hasAccess = userRole == "Admin" || 
                            await _context.Projects.AnyAsync(p => p.Id == projectId && p.OwnerId == userId) || 
                            await _context.ProjectUsers.AnyAsync(pu => pu.ProjectId == projectId && pu.UserId == userId);

            if (!hasAccess)
            {
                return Forbid();
            }

            // Build query
            var query = _context.Metrics.Where(m => m.ProjectId == projectId);

            // Apply filters
            if (!string.IsNullOrEmpty(metricType))
            {
                query = query.Where(m => m.MetricType == metricType);
            }

            if (!string.IsNullOrEmpty(period))
            {
                query = query.Where(m => m.Period == period);
            }

            if (from.HasValue)
            {
                query = query.Where(m => m.PeriodStart >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(m => m.PeriodStart <= to.Value);
            }

            // Get results
            var metrics = await query
                .OrderBy(m => m.PeriodStart)
                .Select(m => new
                {
                    m.Id,
                    m.MetricType,
                    m.Period,
                    m.PeriodStart,
                    m.Value,
                    Dimensions = m.Dimensions
                })
                .ToListAsync();

            return Ok(metrics);
        }

        [HttpGet("events")]
        public async Task<IActionResult> GetEvents([FromQuery] int projectId, [FromQuery] string? eventType = null, [FromQuery] string? eventName = null, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, [FromQuery] int limit = 100, [FromQuery] int offset = 0)
        {
            // Check if user has access to this project
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }
            
            int userId = int.Parse(userIdClaim);
            string? userRole = User.FindFirstValue(ClaimTypes.Role);

            bool hasAccess = userRole == "Admin" || 
                            await _context.Projects.AnyAsync(p => p.Id == projectId && p.OwnerId == userId) || 
                            await _context.ProjectUsers.AnyAsync(pu => pu.ProjectId == projectId && pu.UserId == userId);

            if (!hasAccess)
            {
                return Forbid();
            }

            // Build query
            var query = _context.Events
                .Include(e => e.Device)
                .Where(e => e.ProjectId == projectId);

            // Apply filters
            if (!string.IsNullOrEmpty(eventType))
            {
                query = query.Where(e => e.EventType == eventType);
            }

            if (!string.IsNullOrEmpty(eventName))
            {
                query = query.Where(e => e.EventName == eventName);
            }

            if (from.HasValue)
            {
                query = query.Where(e => e.Timestamp >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(e => e.Timestamp <= to.Value);
            }

            // Get count for pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var events = await query
                .OrderByDescending(e => e.Timestamp)
                .Skip(offset)
                .Take(limit)
                .Select(e => new
                {
                    e.Id,
                    e.EventType,
                    e.EventName,
                    e.Timestamp,
                    Parameters = e.Parameters,
                    Device = e.Device != null ? new
                    {
                        e.Device.DeviceId,
                        e.Device.Platform,
                        e.Device.OsVersion,
                        e.Device.AppVersion,
                        e.Device.Country
                    } : null
                })
                .ToListAsync();

            // Return with pagination info
            return Ok(new
            {
                totalCount,
                offset,
                limit,
                data = events
            });
        }

        [HttpGet("devices")]
        public async Task<IActionResult> GetDevices([FromQuery] int projectId, [FromQuery] string? platform = null, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, [FromQuery] int limit = 100, [FromQuery] int offset = 0)
        {
            // Check if user has access to this project
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }
            
            int userId = int.Parse(userIdClaim);
            string? userRole = User.FindFirstValue(ClaimTypes.Role);

            bool hasAccess = userRole == "Admin" || 
                            await _context.Projects.AnyAsync(p => p.Id == projectId && p.OwnerId == userId) || 
                            await _context.ProjectUsers.AnyAsync(pu => pu.ProjectId == projectId && pu.UserId == userId);

            if (!hasAccess)
            {
                return Forbid();
            }

            // Build query
            var query = _context.Devices.Where(d => d.ProjectId == projectId);

            // Apply filters
            if (!string.IsNullOrEmpty(platform))
            {
                query = query.Where(d => d.Platform == platform);
            }

            if (from.HasValue)
            {
                query = query.Where(d => d.FirstSeen >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(d => d.FirstSeen <= to.Value);
            }

            // Get count for pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var devices = await query
                .OrderByDescending(d => d.LastSeen)
                .Skip(offset)
                .Take(limit)
                .Select(d => new
                {
                    d.Id,
                    d.DeviceId,
                    d.Platform,
                    d.OsVersion,
                    d.AppVersion,
                    d.FirstSeen,
                    d.LastSeen,
                    d.Country
                })
                .ToListAsync();

            // Return with pagination info
            return Ok(new
            {
                totalCount,
                offset,
                limit,
                data = devices
            });
        }
    }
} 