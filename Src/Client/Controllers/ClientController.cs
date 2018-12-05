using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IdentityModel.Client;

namespace Client.Controllers
{

    [Route("client")]
    public class ClientController : Controller
    {

        [HttpGet]
        public async Task<IActionResult> GetIndex(int x1 = 0, int y1 = 0, int x2 = 0, int y2 = 0)
        {
            string accessToken = "";

            using (TokenClient tokenClient = new TokenClient("https://localhost:5000/connect/token", "interface", "secret"))
            {
                accessToken = (await tokenClient.RequestClientCredentialsAsync("api")).AccessToken;
            }

            ViewData["Values"] = await Http.GetJArrayAsync("https://localhost:5001/api/values", accessToken);
            ViewData["Value"] = await Http.GetJArrayAsync("https://localhost:5001/api/value/" + x1 + y1 + x2 + y2, accessToken);
            ViewData["Distance"] = await Http.GetJArrayAsync("https://localhost:5001/api/distance/?x1=" + x1 + "&y1=" + y1 + "&x2=" + x2 + "&y2=" + y2, accessToken);

            return View("Index");
        }
    }
}
