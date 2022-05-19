using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication9.Areas.Admin.ViewModels
{
    public class AdminAddWebCreditUserViewModel
    {
        public string UserId { get; set; }
        public string ProfilePhoto { get; set; }
        public string FullName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string WebCredit { get; set; }
        public string Role { get; set; }
    }
}
