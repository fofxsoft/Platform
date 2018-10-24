using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;

namespace Identity.Models
{
    public class DiagnosticsModel
    {
        public DiagnosticsModel(AuthenticateResult result)
        {
            AuthenticateResult = result;

            if (result.Properties.Items.ContainsKey("client_list"))
            {
                Clients = JsonConvert.DeserializeObject<string[]>(Encoding.UTF8.GetString(Base64Url.Decode(result.Properties.Items["client_list"])));
            }
        }

        public AuthenticateResult AuthenticateResult
        {
            get;
        }

        public IEnumerable<string> Clients
        {
            get;
        } = new List<string>();
    }
}
