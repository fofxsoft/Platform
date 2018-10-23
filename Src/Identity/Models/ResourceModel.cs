using System.Collections.Generic;

namespace Identity.Models
{
    public class ResourceModel
    {
        public string Button
        {
            get;
            set;
        }

        public IEnumerable<string> ScopesConsented
        {
            get;
            set;
        }

        public bool RememberConsent
        {
            get;
            set;
        }

        public string ReturnUrl
        {
            get;
            set;
        }
    }
}
