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
using IdentityServer4.Models;

namespace Identity.Controllers
{

    [Route("grants")]
    [SecurityHeaders]
    [ApiController]
    [Authorize]
    public class GrantsController : ControllerBase
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
        public async Task<ActionResult<IEnumerable<GrantModel>>> Get()
        {
            List<GrantModel> grants = new List<GrantModel>();

            foreach (Consent grant in await _interaction.GetAllUserConsentsAsync())
            {
                Client client = await _clients.FindClientByIdAsync(grant.ClientId);

                if (client != null)
                {
                    Resources resources = await _resources.FindResourcesByScopeAsync(grant.Scopes);

                    GrantModel item = new GrantModel()
                    {
                        ClientId = client.ClientId,
                        ClientName = client.ClientName ?? client.ClientId,
                        ClientLogoUrl = client.LogoUri,
                        ClientUrl = client.ClientUri,
                        Created = grant.CreationTime,
                        Expires = grant.Expiration,
                        IdentityGrantNames = resources.IdentityResources.Select(x => x.DisplayName ?? x.Name).ToArray(),
                        ApiGrantNames = resources.ApiResources.Select(x => x.DisplayName ?? x.Name).ToArray()
                    };

                    grants.Add(item);
                }
            }

            return grants;
        }

        [HttpDelete("{id}")]
        [ValidateAntiForgeryToken]
        public async void Delete(string id)
        {
            await _interaction.RevokeUserConsentAsync(id);
            await _events.RaiseAsync(new GrantsRevokedEvent(User.GetSubjectId(), id));
        }
    }
}
