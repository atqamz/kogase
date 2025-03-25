using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kogase.Engine.Data;
using Kogase.Engine.Models;
using Kogase.Engine.Models.DTOs;
using Kogase.Engine.Services;

namespace Kogase.Engine.Controllers
{
    [ApiController]
    [Route("api/v1/projects")]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly KogaseDbContext _context;
        private readonly ApiKeyService _apiKeyService;

        public ProjectsController(
            KogaseDbContext context,
            ApiKeyService apiKeyService)
        {
            _context = context;
            _apiKeyService = apiKeyService;
        }

        [HttpGet]
        public async Task<IActionResult> GetProjects()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }
            
            int userId = int.Parse(userIdClaim);
            string? userRole = User.FindFirstValue(ClaimTypes.Role);

            // Admin can see all projects
            if (userRole == "Admin")
            {
                var projects = await _context.Projects
                    .Include(p => p.Owner)
                    .Select(p => new ProjectResponse
                    {
                        Id = p.Id,
                        Name = p.Name,
                        ApiKey = p.ApiKey,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt,
                        OwnerName = p.Owner != null ? p.Owner.Name : "Unknown"
                    })
                    .ToListAsync();

                return Ok(projects);
            }
            else
            {
                // Developers can see only their projects
                var projects = await _context.Projects
                    .Where(p => p.OwnerId == userId || 
                            _context.ProjectUsers.Any(pu => pu.ProjectId == p.Id && pu.UserId == userId))
                    .Include(p => p.Owner)
                    .Select(p => new ProjectResponse
                    {
                        Id = p.Id,
                        Name = p.Name,
                        ApiKey = p.ApiKey,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt,
                        OwnerName = p.Owner != null ? p.Owner.Name : "Unknown"
                    })
                    .ToListAsync();

                return Ok(projects);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProject(int id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }
            
            int userId = int.Parse(userIdClaim);
            string? userRole = User.FindFirstValue(ClaimTypes.Role);

            var project = await _context.Projects
                .Include(p => p.Owner)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            // Check if user has access to this project
            bool hasAccess = userRole == "Admin" || 
                             project.OwnerId == userId || 
                             await _context.ProjectUsers.AnyAsync(pu => pu.ProjectId == id && pu.UserId == userId);

            if (!hasAccess)
            {
                return Forbid();
            }

            var projectResponse = new ProjectResponse
            {
                Id = project.Id,
                Name = project.Name,
                ApiKey = project.ApiKey,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt,
                OwnerName = project.Owner != null ? project.Owner.Name : "Unknown"
            };

            return Ok(projectResponse);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] ProjectCreateRequest request)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }
            
            int userId = int.Parse(userIdClaim);
            
            // Generate API key
            string apiKey = _apiKeyService.GenerateApiKey();

            var project = new Project
            {
                Name = request.Name,
                OwnerId = userId,
                ApiKey = apiKey,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            // Get owner name for response
            var owner = await _context.Users.FindAsync(userId);
            
            var projectResponse = new ProjectResponse
            {
                Id = project.Id,
                Name = project.Name,
                ApiKey = project.ApiKey,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt,
                OwnerName = owner?.Name ?? "Unknown"
            };

            return CreatedAtAction(nameof(GetProject), new { id = project.Id }, projectResponse);
        }

        [HttpGet("{id}/apikey")]
        public async Task<IActionResult> GetApiKey(int id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }
            
            int userId = int.Parse(userIdClaim);
            string? userRole = User.FindFirstValue(ClaimTypes.Role);

            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            // Check if user has access to this project
            bool hasAccess = userRole == "Admin" || 
                             project.OwnerId == userId || 
                             await _context.ProjectUsers.AnyAsync(pu => pu.ProjectId == id && pu.UserId == userId && pu.Role == "Admin");

            if (!hasAccess)
            {
                return Forbid();
            }

            var response = new ApiKeyResponse
            {
                ApiKey = project.ApiKey
            };

            return Ok(response);
        }

        [HttpPost("{id}/apikey")]
        public async Task<IActionResult> RegenerateApiKey(int id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }
            
            int userId = int.Parse(userIdClaim);
            string? userRole = User.FindFirstValue(ClaimTypes.Role);

            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            // Check if user has access to this project
            bool hasAccess = userRole == "Admin" || 
                             project.OwnerId == userId || 
                             await _context.ProjectUsers.AnyAsync(pu => pu.ProjectId == id && pu.UserId == userId && pu.Role == "Admin");

            if (!hasAccess)
            {
                return Forbid();
            }

            // Generate new API key
            project.ApiKey = _apiKeyService.GenerateApiKey();
            project.UpdatedAt = DateTime.UtcNow;

            _context.Projects.Update(project);
            await _context.SaveChangesAsync();

            var response = new ApiKeyResponse
            {
                ApiKey = project.ApiKey
            };

            return Ok(response);
        }
    }
} 