using Microsoft.AspNetCore.Mvc;

namespace Client.Controllers
{

    [Route("")]
    public class HomeController : Controller
    {

        [HttpGet]
        public IActionResult GetIndex()
        {
            return View("Index");
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
