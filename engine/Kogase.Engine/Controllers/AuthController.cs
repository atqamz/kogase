using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kogase.Engine.Data;
using Kogase.Engine.Models;
using Kogase.Engine.Models.DTOs;
using Kogase.Engine.Services;

namespace Kogase.Engine.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly KogaseDbContext _context;
        private readonly TokenService _tokenService;
        private readonly PasswordService _passwordService;

        public AuthController(
            KogaseDbContext context,
            TokenService tokenService,
            PasswordService passwordService)
        {
            _context = context;
            _tokenService = tokenService;
            _passwordService = passwordService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Get user by email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // Verify password
            if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // Generate JWT token
            var token = _tokenService.GenerateJwtToken(user);
            
            // Create refresh token
            var refreshToken = new AuthToken
            {
                UserId = user.Id,
                Token = Guid.NewGuid().ToString(),
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                CreatedAt = DateTime.UtcNow
            };

            _context.AuthTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            // Return the tokens
            return Ok(new LoginResponse
            {
                AccessToken = token,
                RefreshToken = refreshToken.Token,
                ExpiresIn = 3600,
                User = new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email
                }
            });
        }
    }
} 