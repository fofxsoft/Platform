namespace Identity.Models
{
    public class RedirectModel
    {
        public string RedirectUri
        {
            get;
            set;
        }

        public string ClientName
        {
            get;
            set;
        }

        public string SignOutIframeUrl
        {
            get;
            set;
        }

        public string LogoutId
        {
            get;
            set;
        }

        public bool TriggerExternalSignout => ExternalAuthenticationScheme != null;

        public string ExternalAuthenticationScheme
        {
            get;
            set;
        }
    }
}
