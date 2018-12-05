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
        public async Task<IActionResult> GetIndex(int x1 = 0, int y1 = 0, int x2 = 0, int y2 = 0)
        {
            string accessToken = await HttpContext.GetTokenAsync("access_token");

            ViewData["Values"] = await Http.GetJArrayAsync("https://localhost:5001/api/values", accessToken);
            ViewData["Value"] = await Http.GetJArrayAsync("https://localhost:5001/api/value/" + x1 + y1 + x2 + y2, accessToken);
            ViewData["Distance"] = await Http.GetJArrayAsync("https://localhost:5001/api/distance/?x1=" + x1 + "&y1=" + y1 + "&x2=" + x2 + "&y2=" + y2, accessToken);

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
