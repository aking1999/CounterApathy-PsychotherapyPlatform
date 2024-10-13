using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class UserAccountInformation
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string LastActivity { get; set; }
        public DateTime? LastActivityDateTime { get; set; }
        public string CurrentIpAddress { get; set; }
        public DateTime? SignInDateTime { get; set; }
        public DateTime? RegistrationDateTime { get; set; }
        public string UserAgent { get; set; }
        public bool? IsCrawler { get; set; }

        public virtual AspNetUsers User { get; set; }
    }
}
