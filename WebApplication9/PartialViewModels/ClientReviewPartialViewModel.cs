using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication9.PartialViewModels
{
    public class ClientReviewPartialViewModel
    {
        public string FullName { get; set; }
        public double StarsRating { get; set; }
        public string Comment { get; set; }
        public string RatingDate { get; set; }
    }
}
