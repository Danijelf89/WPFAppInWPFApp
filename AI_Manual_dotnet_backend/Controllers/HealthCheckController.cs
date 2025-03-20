using Microsoft.AspNetCore.Mvc;

namespace AI_Manual_dotnet_backend.Controllers
{
    [ApiController]
    [Route("api/healthCheck")]
    public class HealthCheckController : ControllerBase
    {
        [HttpGet]
        [Consumes("multipart/form-data")]
        public Task<IActionResult> CheckIsServerIsAlive()
        {
            return Task.FromResult<IActionResult>(Ok());
        }
    }
}
