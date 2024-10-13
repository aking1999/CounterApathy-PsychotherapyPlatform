using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Notifications.NotificationTypes
{
    public class NotificationForRole
    {
        public string SenderUserId { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string Severity { get; set; }
        public DateTime? SendingDateTime { get; set; }
        public string Icon { get; set; }
        public bool? Important { get; set; }
    }
}
