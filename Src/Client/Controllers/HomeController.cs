using Microsoft.AspNetCore.Mvc;

namespace Client.Controllers
{

    [Route("")]
    public class HomeController : Controller
    {

        [HttpGet]
        public IActionResult Public()
        {
            return View("Index");
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
