using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Events;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using IdentityServer4.Models;
using Identity.Models;

namespace Identity.Controllers
{

    [Route("external")]
    [SecurityHeaders]
    [AllowAnonymous]
    public class ExternalController : Controller
    {
        private readonly TestUserStore _users;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly IEventService _events;

        public ExternalController(IIdentityServerInteractionService interaction, IClientStore clientStore, IEventService events, TestUserStore users = null)
        {
            _users = users ?? new TestUserStore(TestUsers.Users);

            _interaction = interaction;
            _clientStore = clientStore;
            _events = events;
        }

        [Route("challenge")]
        [HttpGet]
        public async Task<IActionResult> Challenge(string provider, string returnUrl)
        {
            if (string.IsNullOrEmpty(returnUrl)) returnUrl = "~/";

            if (Url.IsLocalUrl(returnUrl) == false && _interaction.IsValidReturnUrl(returnUrl) == false)
            {
                throw new Exception("invalid return URL");
            }

            if (Config.WindowsAuthenticationSchemeName == provider)
            {
                return await ProcessWindowsLoginAsync(returnUrl);
            }
            else
            {
                AuthenticationProperties props = new AuthenticationProperties
                {
                    RedirectUri = Url.Action(nameof(Callback)),
                    Items =
                    {
                        {
                            "returnUrl",
                            returnUrl
                        },
                        {
                            "scheme",
                            provider
                        },
                    }
                };

                return Challenge(props, provider);
            }
        }

        [Route("callback")]
        [HttpGet]
        public async Task<IActionResult> Callback()
        {
            AuthenticateResult result = await HttpContext.AuthenticateAsync(IdentityServer4.IdentityServerConstants.ExternalCookieAuthenticationScheme);

            if (result?.Succeeded != true)
            {
                throw new Exception("External authentication error");
            }

            (TestUser user, string provider, string providerUserId, IEnumerable <Claim> claims) = FindUserFromExternalProvider(result);

            if (user == null)
            {
                user = AutoProvisionUser(provider, providerUserId, claims);
            }

            List<Claim> additionalLocalClaims = new List<Claim>();
            AuthenticationProperties localSignInProps = new AuthenticationProperties();

            ProcessLoginCallbackForOidc(result, additionalLocalClaims, localSignInProps);
            ProcessLoginCallbackForWsFed(result, additionalLocalClaims, localSignInProps);
            ProcessLoginCallbackForSaml2p(result, additionalLocalClaims, localSignInProps);

            await _events.RaiseAsync(new UserLoginSuccessEvent(provider, providerUserId, user.SubjectId, user.Username));

            await HttpContext.SignInAsync(user.SubjectId, user.Username, provider, localSignInProps, additionalLocalClaims.ToArray());
            await HttpContext.SignOutAsync(IdentityServer4.IdentityServerConstants.ExternalCookieAuthenticationScheme);

            string returnUrl = result.Properties.Items["returnUrl"] ?? "~/";

            AuthorizationRequest context = await _interaction.GetAuthorizationContextAsync(returnUrl);

            if (context != null)
            {
                if (await _clientStore.IsPkceClientAsync(context.ClientId))
                {
                    return Redirect(returnUrl);
                }
            }

            return Redirect(returnUrl);
        }

        private async Task<IActionResult> ProcessWindowsLoginAsync(string returnUrl)
        {
            AuthenticateResult result = await HttpContext.AuthenticateAsync(Config.WindowsAuthenticationSchemeName);

            if (result?.Principal is WindowsPrincipal wp)
            {
                AuthenticationProperties props = new AuthenticationProperties()
                {
                    RedirectUri = Url.Action("Callback"),
                    Items =
                    {
                        {
                            "returnUrl",
                            returnUrl
                        },
                        {
                            "scheme",
                            Config.WindowsAuthenticationSchemeName
                        },
                    }
                };

                ClaimsIdentity id = new ClaimsIdentity(Config.WindowsAuthenticationSchemeName);

                id.AddClaim(new Claim(JwtClaimTypes.Subject, wp.Identity.Name));
                id.AddClaim(new Claim(JwtClaimTypes.Name, wp.Identity.Name));

                await HttpContext.SignInAsync(IdentityServer4.IdentityServerConstants.ExternalCookieAuthenticationScheme, new ClaimsPrincipal(id), props);

                return Redirect(props.RedirectUri);
            }
            else
            {
                return Challenge(Config.WindowsAuthenticationSchemeName);
            }
        }

        private (TestUser user, string provider, string providerUserId, IEnumerable<Claim> claims) FindUserFromExternalProvider(AuthenticateResult result)
        {
            ClaimsPrincipal externalUser = result.Principal;
            Claim userIdClaim = externalUser.FindFirst(JwtClaimTypes.Subject) ?? externalUser.FindFirst(ClaimTypes.NameIdentifier) ?? throw new Exception("Unknown userid");
            List<Claim> claims = externalUser.Claims.ToList();

            claims.Remove(userIdClaim);

            string provider = result.Properties.Items["scheme"];
            string providerUserId = userIdClaim.Value;

            TestUser user = _users.FindByExternalProvider(provider, providerUserId);

            return (user, provider, providerUserId, claims);
        }

        private TestUser AutoProvisionUser(string provider, string providerUserId, IEnumerable<Claim> claims)
        {
            return _users.AutoProvisionUser(provider, providerUserId, claims.ToList());
        }

        private void ProcessLoginCallbackForOidc(AuthenticateResult externalResult, List<Claim> localClaims, AuthenticationProperties localSignInProps)
        {
            Claim sid = externalResult.Principal.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.SessionId);

            if (sid != null)
            {
                localClaims.Add(new Claim(JwtClaimTypes.SessionId, sid.Value));
            }

            string id_token = externalResult.Properties.GetTokenValue("id_token");

            if (id_token != null)
            {
                localSignInProps.StoreTokens(new[] { new AuthenticationToken { Name = "id_token", Value = id_token } });
            }
        }

        private void ProcessLoginCallbackForWsFed(AuthenticateResult externalResult, List<Claim> localClaims, AuthenticationProperties localSignInProps)
        {
        }

        private void ProcessLoginCallbackForSaml2p(AuthenticateResult externalResult, List<Claim> localClaims, AuthenticationProperties localSignInProps)
        {
        }
    }
}
