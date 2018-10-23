using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PlatformOne.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration {
            get;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvcCore()
                    .AddAuthorization()
                    .AddJsonFormatters();

            services.AddAuthentication("Bearer")
                    .AddIdentityServerAuthentication(options =>
                    {
                        options.Authority = "https://localhost:5000";
                        options.RequireHttpsMetadata = false;
                        options.ApiName = "api";
                    });

            services.AddCors(options =>
            {
                options.AddPolicy("default", policy =>
                {
                    policy.WithOrigins("*")
                          .AllowAnyHeader()
                          .AllowAnyMethod();

                });
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseCors("default")
               .UseHttpsRedirection()
               .UseAuthentication()
               .UseMvc();

        }
    }
}
