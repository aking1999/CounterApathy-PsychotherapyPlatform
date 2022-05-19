using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication9.Areas.Therapist.ViewModels
{
    public class WithdrawViewModel
    {
        [Required]
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [Required]
        public string Street { get; set; }

        [Required]
        [Display(Name = "House number")]
        public string HouseNumber { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        [Display(Name = "Postal code")]
        public string PostalCode { get; set; }

        [Required]
        public string Country { get; set; }

        [Required]
        [Display(Name = "Bank account number")]
        public string BankAccountNumber { get; set; }
        public string Amount { get; set; }

        public void Map(Database.Models.Therapists t)
        {
            Street = t.Street;
            HouseNumber = t.HouseNumber;
            City = t.City;
            PostalCode = t.PostalCode;
            Country = t.Country;
            Amount = t.Earnings.ToString();
        }
    }
}
