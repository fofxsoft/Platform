using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using IdentityServer4.Events;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Identity.Models;

namespace Identity.Controllers
{

    [Route("consent")]
    [SecurityHeaders]
    [Authorize]
    public class ConsentController : Controller
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly IResourceStore _resourceStore;
        private readonly IEventService _events;
        private readonly ILogger<ConsentController> _logger;

        public ConsentController(IIdentityServerInteractionService interaction, IClientStore clientStore, IResourceStore resourceStore, IEventService events, ILogger<ConsentController> logger)
        {
            _interaction = interaction;
            _clientStore = clientStore;
            _resourceStore = resourceStore;
            _events = events;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetIndex(string returnUrl)
        {
            ConsentModel consents = await CreateConsentsAsync(returnUrl);

            if (consents != null)
            {
                return View("Index", consents);
            }

            return View("Error");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostIndex(ResourceModel model)
        {
            PermissionModel result = await ProcessConsentAsync(model);

            if (result.IsRedirect)
            {
                if (await _clientStore.IsPkceClientAsync(result.ClientId))
                {
                    return Redirect(result.RedirectUri);
                }

                return Redirect(result.RedirectUri);
            }

            if (result.HasValidationError)
            {
                ModelState.AddModelError("", result.ValidationError);
            }

            if (result.ShowView)
            {
                return View("Index", result.ViewModel);
            }

            return View("Error");
        }

        private async Task<PermissionModel> ProcessConsentAsync(ResourceModel model)
        {
            PermissionModel result = new PermissionModel();
            AuthorizationRequest request = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);

            if (request == null)
            {
                return result;
            }

            ConsentResponse grantedConsent = null;

            if (model.Button == "no")
            {
                grantedConsent = ConsentResponse.Denied;

                await _events.RaiseAsync(new ConsentDeniedEvent(User.GetSubjectId(), result.ClientId, request.ScopesRequested));
            }
            else if (model.Button == "yes" && model != null)
            {
                if (model.ScopesConsented != null && model.ScopesConsented.Any())
                {
                    IEnumerable<string> scopes = model.ScopesConsented;

                    grantedConsent = new ConsentResponse
                    {
                        RememberConsent = true,
                        ScopesConsented = scopes.ToArray()
                    };

                    await _events.RaiseAsync(new ConsentGrantedEvent(User.GetSubjectId(), request.ClientId, request.ScopesRequested, grantedConsent.ScopesConsented, grantedConsent.RememberConsent));
                }
                else
                {
                    result.ValidationError = Config.MustChooseOneErrorMessage;
                }
            }
            else
            {
                result.ValidationError = Config.InvalidSelectionErrorMessage;
            }

            if (grantedConsent != null)
            {
                await _interaction.GrantConsentAsync(request, grantedConsent);

                result.RedirectUri = model.ReturnUrl;
                result.ClientId = request.ClientId;
            }
            else
            {
                result.ViewModel = await CreateConsentsAsync(model.ReturnUrl, model);
            }

            return result;
        }

        private async Task<ConsentModel> CreateConsentsAsync(string returnUrl, ResourceModel model = null)
        {
            AuthorizationRequest request = await _interaction.GetAuthorizationContextAsync(returnUrl);

            if (request != null)
            {
                Client client = await _clientStore.FindEnabledClientByIdAsync(request.ClientId);

                if (client != null)
                {
                    Resources resources = await _resourceStore.FindEnabledResourcesByScopeAsync(request.ScopesRequested);

                    if (resources != null && (resources.IdentityResources.Any() || resources.ApiResources.Any()))
                    {
                        return CreateConsent(model, returnUrl, request, client, resources);
                    }
                    else
                    {
                        _logger.LogError("No scopes matching: {0}", request.ScopesRequested.Aggregate((x, y) => x + ", " + y));
                    }
                }
                else
                {
                    _logger.LogError("Invalid client id: {0}", request.ClientId);
                }
            }
            else
            {
                _logger.LogError("No consent request matching request: {0}", returnUrl);
            }

            return null;
        }

        private ConsentModel CreateConsent(ResourceModel model, string returnUrl, AuthorizationRequest request, Client client, Resources resources)
        {
            ConsentModel consent = new ConsentModel
            {
                ScopesConsented = model?.ScopesConsented ?? Enumerable.Empty<string>(),

                ReturnUrl = returnUrl,

                ClientName = client.ClientName ?? client.ClientId,
                ClientUrl = client.ClientUri,
                ClientLogoUrl = client.LogoUri
            };

            consent.IdentityScopes = resources.IdentityResources.Select(x => CreateScope(x, consent.ScopesConsented.Contains(x.Name) || model == null)).ToArray();
            consent.ResourceScopes = resources.ApiResources.SelectMany(x => x.Scopes).Select(x => CreateScope(x, consent.ScopesConsented.Contains(x.Name) || model == null)).ToArray();

            if (resources.OfflineAccess)
            {
                consent.ResourceScopes = consent.ResourceScopes.Union(new ScopeModel[] {
                    new ScopeModel
                    {
                        Name = IdentityServer4.IdentityServerConstants.StandardScopes.OfflineAccess,
                        DisplayName = Config.OfflineAccessDisplayName,
                        Description = Config.OfflineAccessDescription,
                        Emphasize = true,
                        Checked = (consent.ScopesConsented.Contains(IdentityServer4.IdentityServerConstants.StandardScopes.OfflineAccess) || model == null)
                    }
                });
            }

            return consent;
        }

        private ScopeModel CreateScope(IdentityResource identity, bool check)
        {
            return new ScopeModel
            {
                Name = identity.Name,
                DisplayName = identity.DisplayName,
                Description = identity.Description,
                Emphasize = identity.Emphasize,
                Required = identity.Required,
                Checked = check || identity.Required
            };
        }

        public ScopeModel CreateScope(Scope scope, bool check)
        {
            return new ScopeModel
            {
                Name = scope.Name,
                DisplayName = scope.DisplayName,
                Description = scope.Description,
                Emphasize = scope.Emphasize,
                Required = scope.Required,
                Checked = check || scope.Required
            };
        }
    }
}
