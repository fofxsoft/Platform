using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Identity.Models;

namespace Identity.Controllers
{

    [Route("")]
    [SecurityHeaders]
    [ApiController]
    [Authorize]
    public class VersionController : ControllerBase
    {

        [HttpGet]
        public async Task<ActionResult<VersionModel>> Get()
        {
            AuthenticateResult result = await HttpContext.AuthenticateAsync();

            VersionModel version = new VersionModel()
            {
                Name = "Platform One Authentication",
                Version = "1.0.0"
            };

            return version;
        }
    }
}
