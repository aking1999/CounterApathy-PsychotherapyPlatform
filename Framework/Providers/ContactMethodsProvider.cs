using Database.Models;

namespace Framework.Providers
{
    public class ContactMethodsProvider
    {
        public static ContactMethods InPerson => new ContactMethods { Id = "in-person", Name = "Lično", Color = "#0000FF", Icon = "fal fa-user-friends" };
        public static ContactMethods GoogleMeet => new ContactMethods { Id = "google-meet", Name = "Google Meet", Color = "#DB4437", Icon = "ca-icon-google-meet" };
    }
}
