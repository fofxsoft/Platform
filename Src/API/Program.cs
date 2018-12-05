using System.IO;
using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace PlatformOne.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                                                              .AddJsonFile("appsettings.json", optional: false)
                                                              .AddCommandLine(args)
                                                              .Build();

            CreateWebHostBuilder(args, config).Build()
                                              .Run();

        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args, IConfiguration config) =>
            WebHost.CreateDefaultBuilder(args)
                   .UseKestrel(options =>
                   {
                       options.Listen(IPAddress.Loopback, 5001, listenOptions =>
                       {
                           IConfigurationSection cert = config.GetSection("Certificate");

                           listenOptions.UseHttps(cert.GetValue<string>("FileName"), cert.GetValue<string>("Password"));
                       });
                   })
                   .UseStartup<Startup>();
    }
}
