using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class StripeCustomers
    {
        public string Id { get; set; }
        public string UserIdOrAnonymous { get; set; }
        public string Email { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public string PhoneNumber { get; set; }
    }
}
