﻿using IdentityModel;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Test;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Identity.Models;
using Microsoft.AspNetCore.Identity;

namespace Identity.Controllers
{

    [Route("account")]
    public class AccountController : Controller
    {
        private readonly UserManager<UserModel> _userManager;
        private readonly SignInManager<UserModel> _signInManager;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly IEventService _events;

        public AccountController(UserManager<UserModel> userManager, SignInManager<UserModel> signInManager, IIdentityServerInteractionService interaction, IClientStore clientStore, IAuthenticationSchemeProvider schemeProvider, IEventService events)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _interaction = interaction;
            _clientStore = clientStore;
            _schemeProvider = schemeProvider;
            _events = events;
        }

        [Route("login")]
        [HttpGet]
        public async Task<IActionResult> GetLogin(string returnUrl)
        {
            LoginModel vm = await CreateLoginAsync(returnUrl);

            if (vm.IsExternalLoginOnly)
            {
                return RedirectToAction("Challenge", "External", new { provider = vm.ExternalLoginScheme, returnUrl });
            }

            return View("Index", vm);
        }

        [Route("login")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostLogin(CredentialsModel model, string button)
        {
            if (button != "login")
            {
                AuthorizationRequest context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);

                if (context != null)
                {
                    await _interaction.GrantConsentAsync(context, ConsentResponse.Denied);

                    return Redirect(model.ReturnUrl);
                }
                else
                {
                    return Redirect("~/");
                }
            }

            if (ModelState.IsValid)
            {
                Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberLogin, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    UserModel user = await _userManager.FindByNameAsync(model.Username);

                    await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id, user.UserName));

                    if (_interaction.IsValidReturnUrl(model.ReturnUrl) || Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }

                    return Redirect("~/");
                }

                await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, "invalid credentials"));

                ModelState.AddModelError("", "Invalid username or password");
            }

            return View("Index", await CreateLoginAsync(model));
        }

        [Route("logout")]
        [HttpGet]
        public async Task<IActionResult> GetLogout(string logoutId)
        {
            return await PostLogout(logoutId);
        }

        [Route("logout")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostLogout(string logoutId)
        {
            RedirectModel redirect = await CreateRedirectAsync(logoutId);

            if (User?.Identity.IsAuthenticated == true)
            {
                await _signInManager.SignOutAsync();
                await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
            }

            if (redirect.TriggerExternalSignout)
            {
                string url = Url.Action("Logout", new {
                    logoutId = redirect.LogoutId
                });

                return SignOut(new AuthenticationProperties {
                    RedirectUri = url
                }, redirect.ExternalAuthenticationScheme);
            }

            return Redirect(!string.IsNullOrWhiteSpace(redirect.RedirectUri) ? redirect.RedirectUri : "/");
        }

        private async Task<LoginModel> CreateLoginAsync(string returnUrl)
        {
            AuthorizationRequest context = await _interaction.GetAuthorizationContextAsync(returnUrl);

            if (context?.IdP != null)
            {
                return new LoginModel
                {
                    EnableLocalLogin = false,
                    ReturnUrl = returnUrl,
                    Username = context?.LoginHint,

                    LoginProviders = new LoginProviderModel[]
                    {
                        new LoginProviderModel
                        {
                            AuthenticationScheme = context.IdP
                        }
                    }
                };
            }

            IEnumerable<AuthenticationScheme> schemes = await _schemeProvider.GetAllSchemesAsync();

            List<LoginProviderModel> providers = schemes
                .Where(x => x.DisplayName != null || x.Name.Equals(Config.WindowsAuthenticationSchemeName, StringComparison.OrdinalIgnoreCase))
                .Select(x => new LoginProviderModel
                {
                    DisplayName = x.DisplayName,
                    AuthenticationScheme = x.Name
                }).ToList();

            bool allowLocal = true;

            if (context?.ClientId != null)
            {
                Client client = await _clientStore.FindEnabledClientByIdAsync(context.ClientId);

                if (client != null)
                {
                    allowLocal = client.EnableLocalLogin;

                    if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any())
                    {
                        providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
                    }
                }
            }

            return new LoginModel
            {
                EnableLocalLogin = allowLocal,
                ReturnUrl = returnUrl,
                Username = context?.LoginHint,
                LoginProviders = providers.ToArray()
            };
        }

        private async Task<LoginModel> CreateLoginAsync(CredentialsModel model)
        {
            LoginModel vm = await CreateLoginAsync(model.ReturnUrl);

            vm.Username = model.Username;
            vm.RememberLogin = model.RememberLogin;

            return vm;
        }

        private async Task<RedirectModel> CreateRedirectAsync(string logoutId)
        {
            LogoutRequest logout = await _interaction.GetLogoutContextAsync(logoutId);

            RedirectModel vm = new RedirectModel
            {
                RedirectUri = logout?.PostLogoutRedirectUri,
                ClientName = string.IsNullOrEmpty(logout?.ClientName) ? logout?.ClientId : logout?.ClientName,
                SignOutIframeUrl = logout?.SignOutIFrameUrl,
                LogoutId = logoutId
            };

            if (User?.Identity.IsAuthenticated == true)
            {
                string idp = User.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;

                if (idp != null && idp != IdentityServer4.IdentityServerConstants.LocalIdentityProvider)
                {
                    bool providerSupportsSignout = await HttpContext.GetSchemeSupportsSignOutAsync(idp);

                    if (providerSupportsSignout)
                    {
                        if (vm.LogoutId == null)
                        {
                            vm.LogoutId = await _interaction.CreateLogoutContextAsync();
                        }

                        vm.ExternalAuthenticationScheme = idp;
                    }
                }
            }

            return vm;
        }
    }
}
