using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Client
{
    public class Http
    {
        public static async Task<string> GetStringAsync(string url, string accessToken = null)
        {
            using (HttpClient client = new HttpClient())
            {
                if (!string.IsNullOrEmpty(accessToken))
                {
                    client.SetBearerToken(accessToken);
                }

                try
                {
                    HttpResponseMessage resp = await client.GetAsync(url);

                    if (resp.StatusCode == HttpStatusCode.OK)
                    {
                        return await resp.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        return resp.StatusCode.ToString();
                    }
                }
                catch (HttpRequestException e)
                {
                    return e.Message;
                }
            }
        }

        public static async Task<JArray> GetJArrayAsync(string url, string accessToken = null)
        {
            var value = await GetStringAsync(url, accessToken);

            try
            {
                return JArray.Parse(value);
            }
            catch
            {
                return JArray.Parse("[\"" + value + "\"]");
            }
        }
    }
}
