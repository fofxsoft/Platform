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
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Identity.Models;

namespace Identity.Controllers
{

    [Route("external")]
    [SecurityHeaders]
    [AllowAnonymous]
    public class ExternalController : Controller
    {
        private readonly UserManager<UserModel> _userManager;
        private readonly SignInManager<UserModel> _signInManager;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IEventService _events;

        public ExternalController(UserManager<UserModel> userManager, SignInManager<UserModel> signInManager, IIdentityServerInteractionService interaction, IClientStore clientStore, IAuthenticationSchemeProvider schemeProvider, IEventService events)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _interaction = interaction;
            _events = events;
        }

        [Route("challenge")]
        [HttpGet]
        public async Task<IActionResult> GetChallenge(string provider, string returnUrl)
        {
            if (Config.WindowsAuthenticationSchemeName == provider)
            {
                return await ProcessWindowsLoginAsync(returnUrl);
            }
            else
            {
                var props = new AuthenticationProperties()
                {
                    RedirectUri = Url.Action("callback"),
                    Items =
                    {
                        { "returnUrl", returnUrl },
                        { "scheme", provider },
                    }
                };

                return Challenge(props, provider);
            }
        }

        [Route("callback")]
        [HttpGet]
        public async Task<IActionResult> GetCallback()
        {
            AuthenticateResult result = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);

            if (result?.Succeeded != true)
            {
                throw new Exception("External authentication error");
            }

            (UserModel user, string provider, string providerUserId, IEnumerable <Claim> claims) = await FindUserByExternalProviderAsync(result);

            if (user == null)
            {
                user = await AutoProvisionUserAsync(provider, providerUserId, claims);
            }

            List<Claim> additionalLocalClaims = new List<Claim>();
            AuthenticationProperties localSignInProps = new AuthenticationProperties();

            ProcessLoginCallbackOidc(result, additionalLocalClaims, localSignInProps);
            ProcessLoginCallbackWsFed(result, additionalLocalClaims, localSignInProps);
            ProcessLoginCallbackSaml2p(result, additionalLocalClaims, localSignInProps);

            ClaimsPrincipal principal = await _signInManager.CreateUserPrincipalAsync(user);

            additionalLocalClaims.AddRange(principal.Claims);

            string name = principal.FindFirst(JwtClaimTypes.Name)?.Value ?? user.Id;

            await _events.RaiseAsync(new UserLoginSuccessEvent(provider, providerUserId, user.Id, name));
            await HttpContext.SignInAsync(user.Id, name, provider, localSignInProps, additionalLocalClaims.ToArray());
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            string returnUrl = result.Properties.Items["returnUrl"];

            if (_interaction.IsValidReturnUrl(returnUrl) || Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return Redirect("~/");
        }

        private async Task<IActionResult> ProcessWindowsLoginAsync(string returnUrl)
        {
            AuthenticateResult result = await HttpContext.AuthenticateAsync(Config.WindowsAuthenticationSchemeName);

            if (result?.Principal is WindowsPrincipal wp)
            {
                AuthenticationProperties props = new AuthenticationProperties()
                {
                    RedirectUri = Url.Action("callback"),
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

                await HttpContext.SignInAsync(IdentityConstants.ExternalScheme, new ClaimsPrincipal(id), props);

                return Redirect(props.RedirectUri);
            }
            else
            {
                return Challenge(Config.WindowsAuthenticationSchemeName);
            }
        }

        private async Task<(UserModel user, string provider, string providerUserId, IEnumerable<Claim> claims)> FindUserByExternalProviderAsync(AuthenticateResult result)
        {
            ClaimsPrincipal externalUser = result.Principal;
            Claim userIdClaim = externalUser.FindFirst(JwtClaimTypes.Subject) ?? externalUser.FindFirst(ClaimTypes.NameIdentifier) ?? throw new Exception("Unknown userid");
            List<Claim> claims = externalUser.Claims.ToList();

            claims.Remove(userIdClaim);

            string provider = result.Properties.Items["scheme"];
            string providerUserId = userIdClaim.Value;

            UserModel user = await _userManager.FindByLoginAsync(provider, providerUserId);

            return (user, provider, providerUserId, claims);
        }

        private async Task<UserModel> AutoProvisionUserAsync(string provider, string providerUserId, IEnumerable<Claim> claims)
        {
            List<Claim> filtered = new List<Claim>();
            string name = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Name)?.Value ?? claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;

            if (name != null)
            {
                filtered.Add(new Claim(JwtClaimTypes.Name, name));
            }
            else
            {
                string first = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.GivenName)?.Value ?? claims.FirstOrDefault(x => x.Type == ClaimTypes.GivenName)?.Value;
                string last = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.FamilyName)?.Value ?? claims.FirstOrDefault(x => x.Type == ClaimTypes.Surname)?.Value;

                if (first != null && last != null)
                {
                    filtered.Add(new Claim(JwtClaimTypes.Name, first + " " + last));
                }
                else if (first != null)
                {
                    filtered.Add(new Claim(JwtClaimTypes.Name, first));
                }
                else if (last != null)
                {
                    filtered.Add(new Claim(JwtClaimTypes.Name, last));
                }
            }

            string email = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Email)?.Value ?? claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;

            if (email != null)
            {
                filtered.Add(new Claim(JwtClaimTypes.Email, email));
            }

            var user = new UserModel
            {
                UserName = Guid.NewGuid().ToString(),
            };

            IdentityResult identityResult = await _userManager.CreateAsync(user);

            if (!identityResult.Succeeded)
            {
                throw new Exception(identityResult.Errors.First().Description);
            }

            if (filtered.Any())
            {
                identityResult = await _userManager.AddClaimsAsync(user, filtered);

                if (!identityResult.Succeeded)
                {
                    throw new Exception(identityResult.Errors.First().Description);
                }
            }

            identityResult = await _userManager.AddLoginAsync(user, new UserLoginInfo(provider, providerUserId, provider));

            if (!identityResult.Succeeded)
            {
                throw new Exception(identityResult.Errors.First().Description);
            }

            return user;
        }

        private void ProcessLoginCallbackOidc(AuthenticateResult externalResult, List<Claim> localClaims, AuthenticationProperties localSignInProps)
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

        private void ProcessLoginCallbackWsFed(AuthenticateResult externalResult, List<Claim> localClaims, AuthenticationProperties localSignInProps)
        {
        }

        private void ProcessLoginCallbackSaml2p(AuthenticateResult externalResult, List<Claim> localClaims, AuthenticationProperties localSignInProps)
        {
        }
    }
}
