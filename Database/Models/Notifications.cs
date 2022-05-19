using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class Notifications
    {
        public string Id { get; set; }
        public string SenderUserId { get; set; }
        public string ReceiverUserId { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string Severity { get; set; }
        public bool? Read { get; set; }
        public DateTime? SendingDateTime { get; set; }
        public string Icon { get; set; }

        public virtual AspNetUsers ReceiverUser { get; set; }
    }
}
