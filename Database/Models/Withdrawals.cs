using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class Withdrawals
    {
        public string Id { get; set; }
        public string TherapistId { get; set; }
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
        public DateTime RequestDateTime { get; set; }
        public DateTime? AcceptDateTime { get; set; }

        public virtual Therapists Therapist { get; set; }
    }
}
