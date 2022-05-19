using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class HostedServicesInformation
    {
        public string Id { get; set; }
        public string Information { get; set; }
        public string InformationType { get; set; }
        public DateTime? ExecutionDateTime { get; set; }
    }
}
