using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class Survey
    {
        public Survey()
        {
            AspNetUsers = new HashSet<AspNetUsers>();
        }

        public string Id { get; set; }
        public string ListOfProblems { get; set; }

        public virtual ICollection<AspNetUsers> AspNetUsers { get; set; }
    }
}
