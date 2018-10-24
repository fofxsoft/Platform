using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;

namespace Client.Controllers
{

    [Route("account")]
    public class AccountController : Controller
    {

        [Authorize]
        [HttpGet]
        public IActionResult GetIndex()
        {
            return View("Index");
        }

        [Route("logout")]
        [HttpGet]
        public async Task GetLogout()
        {
            await HttpContext.SignOutAsync("Cookies");
            await HttpContext.SignOutAsync("oidc");
        }
    }
}
