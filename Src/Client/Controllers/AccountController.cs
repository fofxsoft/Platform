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
        public async Task<IActionResult> GetIndex()
        {
            string accessToken = await HttpContext.GetTokenAsync("access_token");

            ViewData["Values"] = await Http.GetJArrayAsync("https://localhost:5001/api/values", accessToken);
            ViewData["Value"] = await Http.GetJArrayAsync("https://localhost:5001/api/value/123456", accessToken);
            ViewData["Distance"] = await Http.GetJArrayAsync("https://localhost:5001/api/distance/?x1=1&y1=4&x2=52&y2=85", accessToken);

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
