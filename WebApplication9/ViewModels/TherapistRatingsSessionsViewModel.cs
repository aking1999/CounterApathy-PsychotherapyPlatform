using Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication9.ViewModels
{
    public class TherapistRatingsSessionsViewModel
    {
        public string Id { get; set; }
        public string ProfilePhoto { get; set; }
        public string FullName { get; set; }
        private double rating;
        private string about;
        //public int NumberOfSessions { get; set; }
        public List<string> Specialties { get; set; }

        public double Rating
        {
            get
            {
                return rating;
            }

            set
            {
                try
                {
                    rating = Math.Ceiling(value);
                }
                catch (Exception)
                {
                    rating = value;
                }
            }
        }

        public string About
        {
            get
            {
                return about;
            }
            set
            {
                try
                {
                    if (value.Length > 57)
                        about = string.Concat(value.Take(57)) + "...";

                }
                catch (Exception)
                {
                    about = null;
                }
            }
        }

        public TherapistRatingsSessionsViewModel()
        {
            Specialties = new List<string>();
        }
    }
}
