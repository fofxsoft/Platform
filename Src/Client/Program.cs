using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Client
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build()
                                      .Run();

        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                   .UseKestrel(options =>
                   {
                       options.Listen(IPAddress.Loopback, 5002, listenOptions =>
                       {
                           listenOptions.UseHttps("certificate.pfx", "Marvin");
                       });
                   })
                   .UseWebRoot("Public")
                   .UseStartup<Startup>();

    }
}
