using Microsoft.AspNetCore.Mvc;

namespace Kogase.Engine.Controllers
{
    [ApiController]
    [Route("api/v1/health")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { status = "healthy", version = "1.0.0" });
        }
    }
} 