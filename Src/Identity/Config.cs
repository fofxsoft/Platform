using System;
using IdentityServer4;
using IdentityServer4.Test;
using IdentityServer4.Models;
using System.Collections.Generic;
using System.Security.Claims;

namespace Identity
{
    public class Config
    {
        public static TimeSpan RememberMeLoginDuration = TimeSpan.FromDays(30);
        public static readonly string WindowsAuthenticationSchemeName = Microsoft.AspNetCore.Server.IISIntegration.IISDefaults.AuthenticationScheme;
        public static string InvalidCredentialsErrorMessage = "Invalid username or password";

        public static string OfflineAccessDisplayName = "Offline Access";
        public static string OfflineAccessDescription = "Access to your applications and resources, even when you are offline";
        public static readonly string MustChooseOneErrorMessage = "You must pick at least one permission";
        public static readonly string InvalidSelectionErrorMessage = "Invalid selection";

        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile()
            };
        }

        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("api", "API"),
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "client",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,

                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    AllowedScopes =
                    {
                        "api"
                    }
                },

                new Client
                {
                    ClientId = "ro.client",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,

                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    AllowedScopes =
                    {
                        "api"
                    }
                },

                new Client
                {
                    ClientId = "interface",
                    ClientName = "Encompass",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    RequireConsent = false,

                    RedirectUris =
                    {
                        "https://localhost:5002/signin-oidc"
                    },

                    PostLogoutRedirectUris =
                    {
                        "https://localhost:5002/signout-callback-oidc"
                    },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile
                    }
                }
            };
        }

        public static List<TestUser> GetUsers()
        {
            return new List<TestUser>
            {
                new TestUser
                {
                    SubjectId = "1",
                    Username = "alice",
                    Password = "password",

                    Claims = new List<Claim>
                    {
                        new Claim("name", "Alice"),
                        new Claim("website", "https://alice.com")
                    }
                },
                new TestUser
                {
                    SubjectId = "2",
                    Username = "bob",
                    Password = "password",

                    Claims = new List<Claim>
                    {
                        new Claim("name", "Bob"),
                        new Claim("website", "https://bob.com")
                    }
                }
            };
        }
    }
}
