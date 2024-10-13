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
    }
}
