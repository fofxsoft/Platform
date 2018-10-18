using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using IdentityModel.Client;

namespace PlatformOne.Console
{
    class Program
    {
        public static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();

        private static async Task MainAsync()
        {
            var disco = await DiscoveryClient.GetAsync("https://localhost:5000");

            if (disco.IsError)
            {
                System.Console.WriteLine(disco.Error);

                return;
            }

            var tokenClient = new TokenClient(disco.TokenEndpoint, "client", "secret");
            var tokenResponse = await tokenClient.RequestClientCredentialsAsync("yoda");

            if (tokenResponse.IsError)
            {
                System.Console.WriteLine(tokenResponse.Error);

                return;
            }

            System.Console.WriteLine(tokenResponse.Json);
            System.Console.WriteLine("\n\n");

            var client = new HttpClient();

            client.SetBearerToken(tokenResponse.AccessToken);

            var response = await client.GetAsync("https://localhost:5001/api/values");

            if (!response.IsSuccessStatusCode)
            {
                System.Console.WriteLine(response.StatusCode);
            }
            else
            {
                System.Console.WriteLine(JArray.Parse(await response.Content.ReadAsStringAsync()));
            }
        }
    }
}
