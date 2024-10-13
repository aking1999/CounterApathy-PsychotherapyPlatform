using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Helpers
{
    public class IgnoreRoutes
    {
        public static bool Contains(string route)
        {
            return new List<string>
            {
                "notifications/getunread",
                "notifications/getall",
                "notifications/getimportant",
                "notifications/setread",
                "notifications/setimportant",
                "notifications/setunimportant"
            }.Contains(route.ToLower());
        }
    }
}
