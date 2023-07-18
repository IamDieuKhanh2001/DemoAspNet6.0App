using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DemoAspNetApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet("Admin")]
        [Authorize(Roles = "Admin")]
        public IActionResult AdminAPI()
        {
            return Ok("Admin can access");
        }
        [HttpGet("Manager")]
        [Authorize(Roles = "Manager")]
        public IActionResult ManagerAPI()
        {
            return Ok("Manager can access");
        }
        [HttpGet("Member")]
        [Authorize(Roles = "Member")]
        public IActionResult MemberAPI()
        {
            return Ok("Member can access");
        }
        [HttpGet("All")]
        [Authorize]
        public IActionResult AllAccess()
        {
            return Ok("All can access");
        }
    }
}
