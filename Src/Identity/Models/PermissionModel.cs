namespace Identity.Models
{
    public class PermissionModel
    {
        public bool IsRedirect => RedirectUri != null;

        public string RedirectUri
        {
            get;
            set;
        }

        public string ClientId
        {
            get;
            set;
        }


        public bool ShowView => ViewModel != null;

        public ConsentModel ViewModel
        {
            get;
            set;
        }

        public bool HasValidationError => ValidationError != null;

        public string ValidationError
        {
            get;
            set;
        }
    }
}
