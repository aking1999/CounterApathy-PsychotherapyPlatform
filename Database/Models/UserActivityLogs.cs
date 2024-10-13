using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class UserActivityLogs
    {
        public string Id { get; set; }
        public string UserIdOrAnonymous { get; set; }
        public string Area { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public string QueryDataJson { get; set; }
        public string IpAddress { get; set; }
        public DateTime? ActivityDateTime { get; set; }
        public long ActionExecutedMilliseconds { get; set; }
        public long ResultExecutedMilliseconds { get; set; }
        public string UserAgent { get; set; }
        public bool? IsCrawler { get; set; }
        public string MethodType { get; set; }
    }
}
