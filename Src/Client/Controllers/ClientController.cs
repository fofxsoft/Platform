using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IdentityModel.Client;

namespace Client.Controllers
{

    [Route("client")]
    public class ClientController : Controller
    {

        [HttpGet]
        public async Task<IActionResult> GetIndex()
        {
            string accessToken = "";

            using (TokenClient tokenClient = new TokenClient("https://localhost:5000/connect/token", "interface", "secret"))
            {
                accessToken = (await tokenClient.RequestClientCredentialsAsync("api")).AccessToken;
            }

            ViewData["Values"] = await Http.GetJArrayAsync("https://localhost:5001/api/values", accessToken);
            ViewData["Value"] = await Http.GetJArrayAsync("https://localhost:5001/api/value/123456", accessToken);
            ViewData["Distance"] = await Http.GetJArrayAsync("https://localhost:5001/api/distance/?x1=1&y1=4&x2=52&y2=85", accessToken);

            return View("Index");
        }
    }
}
