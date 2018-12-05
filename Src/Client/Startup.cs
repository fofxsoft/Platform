using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace Client
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration
        {
            get;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Cookies";
                options.DefaultChallengeScheme = "oidc";
            }).AddCookie("Cookies")
              .AddOpenIdConnect("oidc", options =>
              {
                  options.SignInScheme = "Cookies";
                  options.Authority = Configuration.GetSection("Authentication").GetValue<string>("Authority");
                  options.RequireHttpsMetadata = true;

                  options.ClientId = Configuration.GetSection("Authentication").GetValue<string>("ClientID");
                  options.ClientSecret = Configuration.GetSection("Authentication").GetValue<string>("ClientSecret");
                  options.ResponseType = "code id_token";

                  options.SaveTokens = true;
                  options.GetClaimsFromUserInfoEndpoint = true;

                  options.Scope.Add("api");
                  options.Scope.Add("offline_access");

                  options.Events = new OpenIdConnectEvents
                  {
                      OnRemoteFailure = context =>
                      {
                          context.Response.Redirect("/");
                          context.HandleResponse();

                          return Task.FromResult(0);
                      }
                  };
              });

        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication()
               .UseStaticFiles()
               .UseMvcWithDefaultRoute();

        }

    }
}
