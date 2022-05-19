using Database.Models;
using Framework.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication9.Areas.Admin.ViewModels
{
    public class WithdrawalViewModel
    {
        public string ProfilePhoto { get; set; }
        public string Id { get; set; }
        public string TherapistId { get; set; }
        public string TherapistAccountFirstName { get; set; }
        public string TherapistAccountLastName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Street { get; set; }
        public string HouseNumber { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public int Status { get; set; }
        public string BankAccountNumber { get; set; }
        public double Amount { get; set; }
        public string RequestDateTime { get; set; }
        public string AcceptDateTime { get; set; }

        public void Map(Withdrawals withdrawal)
        {
            Id = withdrawal.Id;
            TherapistId = withdrawal.TherapistId;
            FirstName = withdrawal.FirstName;
            LastName = withdrawal.LastName;
            Street = withdrawal.Street;
            HouseNumber = withdrawal.HouseNumber;
            City = withdrawal.City;
            PostalCode = withdrawal.PostalCode;
            Country = withdrawal.Country;
            Email = withdrawal.Email;
            PhoneNumber = withdrawal.PhoneNumber;
            Status = withdrawal.Status;
            BankAccountNumber = withdrawal.BankAccountNumber;
            Amount = withdrawal.Amount;
            RequestDateTime = withdrawal.RequestDateTime.ToString("dd/MMM/yyyy HH:mm");
            AcceptDateTime = withdrawal.AcceptDateTime.ToString("dd/MMM/yyyy HH:mm");
        }
    }
}
