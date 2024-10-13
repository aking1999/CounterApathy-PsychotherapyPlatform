using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication9.Areas.Admin.ViewModels
{
    public class ApplicationsViewModel
    {
        public string ApplicationId { get; set; }
        public string UserId { get; set; }
        public string ProfilePhoto { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string ApplicationDate { get; set; }
        public int? Accepted { get; set; }
    }
}
