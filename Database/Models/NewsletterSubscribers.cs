using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class NewsletterSubscribers
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string NormalizedEmail { get; set; }
        public int NotifiedCount { get; set; }
        public string IpAddress { get; set; }
        public DateTime? LastNotifiedDateTime { get; set; }
        public DateTime? SubscribeDateTime { get; set; }
    }
}
