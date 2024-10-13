using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Providers
{
    public class UnauthenticatedUserRoles
    {
        public static string Anonymous { get; } = "anonymous";
        public static string Unauthorized { get; } = "unauthorized";
    }
}
