using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Identity.Models;

namespace Identity.Controllers
{

    [Route("diagnostics")]
    [SecurityHeaders]
    [Authorize]
    public class DiagnosticsController : Controller
    {

        [HttpGet]
        public async Task<IActionResult> GetIndex()
        {
            string[] localAddresses = new string[]
            {
                "127.0.0.1",
                "::1",
                HttpContext.Connection.LocalIpAddress.ToString()
            };

            if (!localAddresses.Contains(HttpContext.Connection.RemoteIpAddress.ToString()))
            {
                return NotFound();
            }

            return View("Index", new DiagnosticsModel(await HttpContext.AuthenticateAsync()));
        }
    }
}
