using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using Identity.Models;

namespace Identity.Controllers
{

    [Route("grants")]
    [SecurityHeaders]
    [Authorize]
    public class GrantsController : Controller
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clients;
        private readonly IResourceStore _resources;
        private readonly IEventService _events;

        public GrantsController(IIdentityServerInteractionService interaction, IClientStore clients, IResourceStore resources, IEventService events)
        {
            _interaction = interaction;
            _clients = clients;
            _resources = resources;
            _events = events;
        }

        [HttpGet]
        public async Task<IActionResult> GetIndex()
        {
            return View("Index", await CreateGrantsAsync());
        }

        [Route("revoke")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostRevoke(string clientId)
        {
            await _interaction.RevokeUserConsentAsync(clientId);
            await _events.RaiseAsync(new GrantsRevokedEvent(User.GetSubjectId(), clientId));

            return Redirect("/grants");
        }

        private async Task<GrantsModel> CreateGrantsAsync()
        {
            var grants = new List<GrantModel>();

            foreach (var grant in await _interaction.GetAllUserConsentsAsync())
            {
                var client = await _clients.FindClientByIdAsync(grant.ClientId);

                if (client != null)
                {
                    var resources = await _resources.FindResourcesByScopeAsync(grant.Scopes);

                    grants.Add(new GrantModel()
                    {
                        ClientId = client.ClientId,
                        ClientName = client.ClientName ?? client.ClientId,
                        ClientLogoUrl = client.LogoUri,
                        ClientUrl = client.ClientUri,
                        Created = grant.CreationTime,
                        Expires = grant.Expiration,
                        IdentityGrantNames = resources.IdentityResources.Select(x => x.Description ?? x.DisplayName ?? x.Name).ToArray(),
                        ApiGrantNames = resources.ApiResources.Select(x => x.Description ?? x.DisplayName ?? x.Name).ToArray()
                    });
                }
            }

            return new GrantsModel
            {
                Grants = grants
            };
        }
    }
}
