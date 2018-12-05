using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using IdentityModel;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Identity.Data;
using Identity.Models;

namespace Identity
{
    public class SeedData
    {
        public static void EnsureSeedData(IServiceProvider serviceProvider)
        {
            Console.WriteLine("Seeding database...");

            using (var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

                {
                    ConfigurationDbContext context = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();

                    context.Database.Migrate();
                    EnsureSeedData(context);
                }

                {
                    UserData context = scope.ServiceProvider.GetService<UserData>();

                    context.Database.Migrate();

                    UserManager<UserModel> userMgr = scope.ServiceProvider.GetRequiredService<UserManager<UserModel>>();
                    UserModel alice = userMgr.FindByNameAsync("alice").Result;

                    if (alice == null)
                    {
                        alice = new UserModel
                        {
                            UserName = "alice"
                        };

                        IdentityResult result = userMgr.CreateAsync(alice, "Pass123$").Result;

                        if (!result.Succeeded)
                        {
                            throw new Exception(result.Errors.First().Description);
                        }

                        result = userMgr.AddClaimsAsync(alice, new Claim[]
                        {
                            new Claim(JwtClaimTypes.Name, "Alice Smith"),
                            new Claim(JwtClaimTypes.GivenName, "Alice"),
                            new Claim(JwtClaimTypes.FamilyName, "Smith"),
                            new Claim(JwtClaimTypes.Email, "alice.smith@email.my.domain"),
                            new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                            new Claim(JwtClaimTypes.WebSite, "http://alice.my.domain"),
                            new Claim(JwtClaimTypes.Address, @"{ 'street_address': 'One Hacker Way', 'locality': 'Heidelberg', 'postal_code': 69118, 'country': 'Germany' }", IdentityServer4.IdentityServerConstants.ClaimValueTypes.Json),
                            new Claim("location", "Somewhere")
                        }).Result;

                        if (!result.Succeeded)
                        {
                            throw new Exception(result.Errors.First().Description);
                        }

                        Console.WriteLine("alice created");
                    }
                    else
                    {
                        Console.WriteLine("alice already exists");
                    }

                    UserModel bob = userMgr.FindByNameAsync("bob").Result;

                    if (bob == null)
                    {
                        bob = new UserModel
                        {
                            UserName = "bob"
                        };

                        IdentityResult result = userMgr.CreateAsync(bob, "Pass123$").Result;

                        if (!result.Succeeded)
                        {
                            throw new Exception(result.Errors.First().Description);
                        }

                        result = userMgr.AddClaimsAsync(bob, new Claim[]
                        {
                            new Claim(JwtClaimTypes.Name, "Bob Smith"),
                            new Claim(JwtClaimTypes.GivenName, "Bob"),
                            new Claim(JwtClaimTypes.FamilyName, "Smith"),
                            new Claim(JwtClaimTypes.Email, "bob.smith@email.my.domain"),
                            new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                            new Claim(JwtClaimTypes.WebSite, "http://bob.my.domain"),
                            new Claim(JwtClaimTypes.Address, @"{ 'street_address': 'One Hacker Way', 'locality': 'Heidelberg', 'postal_code': 69118, 'country': 'Germany' }", IdentityServer4.IdentityServerConstants.ClaimValueTypes.Json),
                            new Claim("location", "Somewhere")
                        }).Result;

                        if (!result.Succeeded)
                        {
                            throw new Exception(result.Errors.First().Description);
                        }

                        Console.WriteLine("bob created");
                    }
                    else
                    {
                        Console.WriteLine("bob already exists");
                    }
                }
            }

            Console.WriteLine("Done seeding database.");
            Console.WriteLine();
        }

        private static void EnsureSeedData(ConfigurationDbContext context)
        {
            if (!context.Clients.Any())
            {
                Console.WriteLine("Clients being populated");

                foreach (var client in Config.GetClients().ToList())
                {
                    context.Clients.Add(client.ToEntity());
                }

                context.SaveChanges();
            }
            else
            {
                Console.WriteLine("Clients already populated");
            }

            if (!context.IdentityResources.Any())
            {
                Console.WriteLine("IdentityResources being populated");

                foreach (var resource in Config.GetIdentityResources().ToList())
                {
                    context.IdentityResources.Add(resource.ToEntity());
                }

                context.SaveChanges();
            }
            else
            {
                Console.WriteLine("IdentityResources already populated");
            }

            if (!context.ApiResources.Any())
            {
                Console.WriteLine("ApiResources being populated");

                foreach (var resource in Config.GetApiResources().ToList())
                {
                    context.ApiResources.Add(resource.ToEntity());
                }

                context.SaveChanges();
            }
            else
            {
                Console.WriteLine("ApiResources already populated");
            }
        }
    }
}
