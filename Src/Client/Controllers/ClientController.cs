using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json.Linq;
using IdentityModel.Client;

namespace Client.Controllers
{

    [Route("client")]
    public class ClientController : Controller
    {

        [HttpGet]
        public async Task<IActionResult> GetIndex()
        {
            string accessToken = null;

            if (User.Identity.IsAuthenticated)
            {
                accessToken = await HttpContext.GetTokenAsync("access_token");
            }
            else
            {
                using (TokenClient tokenClient = new TokenClient("https://localhost:5000/connect/token", "interface", "secret"))
                {
                    accessToken = (await tokenClient.RequestClientCredentialsAsync("api")).AccessToken;
                }
            }

            string json = "";

            using (HttpClient client = new HttpClient())
            {
                client.SetBearerToken(accessToken);

                json += JArray.Parse(await client.GetStringAsync("https://localhost:5001/api/values")).ToString() + "\n\n";
                json += JArray.Parse(await client.GetStringAsync("https://localhost:5001/api/value/123456")).ToString() + "\n\n";
                json += JArray.Parse(await client.GetStringAsync("https://localhost:5001/api/distance/?x1=1&y1=4&x2=52&y2=85")).ToString() + "\n\n";
            }

            ViewData["JSON"] = json;

            return View("Index");
        }
    }
}
