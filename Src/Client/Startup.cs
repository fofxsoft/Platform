using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;

namespace Client
{
    public class Startup
    {
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

                  options.Authority = "https://localhost:5000";
                  options.RequireHttpsMetadata = false;

                  options.ClientId = "interface";
                  options.SaveTokens = true;
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
