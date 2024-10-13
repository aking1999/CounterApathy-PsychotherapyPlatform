namespace Framework.Providers
{
    public class UserRoles
    {
        public static string None { get; } = null;
        public static string Client { get; } = "Client";
        public static string Therapist { get; } = "Therapist";
        public static string Admin { get; } = "Admin";

        public enum UserRoleTypes
        {
            None,
            Client,
            Therapist,
            Admin
        }
    }
}