using IdentityServer4.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Identity.Models;

namespace Identity.Controllers
{

    [Route("")]
    [SecurityHeaders]
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IHostingEnvironment _environment;

        public HomeController(IIdentityServerInteractionService interaction, IHostingEnvironment environment)
        {
            _interaction = interaction;
            _environment = environment;
        }

        [HttpGet]
        public IActionResult GetIndex()
        {
            if (_environment.IsDevelopment())
            {
                return View("Index");
            }

            return NotFound();
        }

        public async Task<IActionResult> Error(string errorId)
        {
            var error = new ErrorModel();
            var message = await _interaction.GetErrorContextAsync(errorId);

            if (message != null)
            {
                error.Error = message;

                if (!_environment.IsDevelopment())
                {
                    message.ErrorDescription = null;
                }
            }

            return View("Error", error);
        }
    }
}
