using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Framework.Emails.EmailTypes
{
    public class EmailForRole
    {
        public string Subject { get; set; }
        public string Body { get; set; }
        public List<IFormFile> Attachments { get; set; }
    }
}
